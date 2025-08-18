using Hi3Helper.Sophon.Helper;
using Hi3Helper.Sophon.Infos;
using Hi3Helper.Sophon.Protos;
using Hi3Helper.Sophon.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#if NET6_0_OR_GREATER
using ZstdNet;
#endif

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

#nullable enable
namespace Hi3Helper.Sophon
{
    public static partial class SophonPatch
    {
        private static readonly object DummyInstance = new();

        /// <summary>
        ///     Enumerate/Get the list of Sophon patches for update.
        /// </summary>
        /// <param name="httpClient">
        ///     The <seealso cref="HttpClient" /> to be used to download the manifest data.
        /// </param>
        /// <param name="patchInfoPair">
        ///     Pair of the Patch Manifest and Chunks information struct.
        /// </param>
        /// <param name="mainInfoPair">
        ///     Pair of the Main Manifest and Chunks information struct.
        /// </param>
        /// <param name="downloadSpeedLimiter">
        ///     If the download speed limiter is null, the download speed will be set to unlimited.
        /// </param>
        /// <param name="versionTagUpdateFrom">
        ///     Define which version tag in which the data will be patched from.
        /// </param>
        /// <param name="token">
        ///     Cancellation token for handling cancellation while the routine is running.
        /// </param>
        /// <returns>
        ///     An enumeration to enumerate the Sophon update asset from the manifest.
        /// </returns>
        /// <exception cref="DllNotFoundException">
        ///     Indicates if a library required is missing.
        /// </exception>
        /// <exception cref="HttpRequestException">
        ///     Indicates if an error during Http request is happening.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     Indicates if an argument or Http response returns a <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Indicates if an argument is <c>null</c> or empty.
        /// </exception>
        public static IAsyncEnumerable<SophonPatchAsset>
            EnumerateUpdateAsync(HttpClient                   httpClient,
                                 SophonChunkManifestInfoPair? patchInfoPair,
                                 SophonChunkManifestInfoPair  mainInfoPair,
                                 string                       versionTagUpdateFrom,
                                 SophonDownloadSpeedLimiter?  downloadSpeedLimiter = null,
                                 CancellationToken            token                = default)
            => EnumerateUpdateAsync(httpClient,
                                    patchInfoPair?.ManifestInfo,
                                    patchInfoPair?.ChunksInfo,
                                    mainInfoPair.ManifestInfo,
                                    mainInfoPair.ChunksInfo,
                                    versionTagUpdateFrom,
                                    downloadSpeedLimiter,
                                    token);

