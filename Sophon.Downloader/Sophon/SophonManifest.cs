// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable InvalidXmlDocComment

#if NET9_0_OR_GREATER
using ZstdNet;
#endif
using Hi3Helper.Sophon.Helper;
using Hi3Helper.Sophon.Infos;
using Hi3Helper.Sophon.Protos;
using Hi3Helper.Sophon.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using TaskExtensions = Hi3Helper.Sophon.Helper.TaskExtensions;

// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable UseAwaitUsing

namespace Hi3Helper.Sophon
{
    public static partial class SophonManifest
    {
        /// <summary>
        ///     Enumerate/Get the list of Sophon assets contained within the manifest.
        /// </summary>
        /// <param name="httpClient">
        ///     The <seealso cref="HttpClient" /> to be used to download the manifest data.
        /// </param>
        /// <param name="infoPair">
        ///     Pair of Manifest and Chunks information struct.
        /// </param>
        /// <param name="token">
        ///     Cancellation token for handling cancellation while the routine is running.
        /// </param>
        /// <param name="downloadSpeedLimiter">
        ///     If the download speed limiter is null, the download speed will be set to unlimited.
        /// </param>
        /// <returns>
        ///     An enumeration to enumerate the Sophon asset from the manifest.
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
        public static async IAsyncEnumerable<SophonAsset>
            EnumerateAsync(HttpClient                                 httpClient,
                           SophonChunkManifestInfoPair                infoPair,
                           SophonDownloadSpeedLimiter                 downloadSpeedLimiter = null,
                           [EnumeratorCancellation] CancellationToken token                = default)

        {
            await foreach (SophonAsset asset in EnumerateAsync(httpClient,
                                                               infoPair.ManifestInfo,
                                                               infoPair.ChunksInfo,
                                                               downloadSpeedLimiter,
                                                               token))
            {
                yield return asset;
            }
        }

        /// <summary>
        ///     Enumerate/Get the list of Sophon assets contained within the manifest.
        /// </summary>
        /// <param name="httpClient">
        ///     The <seealso cref="HttpClient" /> to be used to download the manifest data.
        /// </param>
        /// <param name="manifestInfo">
        ///     Manifest information struct.
        /// </param>
        /// <param name="chunksInfo">
        ///     Chunks information struct.
        /// </param>
        /// <param name="downloadSpeedLimiter">
        ///     If the download speed limiter is null, the download speed will be set to unlimited.
        /// </param>
        /// <param name="token">
        ///     Cancellation token for handling cancellation while the routine is running.
        /// </param>
        /// <returns>
        ///     An enumeration to get the Sophon asset list from the manifest.
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
        public static async IAsyncEnumerable<SophonAsset>
            EnumerateAsync(HttpClient                                 httpClient,
                           SophonManifestInfo                         manifestInfo,
                           SophonChunksInfo                           chunksInfo,
                           SophonDownloadSpeedLimiter                 downloadSpeedLimiter = null,
                           [EnumeratorCancellation] CancellationToken token                = default)
        {
        #if NET9_0_OR_GREATER
            if (!DllUtils.IsLibraryExist(DllUtils.DllName))
            {
                throw new DllNotFoundException("libzstd is not found!");
            }
        #else
            List<SophonAsset> assetList = new List<SophonAsset>();
        #endif

            ActionTimeoutTaskCallback<SophonManifestProto> manifestProtoTaskCallback =
                async innerToken => await httpClient.ReadProtoFromManifestInfo(manifestInfo,
                                                                               SophonManifestProto.Parser,
                                                                               innerToken);

            SophonManifestProto manifestProto = await TaskExtensions
               .WaitForRetryAsync(() => manifestProtoTaskCallback,
                                  TaskExtensions.DefaultTimeoutSec,
                                  null,
                                  null,
                                  null,
                                  token);

            foreach (SophonManifestAssetProperty asset in manifestProto.Assets)
            {
                yield return AssetProperty2SophonAsset(asset,
                                                       chunksInfo,
                                                       downloadSpeedLimiter);
            }
        }

        internal static SophonAsset AssetProperty2SophonAsset(SophonManifestAssetProperty asset,
                                                              SophonChunksInfo            chunksInfo,
                                                              SophonDownloadSpeedLimiter  downloadSpeedLimiter)
        {
            SophonAsset assetAdd;
            string      assetName = asset.AssetName;
            int         assetType = asset.AssetType;
            if (assetType != 0 || string.IsNullOrEmpty(asset.AssetHashMd5))
            {
                assetAdd = new SophonAsset
                {
                    AssetName            = assetName,
                    IsDirectory          = true,
                    DownloadSpeedLimiter = downloadSpeedLimiter
                };
                return assetAdd;
            }

            string assetHash = asset.AssetHashMd5;
            long   assetSize = asset.AssetSize;
            SophonChunk[] assetChunks = asset.AssetChunks.Select(x => new SophonChunk
            {
                ChunkName = x.ChunkName,
                ChunkHashDecompressed = Extension.HexToBytes(x.ChunkDecompressedHashMd5
                                                           #if !NET6_0_OR_GREATER
                                                              .AsSpan()
                                                           #endif
                                                            ),
                ChunkOffset           = x.ChunkOnFileOffset,
                ChunkSize             = x.ChunkSize,
                ChunkSizeDecompressed = x.ChunkSizeDecompressed
            }).ToArray();

            assetAdd = new SophonAsset
            {
                AssetName            = assetName,
                AssetHash            = assetHash,
                AssetSize            = assetSize,
                Chunks               = assetChunks,
                SophonChunksInfo     = chunksInfo,
                IsDirectory          = false,
                DownloadSpeedLimiter = downloadSpeedLimiter
            };
            return assetAdd;
        }
    }
}