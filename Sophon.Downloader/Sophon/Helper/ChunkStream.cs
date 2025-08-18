using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
#if NET6_0_OR_GREATER
using System.Buffers;
#endif
// ReSharper disable IdentifierTypo
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace Hi3Helper.Sophon;

public sealed class ChunkStream : Stream
{
    private readonly Stream _stream;
    private          long   Start       { get; }
    private          long   End         { get; }
    private          long   Size        { get => End - Start; }
    private          long   CurPos      { get; set; }
    private          long   Remain      { get => Size - CurPos; }
    private          bool   IsDisposing { get; }

    public ChunkStream(Stream stream,
                       long   start,
                       long   end,
                       bool   isDisposing = false)
    {
        _stream = stream;

        if (_stream.Length == 0)
        {
            throw new Exception("The stream must not have 0 bytes!");
        }

        if (_stream.Length < start || end > _stream.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(stream));
        }

        _stream.Position = start;
        Start = start;
        End = end;
        CurPos = 0;
        IsDisposing = isDisposing;
    }

    ~ChunkStream() => Dispose(IsDisposing);

#if NET6_0_OR_GREATER
    public override int Read(Span<byte> buffer)
    {
        if (Remain == 0) return 0;

        int toSlice = (int)(buffer.Length > Remain ? Remain : buffer.Length);
        _stream.Position = Start + CurPos;
        int read = _stream.Read(buffer[..toSlice]);
        CurPos += read;

        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte>      buffer,
                                                   CancellationToken token = default)
    {
        if (Remain == 0) return 0;

        int toSlice = (int)(buffer.Length > Remain ? Remain : buffer.Length);
        _stream.Position = Start + CurPos;
        int read = await _stream.ReadAsync(buffer[..toSlice], token);
        CurPos += read;

        return read;
    }
#endif

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (Remain == 0) return 0;

        int toRead = (int)(Remain < count ? Remain : count);
        _stream.Position = Start + CurPos;
        int read = _stream.Read(buffer, offset, toRead);
        CurPos += read;
        return read;
    }

    public override async Task<int> ReadAsync(byte[]            buffer,
                                              int               offset,
                                              int               count,
                                              CancellationToken token)
    {
        if (Remain == 0) return 0;

        int toRead = (int)(Remain < count ? Remain : count);
        CurPos += toRead;
        _stream.Position = Start + CurPos;

        return await _stream.ReadAsync(
#if NET6_0_OR_GREATER
            buffer.AsMemory(offset, toRead)
#else
            buffer, offset, toRead
#endif
            , token);
    }

#if NET6_0_OR_GREATER
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (Remain == 0) return;

        int toSlice = (int)(buffer.Length > Remain ? Remain : buffer.Length);
        CurPos += toSlice;

        _stream.Write(buffer[..toSlice]);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
                                               CancellationToken    token = default)
    {
        if (Remain == 0) return;

        int toSlice = (int)(buffer.Length > Remain ? Remain : buffer.Length);
        CurPos += toSlice;

        await _stream.WriteAsync(buffer[..toSlice], token);
    }
#endif

    public override void Write(byte[] buffer,
                               int    offset,
                               int    count)
    {
        int toRead = (int)(Remain < count ? Remain : count);
        int toOffset = offset > Remain ? 0 : offset;
        _stream.Position += toOffset;
        CurPos += toOffset + toRead;

        _stream.Write(buffer, offset, toRead);
    }

    public override async Task WriteAsync(byte[]            buffer,
                                          int               offset,
                                          int               count,
                                          CancellationToken token)
    {
        int toRead = (int)(Remain < count ? Remain : count);
        int toOffset = offset > Remain ? 0 : offset;
        _stream.Position += toOffset;
        CurPos += toOffset + toRead;

        await _stream.WriteAsync(
#if NET6_0_OR_GREATER
            buffer.AsMemory(offset, toRead)
#else
            buffer, offset, toRead
#endif
            ,
            token);
    }

#if NET6_0_OR_GREATER
    public override void CopyTo(Stream destination,
                                int    bufferSize)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            int read;
            while ((read = Read(buffer)) > 0)
            {
                destination.Write(buffer.AsSpan(0, read));
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public override async Task CopyToAsync(Stream            destination,
                                           int               bufferSize,
                                           CancellationToken cancellationToken)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try
        {
            int read;
            while ((read = await ReadAsync(buffer, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
#endif

    public override bool CanRead
    {
        get { return _stream.CanRead; }
    }

    public override bool CanSeek
    {
        get { return _stream.CanSeek; }
    }

    public override bool CanWrite
    {
        get { return _stream.CanWrite; }
    }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override long Length
    {
        get { return Size; }
    }

    public override long Position
    {
        get
        {
            return CurPos;
        }
        set
        {
            if (value > Size)
            {
                throw new IndexOutOfRangeException();
            }

            CurPos = value;
            _stream.Position = CurPos + Start;
        }
    }

    public override long Seek(long       offset,
                              SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                {
                    return offset > Size
                        ? throw new ArgumentOutOfRangeException(nameof(offset))
                        : _stream.Seek(offset + Start, SeekOrigin.Begin) - Start;
                }
            case SeekOrigin.Current:
                {
                    long pos = _stream.Position - Start;
                    if (pos + offset > Size)
                    {
                        throw new ArgumentOutOfRangeException(nameof(offset));
                    }
                    return _stream.Seek(offset, SeekOrigin.Current) - Start;
                }
            default:
                {
                    _stream.Position = End;
                    _stream.Position -= offset;

                    return Position;
                }
        }
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) base.Dispose(true);
        if (IsDisposing) _stream.Dispose();
    }
}