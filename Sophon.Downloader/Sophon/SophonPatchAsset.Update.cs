using Hi3Helper.Sophon.Helper;
using Hi3Helper.Sophon.Structs;
using SharpHDiffPatch.Core;
using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable AccessToModifiedClosure

#nullable enable
namespace Hi3Helper.Sophon
{
    public partial class SophonPatchAsset
    {
        private static ReadOnlySpan<byte> HDiffPatchMagic => "HDIFF"u8;
        private const  string             BlankFileMd5Hash = "d41d8cd98f00b204e9800998ecf8427e";
        private const  string             BlankFileDiffExt = ".diff_ref";

        public async Task ApplyPatchUpdateAsync(HttpClient                  client,
                                                string                      inputDir,
                                                string                      patchOutputDir,
                                                bool                        removeOldAssets       = true,
                                                Action<long>?               downloadReadDelegate  = null,
                                                Action<long>?               diskWriteDelegate     = null,
                                                SophonDownloadSpeedLimiter? downloadSpeedLimiter  = null,
                                                CancellationToken           token                 = default)
        {
            int retry = 5;
        StartOver:
            bool isRemove     = SophonPatchMethod.Remove == PatchMethod;
            bool isCopyOver   = SophonPatchMethod.CopyOver == PatchMethod;
            bool isPatchHDiff = SophonPatchMethod.Patch == PatchMethod;
            string sourceFileNameToCheck = PatchMethod switch
            {
                SophonPatchMethod.Remove => OriginalFilePath,
                SophonPatchMethod.DownloadOver => TargetFilePath,
                SophonPatchMethod.Patch => OriginalFilePath,
                SophonPatchMethod.CopyOver => TargetFilePath,
                _ => throw new InvalidOperationException($"Unsupported patch method: {PatchMethod}")
            };
            string sourceFilePathToCheck = Path.Combine(inputDir, sourceFileNameToCheck);

            if (isRemove)
            {
                if (!removeOldAssets)
                {
                    return;
                }

                FileInfo removableAssetFileInfo = sourceFilePathToCheck.CreateFileInfo();
                PerformPatchAssetRemove(removableAssetFileInfo);
                return;
            }

            if (PatchMethod is SophonPatchMethod.DownloadOver or
                               SophonPatchMethod.CopyOver or
                               SophonPatchMethod.Patch &&
                await IsFilePatched(inputDir, token))
            {
                diskWriteDelegate?.Invoke(TargetFileSize);
                return;
            }

            if (!isCopyOver)
            {
                string sourceFileHashString = PatchMethod switch
                {
                    SophonPatchMethod.Remove => OriginalFileHash,
                    SophonPatchMethod.DownloadOver => TargetFileHash,
                    SophonPatchMethod.Patch => OriginalFileHash,
                    _ => throw new InvalidOperationException($"Unsupported patch method: {PatchMethod}")
                };

                long sourceFileSizeToCheck = PatchMethod switch
                {
                    SophonPatchMethod.Remove => OriginalFileSize,
                    SophonPatchMethod.DownloadOver => TargetFileSize,
                    SophonPatchMethod.Patch => OriginalFileSize,
                    _ => throw new InvalidOperationException($"Unsupported patch method: {PatchMethod}")
                };

                SophonChunk sourceFileToCheckAsChunk = new SophonChunk
                {
                    ChunkHashDecompressed = Extension.HexToBytes(sourceFileHashString.AsSpan()),
                    ChunkName             = sourceFileNameToCheck,
                    ChunkOffset           = 0,
                    ChunkOldOffset        = 0,
                    ChunkSize             = sourceFileSizeToCheck,
                    ChunkSizeDecompressed = sourceFileSizeToCheck
                };

                FileInfo sourceFileInfoToCheck = sourceFilePathToCheck.CreateFileInfo();

                // Check for the original file existence and length
                bool isNeedCompleteDownload = !(sourceFileInfoToCheck is { Exists: true } &&
                                                sourceFileInfoToCheck.Length == sourceFileSizeToCheck);
                FileStream? sourceFileStreamToCheck = null;
                try
                {
                    // If the length check is passed, try check the hash of the file and compare it
                    if (!isNeedCompleteDownload)
                    {
                        // Open the stream, read it and check for the original file hash
                        sourceFileStreamToCheck = sourceFileInfoToCheck
                            .Open(new FileStreamOptions
                            {
                                Mode    = FileMode.OpenOrCreate,
                                Access  = FileAccess.ReadWrite,
                                Share   = FileShare.ReadWrite,
                                Options = FileOptions.SequentialScan
                            });

                        isNeedCompleteDownload = !(sourceFileToCheckAsChunk.ChunkHashDecompressed.Length != 8 ?
                            await sourceFileToCheckAsChunk.CheckChunkMd5HashAsync(sourceFileStreamToCheck,
                                true,
                                token) :
                            await sourceFileToCheckAsChunk.CheckChunkXxh64HashAsync(sourceFileStreamToCheck,
                                                                               sourceFileToCheckAsChunk.ChunkHashDecompressed,
                                                                               true,
                                                                               token));

                        // If it needs a complete download due to unmatched original file hash, then remove the file
                        if (isNeedCompleteDownload)
                        {
#if NET6_0_OR_GREATER
                            await sourceFileStreamToCheck.DisposeAsync();
#else
                            sourceFileStreamToCheck.Dispose();
#endif
                            PerformPatchAssetRemove(sourceFileInfoToCheck);
                        }
                        else
                        {
                            if (!isPatchHDiff)
                            {
                                diskWriteDelegate?.Invoke(TargetFileSize);
                                return;
                            }
                        }
                    }

                    // If the original file needs a download, then perform a DownloadOver patch method.
                    if (isNeedCompleteDownload)
                    {
                        PatchMethod = SophonPatchMethod.DownloadOver;
                    }
                }
                finally
                {
#if NET6_0_OR_GREATER
                    if (sourceFileStreamToCheck != null)
                    {
                        await sourceFileStreamToCheck.DisposeAsync();
                    }
#else
                    sourceFileStreamToCheck?.Dispose();
#endif
                }
            }

            Task<bool> writeDelegateTask = PatchMethod switch
            {
                SophonPatchMethod.DownloadOver => PerformPatchDownloadOver(client,
                                                                           inputDir,
                                                                           downloadReadDelegate,
                                                                           diskWriteDelegate,
                                                                           token),
                SophonPatchMethod.CopyOver => PerformPatchCopyOver(inputDir,
                                                                   patchOutputDir,                                   
                                                                   diskWriteDelegate,
                                                                   token),
                SophonPatchMethod.Patch => PerformPatchHDiff(inputDir,
                                                             patchOutputDir,
                                                             diskWriteDelegate,
                                                             token),
                _ => throw new InvalidOperationException($"Invalid operation while performing patch: {PatchMethod}")
            };

            try
            {
                bool isRedirect = !await writeDelegateTask;
                if (isRedirect)
                {
                    goto StartOver;
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (retry < 0)
                {
                    throw;
                }

                this.PushLogError("An error has occurred while performing patch with method:" +
                    $" {PatchMethod} for asset: {TargetFilePath}. Retry attempt left: {retry}\r\n" +
                    $"Source file: {OriginalFilePath}\r\nPatch file: {PatchNameSource}\r\nException: {ex}");

                // Set patch method to DownloadOver and start over
                PatchMethod = SophonPatchMethod.DownloadOver;
                --retry;

                goto StartOver;
            }
        }

        private async Task<bool> PerformPatchDownloadOver(HttpClient        client,
                                                          string            inputDir,
                                                          Action<long>?     downloadReadDelegate,
                                                          Action<long>?     diskWriteDelegate,
                                                          CancellationToken token)
        {
            bool isSuccess      = false;
            long writtenToDisk  = 0;
            long downloadedRead = 0;

            // Get the FileInfo and Path instance of the target file.
            string   targetFilePath     = Path.Combine(inputDir, TargetFilePath);
            FileInfo targetFileInfo     = targetFilePath.CreateFileInfo();
            string   targetFilePathTemp = targetFilePath + ".temp";
            FileInfo targetFileInfoTemp = targetFilePathTemp.CreateFileInfo();

            // If the target temporary file has already exists, try to unassign read-only before creating new stream.
            if (targetFileInfoTemp.Exists)
            {
                targetFileInfoTemp.IsReadOnly = false;
            }

            // Create target temporary file stream.
            targetFileInfoTemp.Directory?.Create();
            FileStream targetFileStreamTemp = targetFileInfoTemp.Open(new FileStreamOptions
            {
                Mode    = FileMode.Create,
                Access  = FileAccess.ReadWrite,
                Share   = FileShare.ReadWrite
            });
            targetFileInfoTemp.Refresh();
            try
            {
                // Download the file using Main Downloader
                await MainAssetInfo
                    .WriteToStreamAsync(client,
                                        targetFileStreamTemp,
                                        writeInfoDelegate: x =>
                                                           {
                                                               diskWriteDelegate?.Invoke(x);
                                                               Interlocked.Add(ref writtenToDisk, x);
                                                           },
                                        downloadInfoDelegate: (read, _) =>
                                                              {
                                                                  downloadReadDelegate?.Invoke(read);
                                                                  Interlocked.Add(ref downloadedRead, read);
                                                              },
                                        token: token);
                isSuccess = true;
                return true;
            }
            catch
            {
                diskWriteDelegate?.Invoke(-writtenToDisk);
                downloadReadDelegate?.Invoke(-downloadedRead);
                throw;
            }
            finally
            {
                // Dispose the target temporary file stream and try to remove old target file (if exist)
#if NET6_0_OR_GREATER
                await targetFileStreamTemp.DisposeAsync();
#else
                targetFileStreamTemp.Dispose();
#endif

                targetFileInfoTemp.Refresh();
                if (isSuccess)
                {
                    if (targetFileInfo.Exists)
                    {
                        targetFileInfo.IsReadOnly = false;
                        targetFileInfo.Refresh();
                        targetFileInfo.Delete();
                    }

                    // Then rename the temporary file to the actual file name
                    targetFileInfoTemp.MoveTo(targetFilePath);
                }
                else
                {
                    if (targetFileInfoTemp.Exists)
                    {
                        targetFileInfoTemp.IsReadOnly = false;
                        targetFileInfoTemp.Delete();
                    }
                }
            }
        }

        private void PerformPatchAssetRemove(FileInfo originalFileInfo)
        {
            try
            {
                if (!originalFileInfo.Exists)
                {
                    return;
                }

                originalFileInfo.IsReadOnly = false;
                originalFileInfo.Refresh();
                originalFileInfo.Delete();

                this.PushLogDebug("[Method: Remove] Removing asset file:" +
                    $" {OriginalFilePath} is completed!");
            }
            catch (Exception ex)
            {
                this.PushLogError("An error has occurred while deleting old asset:" +
                    $" {originalFileInfo.FullName} | {ex}");
            }
        }

        private async Task<bool> PerformPatchCopyOver(string            inputDir,
                                                      string            patchOutputDir,
                                                      Action<long>?     diskWriteDelegate,
                                                      CancellationToken token)
        {
            bool isSuccess     = false;
            long writtenToDisk = 0;

            PatchTargetProperty patchTargetProperty = PatchTargetProperty
                .Create(patchOutputDir,
                        inputDir,
                        TargetFilePath,
                        this,
                        true);

            bool isUseCopyToStrategy =
#if NET6_0_OR_GREATER
                    PatchChunkLength <= 1 << 20
#else
                    false
#endif
                    ;
            string logMessage = $"[Method: CopyOver][Strategy: {(isUseCopyToStrategy ? "DirectCopyTo" : "BufferedCopy")}]" +
                $" Writing target file: {TargetFilePath} with offset: {PatchOffset:x8}" +
                $" and length: {PatchChunkLength:x8} from {PatchNameSource} is completed!";

            try
            {
                if (patchTargetProperty.TargetFileTempStream == null)
                {
#if NET6_0_OR_GREATER
                    ArgumentNullException.ThrowIfNull(patchTargetProperty.TargetFileTempStream,
#else
                    throw new ArgumentNullException(
#endif
                    nameof(patchTargetProperty.TargetFileTempStream));
                }
                if (patchTargetProperty.PatchChunkStream == null)
                {

#if NET6_0_OR_GREATER
                    ArgumentNullException.ThrowIfNull(patchTargetProperty.PatchChunkStream,
#else
                    throw new ArgumentNullException(
#endif
                        nameof(patchTargetProperty.PatchChunkStream));
                }

                if (IsChunkActuallyHDiff(patchTargetProperty.PatchChunkStream))
                {
                    patchTargetProperty.PatchChunkStream.Position = 0;

                    string   originalFilePath = Path.Combine(inputDir, TargetFilePath + BlankFileDiffExt);
                    FileInfo fileInfo         = new FileInfo(originalFilePath);

                    if (fileInfo.Exists)
                    {
                        fileInfo.IsReadOnly = false;
                    }

                    fileInfo.Directory?.Create();
#if NET6_0_OR_GREATER
                    await fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.Write).DisposeAsync();
#else
                    fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.Write).Dispose();
#endif

                    OriginalFilePath ??= originalFilePath;
                    OriginalFileHash ??= BlankFileMd5Hash;
                    OriginalFileSize =   0;
                    PatchMethod      =   SophonPatchMethod.Patch;

                    // Remove if there's a left-over patch file caused by post issue files
                    FileInfo leftFile = new FileInfo(Path.Combine(inputDir, TargetFilePath));
                    if (!leftFile.Exists)
                    {
                        return false;
                    }

                    bool isDeleteLeftFile;
#if NET6_0_OR_GREATER
                    await
#endif
                    using (FileStream leftFileStream = leftFile.OpenRead())
                    {
                        isDeleteLeftFile = IsChunkActuallyHDiff(leftFileStream);
                    }

                    if (isDeleteLeftFile)
                    {
                        leftFile.IsReadOnly = false;
                        leftFile.Delete();
                    }

                    return false;
                }

#if NET6_0_OR_GREATER
                if (isUseCopyToStrategy)
                {
                    await patchTargetProperty.PatchChunkStream
                        .CopyToAsync(patchTargetProperty.TargetFileTempStream,
                                     token);
                    diskWriteDelegate?.Invoke(PatchChunkLength);

                    Interlocked.Add(ref writtenToDisk, PatchChunkLength);

                    isSuccess = true;
                    return false;
                }
#endif

                byte[] buffer = ArrayPool<byte>.Shared.Rent(SophonAsset.BufferSize);

                try
                {
                    int read;
                    while ((read = await patchTargetProperty.PatchChunkStream.ReadAsync(
#if NET6_0_OR_GREATER
                                buffer, token
#else
                                buffer, 0, buffer.Length, token
#endif
                               )) > 0)
                    {
#if NET6_0_OR_GREATER
                        patchTargetProperty.TargetFileTempStream.Write(buffer.AsSpan(0, read));
#else
                        patchTargetProperty.TargetFileTempStream.Write(buffer, 0, read);
#endif

                        diskWriteDelegate?.Invoke(read);
                        Interlocked.Add(ref writtenToDisk, read);
                    }

                    isSuccess = true;
                    return true;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            catch
            {
                diskWriteDelegate?.Invoke(-writtenToDisk);
                throw;
            }
            finally
            {
                this.PushLogDebug(logMessage);

                if (isSuccess)
                    patchTargetProperty.Dispose();
                else
                    patchTargetProperty.DisposeAndDeleteTemp();
            }
        }

        private async Task<bool> PerformPatchHDiff(string            inputDir,
                                                   string            patchOutputDir,
                                                   Action<long>?     diskWriteDelegate,
                                                   CancellationToken token)
        {
            bool isSuccess     = false;
            long writtenToDisk = 0;

            PatchTargetProperty patchTargetProperty = PatchTargetProperty
                .Create(patchOutputDir,
                        inputDir,
                        TargetFilePath,
                        this,
                        false);
            string logMessage = $"[Method: PatchHDiff] Writing target file: {TargetFilePath}" +
                $" with offset: {PatchOffset:x8} and length: {PatchChunkLength:x8}" +
                $" from {PatchNameSource} is completed!";

            FileInfo patchPath      = patchTargetProperty.PatchFilePath;
            string   targetTempPath = patchTargetProperty.TargetFileTempInfo.FullName;

            try
            {
                await Task.Factory
                          .StartNew(Impl,
                                    token,
                                    token,
                                    TaskCreationOptions.DenyChildAttach,
                                    TaskScheduler.Default)
                          .ConfigureAwait(false);

                this.PushLogDebug(logMessage);
                isSuccess = true;
            }
            finally
            {
                if (isSuccess)
                    patchTargetProperty.Dispose();
                else
                    patchTargetProperty.DisposeAndDeleteTemp();
            }

            return true;

            void Impl(object? ctx)
            {
                HDiffPatch patcher   = new HDiffPatch();
                string     inputPath = Path.Combine(inputDir, OriginalFilePath);
                try
                {
                    patcher.Initialize(CreateChunkStream);

                    patcher.Patch(inputPath,
                                  targetTempPath,
                                  true,
                                  x =>
                                  {
                                      diskWriteDelegate?.Invoke(x);
                                      Interlocked.Add(ref writtenToDisk, x);
                                  },
                                  (CancellationToken)ctx!,
                                  false,
                                  true);
                }
                catch
                {
                    diskWriteDelegate?.Invoke(-writtenToDisk);
                    throw;
                }
                finally
                {
                    if (inputPath.EndsWith(BlankFileDiffExt, StringComparison.OrdinalIgnoreCase))
                    {
                        FileInfo fileInfo = new FileInfo(inputPath);
                        if (fileInfo.Exists)
                        {
                            fileInfo.IsReadOnly = false;
                            fileInfo.Delete();
                        }
                    }
                }
            }

            ChunkStream CreateChunkStream()
            {
                FileStream fileStream = patchPath.Open(new FileStreamOptions
                {
                    Mode    = FileMode.Open,
                    Access  = FileAccess.Read,
                    Share   = FileShare.Read,
                    Options = FileOptions.SequentialScan
                });
                ChunkStream chunkStream = new ChunkStream(fileStream,
                                                          PatchOffset,
                                                          PatchOffset + PatchChunkLength,
                                                          true);

                return chunkStream;
            }
        }

        private async Task<bool> IsFilePatched(string inputPath, CancellationToken token)
        {
            string   targetFilePath = Path.Combine(inputPath, TargetFilePath);
            FileInfo targetFileInfo = targetFilePath.CreateFileInfo();

            if (!targetFileInfo.Exists)
            {
                return false;
            }

            if (TargetFileSize != targetFileInfo.Length)
            {
                return false;
            }

            SophonChunk checkByHashChunk = new SophonChunk
            {
                ChunkHashDecompressed = Extension.HexToBytes(TargetFileHash.AsSpan()),
                ChunkName             = TargetFilePath,
                ChunkSize             = TargetFileSize,
                ChunkSizeDecompressed = TargetFileSize,
                ChunkOffset           = 0,
                ChunkOldOffset        = 0,
            };

#if NET6_0_OR_GREATER
            await
#endif
            using FileStream targetFileStream = targetFileInfo.Open(new FileStreamOptions
            {
                Mode    = FileMode.Open,
                Access  = FileAccess.ReadWrite,
                Share   = FileShare.ReadWrite,
                Options = FileOptions.SequentialScan
            });

            bool isHashMatched = checkByHashChunk.ChunkHashDecompressed.Length == 8 ?
                await checkByHashChunk.CheckChunkXxh64HashAsync(targetFileStream,
                                                                checkByHashChunk.ChunkHashDecompressed,
                                                                true,
                                                                token) :
                await checkByHashChunk.CheckChunkMd5HashAsync(targetFileStream,
                                                              true,
                                                              token);

            return isHashMatched;
        }

        private static bool IsChunkActuallyHDiff(Stream chunkStream)
        {
#if NET7_0_OR_GREATER
            Span<byte> magicBuffer = stackalloc byte[HDiffPatchMagic.Length];
            _ = chunkStream.ReadAtLeast(magicBuffer, magicBuffer.Length, false);
#else
            byte[] magicBuffer = new byte[HDiffPatchMagic.Length];
            _ = chunkStream.Read(magicBuffer, 0, magicBuffer.Length);
#endif
            chunkStream.Position = 0;

            return HDiffPatchMagic.SequenceEqual(magicBuffer);
        }
    }

    internal class PatchTargetProperty : IDisposable
    {
        private FileInfo     TargetFileInfo       { get; }
        public  FileInfo     TargetFileTempInfo   { get; }
        public  FileStream?  TargetFileTempStream { get; }
        public  FileInfo     PatchFilePath        { get; }
        private FileStream?  PatchFileStream      { get; }
        public  ChunkStream? PatchChunkStream     { get; }

        private PatchTargetProperty(string patchOutputDir,
                                    string inputDir,
                                    string targetFilePath,
                                    SophonPatchAsset asset,
                                    bool createStream)
        {
            PatchFilePath  = asset.GetLegacyOrHoyoPlayPatchChunkPath(patchOutputDir);
            targetFilePath = Path.Combine(inputDir,       targetFilePath);
            string targetFileTempPath = targetFilePath + ".temp";

            TargetFileInfo     = targetFilePath.CreateFileInfo();
            TargetFileTempInfo = targetFileTempPath.CreateFileInfo();
            TargetFileTempInfo.Directory?.Create();

            if (TargetFileTempInfo.Exists)
            {
                TargetFileTempInfo.IsReadOnly = false;
                TargetFileTempInfo.Refresh();
            }

            if (!PatchFilePath.Exists)
            {
                throw new FileNotFoundException($"Required patch file: {PatchFilePath} is not found!");
            }

            if (!createStream)
            {
                return;
            }

            long patchChunkStart = asset.PatchOffset;
            long patchChunkEnd   = patchChunkStart + asset.PatchChunkLength;

            TargetFileTempStream = TargetFileTempInfo.Open(new FileStreamOptions
            {
                Mode    = FileMode.Create,
                Access  = FileAccess.Write,
                Share   = FileShare.Write
            });

            PatchFileStream = PatchFilePath.Open(new FileStreamOptions
            {
                Mode    = FileMode.Open,
                Access  = FileAccess.Read,
                Share   = FileShare.Read,
                Options = FileOptions.SequentialScan
            });

            PatchChunkStream = new ChunkStream(PatchFileStream, patchChunkStart, patchChunkEnd);
        }

        public static PatchTargetProperty Create(string patchOutputDir,
                                                 string inputDir,
                                                 string targetFilePath,
                                                 SophonPatchAsset asset,
                                                 bool createTempStream)
            => new(patchOutputDir,
                   inputDir,
                   targetFilePath,
                   asset,
                   createTempStream);

        public void Flush()
        {
            PatchChunkStream?.Dispose();
            PatchFileStream?.Dispose();
            TargetFileTempStream?.Dispose();
            TargetFileTempInfo.Refresh();
        }

        public void DisposeAndDeleteTemp()
        {
            Flush();
            if (!TargetFileTempInfo.Exists)
            {
                return;
            }

            TargetFileTempInfo.IsReadOnly = false;
            TargetFileTempInfo.Delete();
        }

        public void Dispose()
        {
            Flush();
            if (TargetFileInfo.Exists)
            {
                TargetFileInfo.IsReadOnly = false;
                TargetFileInfo.Delete();
            }

            TargetFileTempInfo.MoveTo(TargetFileInfo.FullName);
        }
    }
}