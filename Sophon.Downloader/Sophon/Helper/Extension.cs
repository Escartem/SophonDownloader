using Google.Protobuf;
using Hi3Helper.Sophon.Infos;
using Hi3Helper.Sophon.Structs;
using System;
using System.Buffers;
#if NETSTANDARD2_0_OR_GREATER
using System.Collections.Generic;
#endif
using System.IO;
using System.IO.Hashing;
using System.Net.Http;
using System.Runtime.InteropServices;

#if !NET9_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable UseCollectionExpression
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable IdentifierTypo
// ReSharper disable EntityNameCapturedOnly.Global

using ZstdStream = ZstdNet.DecompressionStream;
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo

namespace Hi3Helper.Sophon.Helper
{
    internal static class Extension
    {
#if !NET9_0_OR_GREATER
        private static readonly byte[] LookupFromHexTable = new byte[] {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0,   1,
            2,   3,   4,   5,   6,   7,   8,   9,   255, 255,
            255, 255, 255, 255, 255, 10,  11,  12,  13,  14,
            15,  255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 10,  11,  12,
            13,  14,  15
        };

        private static readonly byte[] LookupFromHexTable16 = new byte[] {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0,   16,
            32,  48,  64,  80,  96,  112, 128, 144, 255, 255,
            255, 255, 255, 255, 255, 160, 176, 192, 208, 224,
            240, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 160, 176, 192,
            208, 224, 240
        };

        internal static unsafe byte[] HexToBytes(ReadOnlySpan<char> source)
        {
            if (source.IsEmpty) return [];
            if (source.Length % 2 == 1)
                throw new IndexOutOfRangeException($"The length of the {nameof(source)} must be even!");

            int index = 0;
            int len = source.Length >> 1;

            fixed (char* sourceRef = &source[0])
            {
                if (*(int*)sourceRef == 7864368)
                {
                    if (source.Length == 2)
                    {
                        throw new InvalidOperationException();
                    }

                    index += 2;
                    len -= 1;
                }

                // ReSharper disable once TooWideLocalVariableScope
                byte add;
                byte[] result = new byte[len];

                fixed (byte* hiRef = &LookupFromHexTable16[0])
                {
                    fixed (byte* lowRef = &LookupFromHexTable[0])
                    {
                        fixed (byte* resultRef = &result[0])
                        {
                            char* s = &sourceRef[index];
                            byte* r = &resultRef[0];

                            while (*s != 0)
                            {
                                if (*s > 102 || (*r = hiRef[*s++]) == 255 || *s > 102 || (add = lowRef[*s++]) == 255)
                                {
                                    throw new InvalidOperationException();
                                }
                                *r++ += add;
                            }
                            return result;
                        }
                    }
                }
            }
        }
#else
        internal static byte[] HexToBytes(ReadOnlySpan<char> source)
            => Convert.FromHexString(source);
#endif

#if !NET9_0_OR_GREATER
        private static readonly uint[] Lookup32Unsafe =
        {
            0x300030, 0x310030, 0x320030, 0x330030, 0x340030, 0x350030, 0x360030, 0x370030, 0x380030, 0x390030,
            0x610030, 0x620030,
            0x630030, 0x640030, 0x650030, 0x660030, 0x300031, 0x310031, 0x320031, 0x330031, 0x340031, 0x350031,
            0x360031, 0x370031,
            0x380031, 0x390031, 0x610031, 0x620031, 0x630031, 0x640031, 0x650031, 0x660031, 0x300032, 0x310032,
            0x320032, 0x330032,
            0x340032, 0x350032, 0x360032, 0x370032, 0x380032, 0x390032, 0x610032, 0x620032, 0x630032, 0x640032,
            0x650032, 0x660032,
            0x300033, 0x310033, 0x320033, 0x330033, 0x340033, 0x350033, 0x360033, 0x370033, 0x380033, 0x390033,
            0x610033, 0x620033,
            0x630033, 0x640033, 0x650033, 0x660033, 0x300034, 0x310034, 0x320034, 0x330034, 0x340034, 0x350034,
            0x360034, 0x370034,
            0x380034, 0x390034, 0x610034, 0x620034, 0x630034, 0x640034, 0x650034, 0x660034, 0x300035, 0x310035,
            0x320035, 0x330035,
            0x340035, 0x350035, 0x360035, 0x370035, 0x380035, 0x390035, 0x610035, 0x620035, 0x630035, 0x640035,
            0x650035, 0x660035,
            0x300036, 0x310036, 0x320036, 0x330036, 0x340036, 0x350036, 0x360036, 0x370036, 0x380036, 0x390036,
            0x610036, 0x620036,
            0x630036, 0x640036, 0x650036, 0x660036, 0x300037, 0x310037, 0x320037, 0x330037, 0x340037, 0x350037,
            0x360037, 0x370037,
            0x380037, 0x390037, 0x610037, 0x620037, 0x630037, 0x640037, 0x650037, 0x660037, 0x300038, 0x310038,
            0x320038, 0x330038,
            0x340038, 0x350038, 0x360038, 0x370038, 0x380038, 0x390038, 0x610038, 0x620038, 0x630038, 0x640038,
            0x650038, 0x660038,
            0x300039, 0x310039, 0x320039, 0x330039, 0x340039, 0x350039, 0x360039, 0x370039, 0x380039, 0x390039,
            0x610039, 0x620039,
            0x630039, 0x640039, 0x650039, 0x660039, 0x300061, 0x310061, 0x320061, 0x330061, 0x340061, 0x350061,
            0x360061, 0x370061,
            0x380061, 0x390061, 0x610061, 0x620061, 0x630061, 0x640061, 0x650061, 0x660061, 0x300062, 0x310062,
            0x320062, 0x330062,
            0x340062, 0x350062, 0x360062, 0x370062, 0x380062, 0x390062, 0x610062, 0x620062, 0x630062, 0x640062,
            0x650062, 0x660062,
            0x300063, 0x310063, 0x320063, 0x330063, 0x340063, 0x350063, 0x360063, 0x370063, 0x380063, 0x390063,
            0x610063, 0x620063,
            0x630063, 0x640063, 0x650063, 0x660063, 0x300064, 0x310064, 0x320064, 0x330064, 0x340064, 0x350064,
            0x360064, 0x370064,
            0x380064, 0x390064, 0x610064, 0x620064, 0x630064, 0x640064, 0x650064, 0x660064, 0x300065, 0x310065,
            0x320065, 0x330065,
            0x340065, 0x350065, 0x360065, 0x370065, 0x380065, 0x390065, 0x610065, 0x620065, 0x630065, 0x640065,
            0x650065, 0x660065,
            0x300066, 0x310066, 0x320066, 0x330066, 0x340066, 0x350066, 0x360066, 0x370066, 0x380066, 0x390066,
            0x610066, 0x620066,
            0x630066, 0x640066, 0x650066, 0x660066
        };