        /// <summary>
        ///     Enumerate/Get the list of Sophon patches for update.
        /// </summary>
        /// <param name="httpClient">
        ///     The <seealso cref="HttpClient" /> to be used to download the manifest data.
        /// </param>
        /// <param name="patchManifestInfo">
        ///     Patch Manifest information struct.
        /// </param>
        /// <param name="patchChunksInfo">
        ///     Patch Chunks information struct.
        /// </param>
        /// <param name="mainManifestInfo">
        ///     Patch Manifest information struct.
        /// </param>
        /// <param name="mainChunksInfo">
        ///     Patch Chunks information struct.
        /// </param>
        /// <param name="downloadSpeedLimiter">
        ///     If the download speed limiter is null, the download speed will be set to unlimited.
        /// </param>
        /// <param name="versionTagUpdateFrom">
        ///     Define which version tag in which the data will be patched from.
        /// </param>
        /// <param name="token">
        ///     Cancellation token for handling cancellation while the routine is running.
        /// </param>
        /// <returns>
        ///     An enumeration to enumerate the Sophon update asset from the manifest.
        /// </returns>
        /// <exception cref="DllNotFoundException">
        ///     Indicates if a library required is missing.
        /// </exception>
        /// <exception cref="HttpRequestException">
        ///     Indicates if an error during Http request is happening.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     Indicates if an argument or Http response returns a <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Indicates if an argument is <c>null</c> or empty.
        /// </exception>
        public static async IAsyncEnumerable<SophonPatchAsset>
            EnumerateUpdateAsync(HttpClient                                 httpClient,
                                 SophonManifestInfo?                        patchManifestInfo,
                                 SophonChunksInfo?                          patchChunksInfo,
                                 [NotNull] SophonManifestInfo?              mainManifestInfo,
                                 [NotNull] SophonChunksInfo?                mainChunksInfo,
                                 string                                     versionTagUpdateFrom,
                                 SophonDownloadSpeedLimiter?                downloadSpeedLimiter = null,
                                 [EnumeratorCancellation] CancellationToken token                = default)
        {
            ArgumentNullException.ThrowIfNull(mainManifestInfo, nameof(mainManifestInfo));
            ArgumentNullException.ThrowIfNull(mainChunksInfo, nameof(mainChunksInfo));

#if NET6_0_OR_GREATER
            if (!DllUtils.IsLibraryExist(DllUtils.DllName))
            {
                throw new DllNotFoundException("libzstd is not found!");
            }
#endif

            if (string.IsNullOrEmpty(versionTagUpdateFrom))
            {
                throw new ArgumentNullException(nameof(versionTagUpdateFrom), "Version tag is not defined!");
            }

            Dictionary<string, (SophonPatchAssetProperty, SophonPatchAssetInfo)> patchAssetPropertyDict = new(StringComparer.OrdinalIgnoreCase);
            if (patchManifestInfo != null && patchChunksInfo != null)
            {
                ActionTimeoutTaskCallback<SophonPatchProto> manifestFromProtoTaskCallback =
                    async innerToken => await httpClient.ReadProtoFromManifestInfo(patchManifestInfo,
                        SophonPatchProto.Parser,
                        innerToken);

                var patchManifestProto = await TaskExtensions
                   .WaitForRetryAsync(() => manifestFromProtoTaskCallback,
                                      TaskExtensions.DefaultTimeoutSec,
                                      null,
                                      null,
                                      null,
                                      token);

                foreach (SophonPatchAssetProperty patchAssetProperty in patchManifestProto.PatchAssets)
                {
                    var patchAssetInfo = patchAssetProperty.AssetInfos
                                                           .FirstOrDefault(x => x.VersionTag.Equals(versionTagUpdateFrom, StringComparison.OrdinalIgnoreCase));

                    if (patchAssetInfo != null)
                    {
                        patchAssetPropertyDict.TryAdd(patchAssetProperty.AssetName, (patchAssetProperty, patchAssetInfo));
                    }
                }
            }

            await foreach (SophonAsset mainAsset in SophonManifest
                .EnumerateAsync(httpClient,
                                mainManifestInfo,
                                mainChunksInfo,
                                downloadSpeedLimiter,
                                token))
            {
                if (mainAsset.IsDirectory)
                {
                    continue;
                }

                ref var patchProperty = ref CollectionsMarshal
                    .GetValueRefOrNullRef(patchAssetPropertyDict,
                                          mainAsset.AssetName);

                if (Unsafe.IsNullRef(ref patchProperty))
                {
                    yield return new SophonPatchAsset
                    {
                        MainAssetInfo  = mainAsset,
                        TargetFilePath = mainAsset.AssetName,
                        TargetFileSize = mainAsset.AssetSize,
                        TargetFileHash = mainAsset.AssetHash,
                        PatchMethod    = SophonPatchMethod.DownloadOver
                    };
                    continue;
                }

                if (string.IsNullOrEmpty(patchProperty.Item2.Chunk.OriginalFileName))
                {
                    yield return new SophonPatchAsset
                    {
                        MainAssetInfo    = mainAsset,
                        PatchInfo        = patchChunksInfo,
                        PatchNameSource  = patchProperty.Item2.Chunk.PatchName,
                        PatchHash        = patchProperty.Item2.Chunk.PatchMd5,
                        PatchOffset      = patchProperty.Item2.Chunk.PatchOffset,
                        PatchSize        = patchProperty.Item2.Chunk.PatchSize,
                        PatchChunkLength = patchProperty.Item2.Chunk.PatchLength,
                        TargetFilePath   = mainAsset.AssetName,
                        TargetFileSize   = mainAsset.AssetSize,
                        TargetFileHash   = mainAsset.AssetHash,
                        PatchMethod      = SophonPatchMethod.CopyOver
                    };
                    continue;
                }

                yield return new SophonPatchAsset
                {
                    MainAssetInfo    = mainAsset,
                    PatchInfo        = patchChunksInfo,
                    PatchNameSource  = patchProperty.Item2.Chunk.PatchName,
                    PatchHash        = patchProperty.Item2.Chunk.PatchMd5,
                    PatchOffset      = patchProperty.Item2.Chunk.PatchOffset,
                    PatchSize        = patchProperty.Item2.Chunk.PatchSize,
                    PatchChunkLength = patchProperty.Item2.Chunk.PatchLength,
                    TargetFilePath   = mainAsset.AssetName,
                    TargetFileSize   = mainAsset.AssetSize,
                    TargetFileHash   = mainAsset.AssetHash,
                    OriginalFilePath = patchProperty.Item2.Chunk.OriginalFileName,
                    OriginalFileSize = patchProperty.Item2.Chunk.OriginalFileLength,
                    OriginalFileHash = patchProperty.Item2.Chunk.OriginalFileMd5,
                    PatchMethod      = SophonPatchMethod.Patch
                };
            }
        }

