using Hi3Helper.Sophon.Helper;
using Hi3Helper.Sophon.Infos;
using Hi3Helper.Sophon.Structs;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable AccessToModifiedClosure

using TaskExtensions = Hi3Helper.Sophon.Helper.TaskExtensions;
using ZstdStream = ZstdNet.DecompressionStream;

namespace Hi3Helper.Sophon
{
    public enum SophonPatchMethod
    {
        CopyOver,
        DownloadOver,
        Patch,
        Remove
    }

    public partial class SophonPatchAsset
    {
        internal const int BufferSize = 4 << 10;

        public   SophonAsset       MainAssetInfo     { get; set; }
        public   SophonChunksInfo  PatchInfo         { get; set; }
        public   SophonPatchMethod PatchMethod       { get; set; }
        public   string            PatchNameSource   { get; set; }
        public   string            PatchHash         { get; set; }
        public   long              PatchOffset       { get; set; }
        public   long              PatchSize         { get; set; }
        public   long              PatchChunkLength  { get; set; }
        public   string            OriginalFilePath  { get; set; }
        public   string            OriginalFileHash  { get; set; }
        public   long              OriginalFileSize  { get; set; }
        public   string            TargetFilePath    { get; set; }
        public   string            TargetFileHash    { get; set; }
        public   long              TargetFileSize    { get; set; }

#nullable enable
        public async Task<bool> DownloadPatchAsync(HttpClient                  client,
                                                   string                      inputDir,
                                                   string                      patchOutputDir,
                                                   bool                        forceVerification    = false,
                                                   Action<long>?               downloadReadDelegate = null,
                                                   SophonDownloadSpeedLimiter? downloadSpeedLimiter = null,
                                                   CancellationToken           token                = default)
        {
            // Ignore SophonPatchMethod.Remove and SophonPatchMethod.DownloadOver assets
            if (PatchMethod is SophonPatchMethod.Remove or
                               SophonPatchMethod.DownloadOver)
            {
                return true;
            }

            FileInfo patchFilePathHashedFileInfo = this.GetLegacyOrHoyoPlayPatchChunkPath(patchOutputDir);
            if (!PatchNameSource.TryGetChunkXxh64Hash(out byte[] patchHash))
            {
                patchHash = Extension.HexToBytes(PatchHash.AsSpan());
            }

            SophonChunk patchAsChunk = new SophonChunk
            {
                ChunkHashDecompressed = patchHash,
                ChunkName             = PatchNameSource,
                ChunkOffset           = 0,
                ChunkOldOffset        = 0,
                ChunkSize             = PatchSize,
                ChunkSizeDecompressed = PatchSize
            };

            FileStream fileStream = patchFilePathHashedFileInfo
                .Open(new FileStreamOptions
                {
                    Mode    = FileMode.OpenOrCreate,
                    Access  = FileAccess.ReadWrite,
                    Share   = FileShare.ReadWrite,
                    Options = FileOptions.SequentialScan
                });

            try
            {
                bool isPatchUnmatched = fileStream.Length != PatchSize;
                if (forceVerification)
                {
                    isPatchUnmatched = patchHash.Length > 8 ?
                        !await patchAsChunk.CheckChunkMd5HashAsync(fileStream,
                                                                   true,
                                                                   token) :
                        !await patchAsChunk.CheckChunkXxh64HashAsync(fileStream,
                                                                     patchHash,
                                                                     true,
                                                                     token);
                }

                if (!isPatchUnmatched)
                {
#if DEBUG
                    this.PushLogDebug($"Skipping patch {PatchNameSource} for: {TargetFilePath}");
#endif
                    downloadReadDelegate?.Invoke(PatchSize);

                    return true;
                }

                if (fileStream.Length != 0)
                {
#if NET6_0_OR_GREATER
                    await fileStream.DisposeAsync();
#else
                    fileStream.Dispose();
#endif
                    fileStream = patchFilePathHashedFileInfo.Open(new FileStreamOptions
                    {
                        Mode    = FileMode.Create,
                        Access  = FileAccess.ReadWrite,
                        Share   = FileShare.ReadWrite
                    });
                }

                await InnerWriteChunkCopyAsync(client,
                                               fileStream,
                                               patchAsChunk,
                                               PatchInfo,
                                               PatchInfo,
                                               writeInfoDelegate: null,
                                               downloadInfoDelegate: (_, y) =>
                                               {
                                                   downloadReadDelegate?.Invoke(y);
                                               },
                                               downloadSpeedLimiter: downloadSpeedLimiter,
                                               token: token);

                return true;
            }
            finally
            {
#if NET6_0_OR_GREATER
                await fileStream.DisposeAsync();
#else
                fileStream.Dispose();
#endif
            }
        }