        private static readonly unsafe uint* Lookup32UnsafeP =
            (uint*)GCHandle.Alloc(Lookup32Unsafe, GCHandleType.Pinned).AddrOfPinnedObject();
#endif

        internal static
#if !NET9_0_OR_GREATER
            unsafe
#endif
            string BytesToHex(ReadOnlySpan<byte> bytes)
#if !NET9_0_OR_GREATER
        {
            uint* lookupP = Lookup32UnsafeP;
            char* result  = stackalloc char[bytes.Length * 2];
            fixed (byte* bytesP = bytes)
            {
                uint* resultP2 = (uint*)result;
                for (int i = 0; i < bytes.Length; i++)
                {
                    resultP2[i] = lookupP[bytesP[i]];
                }
            }

            return new string(result, 0, bytes.Length * 2);
        }
#else
            => Convert.ToHexStringLower(bytes);
#endif

        internal static async
#if NET6_0_OR_GREATER
            ValueTask<bool>
#else
            Task<bool>
#endif
            CheckChunkXxh64HashAsync(this SophonChunk  chunk,
                                     Stream            outStream,
                                     byte[]            chunkXxh64Hash,
                                     bool              isSingularStream,
                                     CancellationToken token)
        {
            XxHash64 hash = new XxHash64();

            await hash.AppendAsync(isSingularStream ? outStream : GetChunkStream(), token);
            bool isHashMatch = hash.GetHashAndReset()
                                   .AsSpan()
                                   .SequenceEqual(chunkXxh64Hash);

            return isHashMatch;

            Stream GetChunkStream()
            {
                long chunkPosStart = chunk.ChunkOffset;
                long chunkPosEnd   = chunkPosStart + chunk.ChunkSizeDecompressed;
                return new ChunkStream(outStream, chunkPosStart, chunkPosEnd);
            }
        }