        /// <summary>
        ///     Enumerate/Get the list of Sophon patches for update.
        /// </summary>
        /// <param name="httpClient">
        ///     The <seealso cref="HttpClient" /> to be used to download the manifest data.
        /// </param>
        /// <param name="patchInfoPair">
        ///     Pair of the Patch Manifest and Chunks information struct.
        /// </param>
        /// <param name="mainInfoPair">
        ///     Pair of the Main Manifest and Chunks information struct.
        /// </param>
        /// <param name="versionTagUpdateFrom">
        ///     Define which version tag in which the data will be patched from.
        /// </param>
        /// <param name="compareWithList">
        ///     The list of asset paths to compare with.
        /// </param>
        /// <param name="token">
        ///     Cancellation token for handling cancellation while the routine is running.
        /// </param>
        /// <returns>
        ///     An enumeration to enumerate the Sophon update asset from the manifest.
        /// </returns>
        /// <exception cref="DllNotFoundException">
        ///     Indicates if a library required is missing.
        /// </exception>
        /// <exception cref="HttpRequestException">
        ///     Indicates if an error during Http request is happening.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     Indicates if an argument or Http response returns a <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Indicates if an argument is <c>null</c> or empty.
        /// </exception>
        public static IAsyncEnumerable<SophonPatchAsset>
            EnumerateRemovableAsync(HttpClient                   httpClient,
                                    SophonChunkManifestInfoPair? patchInfoPair,
                                    SophonChunkManifestInfoPair  mainInfoPair,
                                    string                       versionTagUpdateFrom,
                                    HashSet<string>              compareWithList,
                                    CancellationToken            token = default)
            => EnumerateRemovableAsync(httpClient,
                                       patchInfoPair?.ManifestInfo,
                                       patchInfoPair?.ChunksInfo,
                                       mainInfoPair.ManifestInfo,
                                       mainInfoPair.ChunksInfo,
                                       versionTagUpdateFrom,
                                       compareWithList,
                                       token);