        private async
            Task
            InnerWriteChunkCopyAsync(HttpClient                  client,
                                     Stream                      outStream,
                                     SophonChunk                 chunk,
                                     SophonChunksInfo            currentSophonChunkInfo,
                                     SophonChunksInfo            altSophonChunkInfo,
                                     DelegateWriteStreamInfo?    writeInfoDelegate,
                                     DelegateWriteDownloadInfo?  downloadInfoDelegate,
                                     SophonDownloadSpeedLimiter? downloadSpeedLimiter,
                                     CancellationToken           token)
        {
            const int retryCount = TaskExtensions.DefaultRetryAttempt;
            int currentRetry = 0;

            long currentWriteOffset = 0;

#if !NOSTREAMLOCK
            if (outStream is FileStream fs)
            {
                fs.Lock(chunk.ChunkOffset, chunk.ChunkSizeDecompressed);
                this.PushLogDebug($"Locked data stream from pos: 0x{chunk.ChunkOffset:x8}" +
                $" -> L: 0x{chunk.ChunkSizeDecompressed:x8} for chunk: {chunk.ChunkName}" +
                $" by asset: {TargetFilePath}");
            }
#endif

            long written = 0;
            long thisInstanceDownloadLimitBase = downloadSpeedLimiter?.InitialRequestedSpeed ?? -1;
            Stopwatch currentStopwatch = Stopwatch.StartNew();

            double maximumBytesPerSecond;
            double bitPerUnit;

            CalculateBps();

            if (downloadSpeedLimiter != null)
            {
                downloadSpeedLimiter.CurrentChunkProcessingChangedEvent += UpdateChunkRangesCountEvent;
                downloadSpeedLimiter.DownloadSpeedChangedEvent += DownloadClientDownloadSpeedLimitChanged;
            }

            while (true)
            {
                bool allowDispose = false;
                HttpResponseMessage? httpResponseMessage = null;
                Stream? httpResponseStream = null;
                Stream? sourceStream = null;

                byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

                {
                    try
                    {
                        CancellationTokenSource innerTimeoutToken =
                            new CancellationTokenSource(TimeSpan.FromSeconds(TaskExtensions.DefaultTimeoutSec)
#if NET8_0_OR_GREATER
                                                      , TimeProvider.System
#endif
                                                       );
                        CancellationTokenSource cooperatedToken =
                            CancellationTokenSource.CreateLinkedTokenSource(token, innerTimeoutToken.Token);

#if DEBUG
                        this.PushLogDebug($"Init. by offset: 0x{chunk.ChunkOffset:x8}" +
                            $" -> L: 0x{chunk.ChunkSizeDecompressed:x8} for chunk: {chunk.ChunkName}");

#endif
                        outStream.Position = 0;
                        httpResponseMessage = await client
                            .GetChunkAndIfAltAsync(chunk.ChunkName,
                                                   currentSophonChunkInfo,
                                                   altSophonChunkInfo,
                                                   cooperatedToken.Token);
                        httpResponseStream = await httpResponseMessage
                                                  .EnsureSuccessStatusCode()
                                                  .Content
                                                  .ReadAsStreamAsync(
#if NET6_0_OR_GREATER
                                                                     cooperatedToken.Token
#endif
                                                                    );

                        sourceStream = httpResponseStream;
#if DEBUG
                        this.PushLogDebug($"[Complete init.] by offset: 0x{chunk.ChunkOffset:x8}" +
                            $" -> L: 0x{chunk.ChunkSizeDecompressed:x8} for chunk: {chunk.ChunkName}");
#endif

                        downloadSpeedLimiter?.IncrementChunkProcessedCount();
                        int read;
                        while ((read = await sourceStream.ReadAsync(
#if NET6_0_OR_GREATER
                                                                    buffer
#else
                                                                    buffer, 0, buffer.Length
#endif
                                                                  , cooperatedToken.Token)) > 0)
                        {

#if NET6_0_OR_GREATER
                            outStream.Write(buffer.AsSpan(0, read));
#else
                            outStream.Write(buffer, 0, read);
#endif

                            currentWriteOffset += read;
                            writeInfoDelegate?.Invoke(read);
                            downloadInfoDelegate?.Invoke(read, read);
                            written += read;

                            currentRetry = 0;
                            innerTimeoutToken.Dispose();
                            cooperatedToken.Dispose();

                            innerTimeoutToken =
                                new CancellationTokenSource(TimeSpan.FromSeconds(TaskExtensions.DefaultTimeoutSec)
#if NET8_0_OR_GREATER
                                                          , TimeProvider.System
#endif
                                                           );
                            cooperatedToken =
                                CancellationTokenSource.CreateLinkedTokenSource(token, innerTimeoutToken.Token);

                            await ThrottleAsync();
                        }

                        outStream.Position = 0;
                        Stream checkHashStream = outStream;

                        bool isHashVerified;
                        if (chunk.ChunkName.TryGetChunkXxh64Hash(out byte[] outHash))
                        {
                            isHashVerified =
                                await chunk.CheckChunkXxh64HashAsync(checkHashStream,
                                                                     outHash,
                                                                     true,
                                                                     cooperatedToken.Token);
                        }
                        else
                        {
                            if (PatchInfo.IsUseCompression)
                            {
                                checkHashStream = new ZstdStream(checkHashStream);
                            }

                            isHashVerified =
                                await chunk.CheckChunkMd5HashAsync(checkHashStream,
                                                                   true,
                                                                   cooperatedToken.Token);
                        }

                        if (!isHashVerified)
                        {
                            writeInfoDelegate?.Invoke(-chunk.ChunkSizeDecompressed);
                            downloadInfoDelegate?.Invoke(-chunk.ChunkSizeDecompressed, 0);
                            this.PushLogWarning("Output data seems to be corrupted at transport.\r\n" +
                                $"Restarting download for chunk: {chunk.ChunkName} | 0x{chunk.ChunkOffset:x8}" +
                                $" -> L: 0x{chunk.ChunkSizeDecompressed:x8} for: {TargetFilePath}");
                            continue;
                        }

#if DEBUG
                        this.PushLogDebug($"Download completed! Chunk: {chunk.ChunkName}" +
                            $" | 0x{chunk.ChunkOffset:x8} -> L: 0x{chunk.ChunkSizeDecompressed:x8}" +
                            $" for: {TargetFilePath}");
#endif
                        return;
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        allowDispose = true;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if (currentRetry < retryCount)
                        {
                            writeInfoDelegate?.Invoke(-currentWriteOffset);
                            downloadInfoDelegate?.Invoke(-currentWriteOffset, 0);
                            currentWriteOffset = 0;
                            currentRetry++;

                            this.PushLogWarning("An error has occurred while" +
                                $" downloading chunk: {chunk.ChunkName}" +
                                $" | 0x{chunk.ChunkOffset:x8} -> L: 0x{chunk.ChunkSizeDecompressed:x8}" +
                                $" for: {TargetFilePath}\r\n{ex}");
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                            continue;
                        }

                        allowDispose = true;
                        this.PushLogError($"An unhandled error has occurred while downloading chunk:" +
                            $" {chunk.ChunkName} | 0x{chunk.ChunkOffset:x8} ->" +
                            $" L: 0x{chunk.ChunkSizeDecompressed:x8} for: {TargetFilePath}\r\n{ex}");
                        throw;
                    }
                    finally
                    {
                        if (allowDispose)
                        {
                            httpResponseMessage?.Dispose();
#if NET6_0_OR_GREATER
                            if (httpResponseStream != null)
                            {
                                await httpResponseStream.DisposeAsync();
                            }

                            if (sourceStream != null)
                            {
                                await sourceStream.DisposeAsync();
                            }
#else
                            sourceStream?.Dispose();
                            httpResponseStream?.Dispose();
#endif
                        }

                        downloadSpeedLimiter?.DecrementChunkProcessedCount();
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
            }

            void CalculateBps()
            {
                if (thisInstanceDownloadLimitBase <= 0)
                {
                    thisInstanceDownloadLimitBase = -1;
                }
                else
                {
                    thisInstanceDownloadLimitBase = Math.Max(64 << 10, thisInstanceDownloadLimitBase);
                }

#if NET6_0_OR_GREATER
                double threadNum = Math.Clamp(downloadSpeedLimiter?.CurrentChunkProcessing ?? 1, 1, 16 << 10);
#else
                double threadNum = downloadSpeedLimiter?.CurrentChunkProcessing ?? 1;
                threadNum = threadNum switch
                {
                    < 1 => 1,
                    > 16 << 10 => 16 << 10,
                    _ => threadNum
                };
#endif
                maximumBytesPerSecond = thisInstanceDownloadLimitBase / threadNum;
                bitPerUnit            = 940 - (threadNum - 2) / (16 - 2) * 400;
            }

            void DownloadClientDownloadSpeedLimitChanged(object? sender, long e)
            {
                thisInstanceDownloadLimitBase = e == 0 ? -1 : e;
                CalculateBps();
            }

            void UpdateChunkRangesCountEvent(object? sender, int e)
            {
                CalculateBps();
            }

            async Task ThrottleAsync()
            {
                // Make sure the buffer isn't empty.
                if (maximumBytesPerSecond <= 0 || written <= 0)
                {
                    return;
                }

                long elapsedMilliseconds = currentStopwatch.ElapsedMilliseconds;

                if (elapsedMilliseconds > 0)
                {
                    // Calculate the current bps.
                    double bps = written * bitPerUnit / elapsedMilliseconds;

                    // If the bps are more then the maximum bps, try to throttle.
                    if (bps > maximumBytesPerSecond)
                    {
                        // Calculate the time to sleep.
                        double wakeElapsed = written * bitPerUnit / maximumBytesPerSecond;
                        double toSleep     = wakeElapsed - elapsedMilliseconds;

                        if (toSleep > 1)
                        {
                            // The time to sleep is more than a millisecond, so sleep.
                            await Task.Delay(TimeSpan.FromMilliseconds(toSleep), token);

                            // A sleep has been done, reset.
                            currentStopwatch.Restart();

                            written = 0;
                        }
                    }
                }
            }
        }
    }
}