        internal static async
#if NET6_0_OR_GREATER
            ValueTask<bool>
#else
            Task<bool>
#endif
            CheckChunkMd5HashAsync(this SophonChunk  chunk,
                                   Stream            outStream,
                                   bool              isSingularStream,
                                   CancellationToken token)
        {
            using MD5 hash       = MD5.Create();
            byte[]    resultHash = await hash.ComputeHashAsync(isSingularStream ? outStream : GetChunkStream(), token);

            bool isHashMatch = resultHash
                              .AsSpan()
                              .SequenceEqual(chunk.ChunkHashDecompressed);

            return isHashMatch;

            Stream GetChunkStream()
            {
                long chunkPosStart = chunk.ChunkOffset;
                long chunkPosEnd   = chunkPosStart + chunk.ChunkSizeDecompressed;
                return new ChunkStream(outStream, chunkPosStart, chunkPosEnd);
            }
        }

        internal static unsafe string GetChunkStagingFilenameHash(this SophonChunk chunk,
                                                                  SophonAsset      asset)
        {
            string concatName = $"{asset.AssetName}${asset.AssetHash}${chunk.ChunkName}";
            byte[] concatNameBuffer = ArrayPool<byte>.Shared.Rent(concatName.Length);
            byte[] hash = ArrayPool<byte>.Shared.Rent(16);

            fixed (char* concatNamePtr = concatName)
            {
                fixed (byte* concatNameBufferPtr = &concatNameBuffer[0])
                {
                    try
                    {
                        int written = Encoding.UTF8.GetBytes(concatNamePtr, concatName.Length, concatNameBufferPtr,
                                                             concatNameBuffer.Length);
                        ReadOnlySpan<byte> writtenBytes = concatNameBuffer.AsSpan(0, written);
                        XxHash128.Hash(writtenBytes, hash);
                        return BytesToHex(hash);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(concatNameBuffer);
                        ArrayPool<byte>.Shared.Return(hash);
                    }
                }
            }
        }

        internal static bool TryGetChunkXxh64Hash(
            this string fileName,
            out  byte[] outHash)
        {
#if NET8_0_OR_GREATER
            return TryGetChunkXxh64Hash(fileName.AsSpan(), out outHash);
#else
            outHash = null;
            string[] splits = fileName.Split('_');
            if (splits.Length != 2)
                return false;

            if (splits[0].Length != 16)
                return false;

            outHash = HexToBytes(splits[0].AsSpan());
            return true;
#endif
        }

        internal static bool TryGetChunkXxh64Hash(
            ReadOnlySpan<char> fileName,
            out byte[]         outHash)
        {
            outHash = null;
            Span<Range> ranges = stackalloc Range[2];
            if (fileName.Split(ranges, '_') != 2)
            {
                return false;
            }

            ReadOnlySpan<char> nameSpan       = fileName;
            ReadOnlySpan<char> chunkXxh64Hash = nameSpan[ranges[0]];

            if (chunkXxh64Hash.Length != 16)
            {
                return false;
            }

            outHash = HexToBytes(chunkXxh64Hash);
            return true;
        }

        internal static void EnsureOrThrowOutputDirectoryExistence(this SophonAsset asset,
                                                                   string           outputDirPath)
        {
            if (string.IsNullOrEmpty(outputDirPath))
            {
                throw new ArgumentNullException(nameof(asset), "Directory path cannot be empty or null!");
            }

            if (!Directory.Exists(outputDirPath))
            {
                throw new DirectoryNotFoundException($"Directory path: {outputDirPath} does not exist!");
            }
        }

        internal static void EnsureOrThrowChunksState(this SophonAsset asset)
        {
            if (asset.Chunks == null)
            {
                throw new NullReferenceException("This asset does not have chunk(s)!");
            }
        }

        internal static void EnsureOrThrowStreamState(this SophonAsset asset,
                                                      Stream           outStream)
        {
            if (outStream == null)
            {
                throw new NullReferenceException("Output stream cannot be null!");
            }

            if (!outStream.CanRead)
            {
                throw new NotSupportedException("Output stream must be readable!");
            }

            if (!outStream.CanWrite)
            {
                throw new NotSupportedException("Output stream must be writable!");
            }

            if (!outStream.CanSeek)
            {
                throw new NotSupportedException("Output stream must be seekable!");
            }
        }