        /// <summary>
        ///     Enumerate/Get the list of Sophon removable assets.
        /// </summary>
        /// <param name="httpClient">
        ///     The <seealso cref="HttpClient" /> to be used to download the manifest data.
        /// </param>
        /// <param name="patchManifestInfo">
        ///     Patch Manifest information struct.
        /// </param>
        /// <param name="patchChunksInfo">
        ///     Patch Chunks information struct.
        /// </param>
        /// <param name="mainManifestInfo">
        ///     Patch Manifest information struct.
        /// </param>
        /// <param name="mainChunksInfo">
        ///     Patch Chunks information struct.
        /// </param>
        /// <param name="versionTagUpdateFrom">
        ///     Define which version tag in which the data will be patched from.
        /// </param>
        /// <param name="compareWithList">
        ///     The list of asset paths to compare with.
        /// </param>
        /// <param name="token">
        ///     Cancellation token for handling cancellation while the routine is running.
        /// </param>
        /// <returns>
        ///     An enumeration to enumerate the Sophon update asset from the manifest.
        /// </returns>
        /// <exception cref="DllNotFoundException">
        ///     Indicates if a library required is missing.
        /// </exception>
        /// <exception cref="HttpRequestException">
        ///     Indicates if an error during Http request is happening.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     Indicates if an argument or Http response returns a <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Indicates if an argument is <c>null</c> or empty.
        /// </exception>
        public static async IAsyncEnumerable<SophonPatchAsset>
            EnumerateRemovableAsync(HttpClient                                 httpClient,
                                    SophonManifestInfo?                        patchManifestInfo,
                                    SophonChunksInfo?                          patchChunksInfo,
                                    [NotNull] SophonManifestInfo?              mainManifestInfo,
                                    [NotNull] SophonChunksInfo?                mainChunksInfo,
                                    string                                     versionTagUpdateFrom,
                                    HashSet<string>                            compareWithList,
                                    [EnumeratorCancellation] CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(mainManifestInfo, nameof(mainManifestInfo));
            ArgumentNullException.ThrowIfNull(mainChunksInfo, nameof(mainChunksInfo));

#if NET6_0_OR_GREATER
            if (!DllUtils.IsLibraryExist(DllUtils.DllName))
            {
                throw new DllNotFoundException("libzstd is not found!");
            }
#endif

            if (string.IsNullOrEmpty(versionTagUpdateFrom))
            {
                throw new ArgumentNullException(nameof(versionTagUpdateFrom), "Version tag is not defined!");
            }

            if (patchManifestInfo == null || patchChunksInfo == null)
            {
                yield break;
            }

            ActionTimeoutTaskCallback<SophonPatchProto> manifestFromProtoTaskCallback =
                async innerToken => await httpClient.ReadProtoFromManifestInfo(patchManifestInfo,
                                                                               SophonPatchProto.Parser,
                                                                               innerToken);

            var patchManifestProto = await TaskExtensions
               .WaitForRetryAsync(() => manifestFromProtoTaskCallback,
                                  TaskExtensions.DefaultTimeoutSec,
                                  null,
                                  null,
                                  null,
                                  token);

            foreach (SophonUnusedAssetProperty unusedAssetProperty in patchManifestProto.UnusedAssets)
            {
                foreach (SophonUnusedAssetInfo assetInfo in unusedAssetProperty.AssetInfos)
                {
                    foreach (SophonUnusedAssetFile unusedAssetFile in assetInfo.Assets)
                    {
                        if (compareWithList.Contains(unusedAssetFile.FileName))
                        {
                            continue;
                        }

                        yield return new SophonPatchAsset
                        {
                            OriginalFileHash = unusedAssetFile.FileMd5,
                            OriginalFileSize = unusedAssetFile.FileSize,
                            OriginalFilePath = unusedAssetFile.FileName,
                            PatchMethod      = SophonPatchMethod.Remove
                        };
                    }
                }
            }
        }

        /// <summary>
        ///     Enumerate/Get the list of Sophon patches for update.
        /// </summary>
        /// <param name="httpClient">
        ///     The <seealso cref="HttpClient" /> to be used to download the manifest data.
        /// </param>
        /// <param name="patchInfoPair">
        ///     Pair of the Patch Manifest and Chunks information struct.
        /// </param>
        /// <param name="mainInfoPair">
        ///     Pair of the Main Manifest and Chunks information struct.
        /// </param>
        /// <param name="versionTagUpdateFrom">
        ///     Define which version tag in which the data will be patched from.
        /// </param>
        /// <param name="compareWithList">
        ///     The list of assets to compare with.
        /// </param>
        /// <param name="token">
        ///     Cancellation token for handling cancellation while the routine is running.
        /// </param>
        /// <returns>
        ///     An enumeration to enumerate the Sophon update asset from the manifest.
        /// </returns>
        /// <exception cref="DllNotFoundException">
        ///     Indicates if a library required is missing.
        /// </exception>
        /// <exception cref="HttpRequestException">
        ///     Indicates if an error during Http request is happening.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     Indicates if an argument or Http response returns a <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Indicates if an argument is <c>null</c> or empty.
        /// </exception>
        public static IAsyncEnumerable<SophonPatchAsset>
            EnumerateRemovableAsync(HttpClient                   httpClient,
                                    SophonChunkManifestInfoPair? patchInfoPair,
                                    SophonChunkManifestInfoPair  mainInfoPair,
                                    string                       versionTagUpdateFrom,
                                    List<SophonPatchAsset>       compareWithList,
                                    CancellationToken            token = default)
            => EnumerateRemovableAsync(httpClient,
                                       patchInfoPair?.ManifestInfo,
                                       patchInfoPair?.ChunksInfo,
                                       mainInfoPair.ManifestInfo,
                                       mainInfoPair.ChunksInfo,
                                       versionTagUpdateFrom,
                                       compareWithList,
                                       token);

