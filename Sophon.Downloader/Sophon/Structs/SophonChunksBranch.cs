using Hi3Helper.Sophon.Structs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
#if NET6_0_OR_GREATER
using System.Text.Json.Serialization.Metadata;
#endif
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CheckNamespace
namespace Hi3Helper.Sophon
{
    public static partial class SophonManifest
    {
        public static async
            Task<T>
            GetSophonBranchInfo<T>(
                HttpClient client,
                string url,
#if NET6_0_OR_GREATER
                JsonTypeInfo<T> jsonTypeInfo,
#endif
                HttpMethod httpMethod,
                CancellationToken token = default)
        {
            using HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, url);
            using HttpResponseMessage responseMessage = await client.SendAsync(requestMessage,
                                                                               HttpCompletionOption.ResponseHeadersRead,
                                                                               token);
            responseMessage.EnsureSuccessStatusCode();

#if NET6_0_OR_GREATER
            await
#endif
            using Stream responseStream = await responseMessage.Content.ReadAsStreamAsync(
#if NET6_0_OR_GREATER
                token
#endif
                );

#if NET6_0_OR_GREATER
            return await JsonSerializer.DeserializeAsync(responseStream, jsonTypeInfo, token);
#else
            return (T)await JsonSerializer.DeserializeAsync(
                utf8Json: responseStream,
                returnType: typeof(T),
                options: null,
                cancellationToken: token
                );
#endif
        }

        public static
            Task<SophonChunkManifestInfoPair>
            CreateSophonChunkManifestInfoPair(HttpClient client,
                                              string url,
                                              string matchingField = null,
                                              CancellationToken token = default)
            => CreateSophonChunkManifestInfoPair(client, url, matchingField, true, token);

        public static async
            Task<SophonChunkManifestInfoPair>
            CreateSophonChunkManifestInfoPair(HttpClient        client,
                                              string            url,
                                              string            matchingField     = null,
                                              bool              isThrowIfNotFound = true,
                                              CancellationToken token             = default)
        {
            var sophonBranch = await GetSophonBranchInfo
#if !NET6_0_OR_GREATER
                <SophonManifestBuildBranch>
#endif
                (client,
                 url,
#if NET6_0_OR_GREATER
                 SophonContext.Default.SophonManifestBuildBranch,
#endif
                 HttpMethod.Get,
                 token);

            if (sophonBranch.Data == null)
            {
                return new SophonChunkManifestInfoPair
                {
                    IsFound       = false,
                    ReturnCode    = sophonBranch.ReturnCode,
                    ReturnMessage = sophonBranch.ReturnMessage
                };
            }

            if (string.IsNullOrEmpty(matchingField))
            {
                matchingField = "game";
            }

            var sophonManifestIdentity =
                sophonBranch.Data.ManifestIdentityList?
                   .FirstOrDefault(x => x.MatchingField == matchingField);

            if (sophonManifestIdentity != null)
            {
                return new SophonChunkManifestInfoPair
                {
                    ChunksInfo = CreateChunksInfo(sophonManifestIdentity.ChunksUrlInfo.UrlPrefix,
                                                  sophonManifestIdentity.ChunkInfo.ChunkCount,
                                                  sophonManifestIdentity.ChunkInfo.FileCount,
                                                  sophonManifestIdentity.ChunksUrlInfo.IsCompressed,
                                                  sophonManifestIdentity.ChunkInfo.UncompressedSize,
                                                  sophonManifestIdentity.ChunkInfo.CompressedSize),
                    ManifestInfo = CreateManifestInfo(sophonManifestIdentity.ManifestUrlInfo.UrlPrefix,
                                                      sophonManifestIdentity.ManifestFileInfo.Checksum,
                                                      sophonManifestIdentity.ManifestFileInfo.FileName,
                                                      sophonManifestIdentity.ManifestUrlInfo.IsCompressed,
                                                      sophonManifestIdentity.ManifestFileInfo.UncompressedSize,
                                                      sophonManifestIdentity.ManifestFileInfo.CompressedSize),
                    OtherSophonBuildData = sophonBranch.Data,
                    MatchingField        = sophonManifestIdentity.MatchingField,
                    CategoryName         = sophonManifestIdentity.CategoryName,
                    CategoryId           = sophonManifestIdentity.CategoryId
                };
            }

            if (isThrowIfNotFound)
                throw new KeyNotFoundException($"Sophon manifest with matching field: {matchingField} is not found!");

            return new SophonChunkManifestInfoPair
            {
                IsFound       = false,
                ReturnCode    = 404,
                ReturnMessage = $"Sophon manifest with matching field: {matchingField} is not found!"
            };
        }
    }
}