        internal static FileInfo CreateFileInfo(this string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Directory is { Exists: false } directoryInfo)
            {
                directoryInfo.Create();
            }

            if (fileInfo.Exists)
            {
                fileInfo.IsReadOnly = false;
            }

            return fileInfo;
        }

        internal static async Task<HttpResponseMessage>
            GetChunkAndIfAltAsync(this HttpClient   httpClient,
                                  string            chunkName,
                                  SophonChunksInfo  currentSophonChunkInfo,
                                  SophonChunksInfo  altSophonChunkInfo,
                                  CancellationToken token = default)
        {
            // Concat the string
            string url = currentSophonChunkInfo.ChunksBaseUrl.TrimEnd('/') + '/' + chunkName;

            bool isDispose = false;
            HttpResponseMessage httpResponseMessage = null;
            try
            {
                // Try to get the HttpResponseMessage
                httpResponseMessage = await httpClient.GetAsync(
                    url,
                    HttpCompletionOption.ResponseHeadersRead,
                    token);

                // If it fails and does have the alt SophonChunksInfo, then try to return
                // with the alt one
                if (httpResponseMessage.IsSuccessStatusCode || altSophonChunkInfo == null)
                {
                    return httpResponseMessage;
                }

                // Dispose the previous HttpResponseMessage
                isDispose = true;

                // Return another one from alt
                return await httpClient.GetChunkAndIfAltAsync(chunkName,
                                                              altSophonChunkInfo,
                                                              null,
                                                              token);

                // If it doesn't fail or has no alt even though it's failing or not, then return
            }
            finally
            {
                // If the old one is asked to be disposed, then do it.
                if (isDispose)
                    httpResponseMessage.Dispose();
            }
        }

        internal static (T Data, bool IsSuccess) TryReadFromCached<T>(
            SophonManifestInfo manifestInfo,
            out string         localMetadataPath,
            MessageParser<T>   messageParser)
            where T : IMessage<T>
        {
            const string metadataMarkName = "manifest_";
            localMetadataPath = string.Empty;

        #pragma warning disable CA1510
            if (manifestInfo == null)
        #pragma warning restore CA1510
            {
                throw new ArgumentNullException(nameof(manifestInfo));
            }

            string currentUserDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string localLowDir = Path.Combine(currentUserDir, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                                                  @"AppData\LocalLow\CollapseLauncher\_sophonMetadataCache" :
                                                  $"sophon{Path.PathSeparator}metadatacache");

            Directory.CreateDirectory(localLowDir);

            string metadataFilename = Path.GetFileName(manifestInfo.ManifestFileUrl ?? "");
            string cachedFilepath   = Path.Combine(localLowDir, metadataFilename);
            localMetadataPath = cachedFilepath;

            if (!File.Exists(cachedFilepath) ||
                !metadataFilename.StartsWith(metadataMarkName, StringComparison.OrdinalIgnoreCase))
            {
                return (default, false);
            }

            ReadOnlySpan<char> metadataOnlyHashName = metadataFilename.AsSpan(metadataMarkName.Length);
            if (!TryGetChunkXxh64Hash(metadataOnlyHashName, out byte[] metadataXxh64Hash))
            {
                return (default, false);
            }

            using FileStream stream = File.Open(cachedFilepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (stream.Length == 0)
            {
                return (default, false);
            }

            byte[]     readBytes  = ArrayPool<byte>.Shared.Rent((int)manifestInfo.ManifestSize);
            Span<byte> bufferHash = stackalloc byte[8];
            try
            {
                int offset = 0;
                int read;
                while ((read = stream.Read(readBytes.AsSpan(offset))) > 0)
                {
                    offset += read;
                }

                XxHash64.TryHash(readBytes.AsSpan(0, offset), bufferHash, out _);
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (!bufferHash.SequenceEqual(metadataXxh64Hash))
                {
                    return (default, false);
                }

                using Stream parserStream = manifestInfo.IsUseCompression ?
                    GetDecompressorStream(stream) :
                    stream;

                return (messageParser.ParseFrom(parserStream), true);
            }
            // ReSharper disable once RedundantCatchClause
            catch
            {
#if DEBUG
                throw;
#else
                return (default, false);
#endif
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(readBytes);
            }

            ZstdStream GetDecompressorStream(Stream sourceStream)
            {
                sourceStream.Position = 0;
                return new ZstdStream(sourceStream);
            }
        }

#if NETSTANDARD2_0_OR_GREATER
        internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable) => new HashSet<T>(enumerable);
#endif