        /// <summary>
        ///     Enumerate/Get the list of Sophon removable assets.
        /// </summary>
        /// <param name="httpClient">
        ///     The <seealso cref="HttpClient" /> to be used to download the manifest data.
        /// </param>
        /// <param name="patchManifestInfo">
        ///     Patch Manifest information struct.
        /// </param>
        /// <param name="patchChunksInfo">
        ///     Patch Chunks information struct.
        /// </param>
        /// <param name="mainManifestInfo">
        ///     Patch Manifest information struct.
        /// </param>
        /// <param name="mainChunksInfo">
        ///     Patch Chunks information struct.
        /// </param>
        /// <param name="versionTagUpdateFrom">
        ///     Define which version tag in which the data will be patched from.
        /// </param>
        /// <param name="compareWithList">
        ///     The list of assets to compare with.
        /// </param>
        /// <param name="token">
        ///     Cancellation token for handling cancellation while the routine is running.
        /// </param>
        /// <returns>
        ///     An enumeration to enumerate the Sophon update asset from the manifest.
        /// </returns>
        /// <exception cref="DllNotFoundException">
        ///     Indicates if a library required is missing.
        /// </exception>
        /// <exception cref="HttpRequestException">
        ///     Indicates if an error during Http request is happening.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     Indicates if an argument or Http response returns a <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Indicates if an argument is <c>null</c> or empty.
        /// </exception>
        public static IAsyncEnumerable<SophonPatchAsset>
            EnumerateRemovableAsync(HttpClient                    httpClient,
                                    SophonManifestInfo?           patchManifestInfo,
                                    SophonChunksInfo?             patchChunksInfo,
                                    [NotNull] SophonManifestInfo? mainManifestInfo,
                                    [NotNull] SophonChunksInfo?   mainChunksInfo,
                                    string                        versionTagUpdateFrom,
                                    List<SophonPatchAsset>        compareWithList,
                                    CancellationToken             token = default)
        {
            HashSet<string> hashSet = new(StringComparer.OrdinalIgnoreCase);
            foreach (SophonPatchAsset asset in compareWithList)
            {
                hashSet.Add(asset.TargetFilePath);
            }

            return EnumerateRemovableAsync(httpClient, patchManifestInfo, patchChunksInfo, mainManifestInfo, mainChunksInfo, versionTagUpdateFrom, hashSet, token);
        }

        public static IEnumerable<SophonPatchAsset> EnsureOnlyGetDedupPatchAssets(this IEnumerable<SophonPatchAsset> patchAssetEnumerable)
        {
            HashSet<string> processedAsset = [];
            foreach (SophonPatchAsset asset in patchAssetEnumerable
                        .Where(x => !string.IsNullOrEmpty(x.PatchNameSource) &&
                                    processedAsset.Add(x.PatchNameSource)))
            {
                yield return asset;
            }
        }

        public static void RemovePatches(this IEnumerable<SophonPatchAsset> patchAssetEnumerable,
                                         string                             patchOutputDir)
        {
            foreach (SophonPatchAsset asset in patchAssetEnumerable
                        .EnsureOnlyGetDedupPatchAssets())
            {
                string patchFilePath = Path.Combine(patchOutputDir, asset.PatchNameSource);

                try
                {
                    FileInfo fileInfo = patchFilePath.CreateFileInfo();
                    if (fileInfo.Exists)
                    {
                        fileInfo.Delete();
                        DummyInstance.PushLogDebug($"Removed patch file: {patchFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    DummyInstance.PushLogError($"Failed while trying to remove patch file: {patchFilePath} | {ex}");
                }
            }
        }
    }
}