        internal static async Task<T> ReadProtoFromManifestInfo<T>(this HttpClient    httpClient,
                                                                   SophonManifestInfo manifestInfo,
                                                                   MessageParser<T>   messageParser,
                                                                   CancellationToken  innerToken)
            where T : IMessage<T>
        {
            (T cachedData, bool isSuccess) = TryReadFromCached(manifestInfo,
                                                               out string localMetadataPath,
                                                               messageParser);
            if (isSuccess)
            {
                return cachedData;
            }

            FileInfo localMetadataInfo = new FileInfo(localMetadataPath);
            if (localMetadataInfo.Exists)
            {
                localMetadataInfo.IsReadOnly = false;
            }

            using (HttpResponseMessage httpResponseMessage = await httpClient
                .GetAsync(manifestInfo.ManifestFileUrl,
                          HttpCompletionOption.ResponseHeadersRead,
                          innerToken
                         ))
#if NET6_0_OR_GREATER
            await
#endif
            using (Stream manifestProtoStream = await httpResponseMessage
                                                     .EnsureSuccessStatusCode()
                                                     .Content
                                                     .ReadAsStreamAsync(
#if NET6_0_OR_GREATER
                                                                        innerToken
#endif
                                                                       ))
            {
#if NET6_0_OR_GREATER
                await
#endif
                using FileStream manifestLocalStream = localMetadataInfo.Create();
                await manifestProtoStream.CopyToAsync(manifestLocalStream, innerToken);

                manifestLocalStream.Position = 0;

#if NET6_0_OR_GREATER
                await
#endif
                using (Stream decompressedProtoStream = manifestInfo.IsUseCompression ?
                           new ZstdStream(manifestLocalStream) :
                           manifestLocalStream)
                {
                    return await Task<T>.Factory.StartNew(() => messageParser.ParseFrom(decompressedProtoStream),
                                                          innerToken,
                                                          TaskCreationOptions.DenyChildAttach,
                                                          TaskScheduler.Default);
                }
            }
        }

        internal static FileInfo GetLegacyOrHoyoPlayPatchChunkPath(this SophonPatchAsset asset, string patchOutputDir)
        {
            ArgumentNullException.ThrowIfNull(asset, nameof(asset));

            ArgumentException.ThrowIfNullOrEmpty(patchOutputDir,        nameof(patchOutputDir));
            ArgumentException.ThrowIfNullOrEmpty(asset.PatchNameSource, nameof(asset.PatchNameSource));

            string nativeChunkPath = Path.Combine(patchOutputDir, asset.PatchNameSource);

            ReadOnlySpan<char> outputDirParent = Path.GetDirectoryName(patchOutputDir.AsSpan());
            // ReSharper disable once StringLiteralTypo
            string hoyoPlayChunkDir  = Path.Join(outputDirParent, "ldiff");
            string hoyoPlayChunkPath = Path.Combine(hoyoPlayChunkDir, asset.PatchNameSource);

            string legacyChunkDir  = Path.Join(outputDirParent, "chunk_collapse");
            string legacyChunkPath = Path.Combine(legacyChunkDir, asset.PatchNameSource);

            FileInfo hoyoPlayChunkInfo = hoyoPlayChunkPath.CreateFileInfo();
            FileInfo legacyChunkInfo   = legacyChunkPath.CreateFileInfo();
            FileInfo nativeChunkInfo   = nativeChunkPath.CreateFileInfo();

            // Check for HoYoPlay LDiff path first
            if (hoyoPlayChunkInfo.Exists && hoyoPlayChunkInfo.Length == asset.PatchSize)
            {
                return hoyoPlayChunkInfo;
            }

            // If none, check for legacy path
            if (legacyChunkInfo.Exists && legacyChunkInfo.Length == asset.PatchSize)
            {
                return legacyChunkInfo;
            }

            // Otherwise, use native path info
            return nativeChunkInfo;
        }
    }
}