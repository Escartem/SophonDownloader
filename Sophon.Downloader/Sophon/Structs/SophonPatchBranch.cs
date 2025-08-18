using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hi3Helper.Sophon.Structs;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

#nullable enable
namespace Hi3Helper.Sophon
{
    public partial class SophonPatch
    {
        public static async
            Task<SophonChunkManifestInfoPair>
            CreateSophonChunkManifestInfoPair(HttpClient        client,
                                              string            url,
                                              string            versionUpdateFrom,
                                              string?           matchingField = null,
                                              CancellationToken token         = default)
        {
            var sophonPatchBranch = await SophonManifest.GetSophonBranchInfo
#if !NET6_0_OR_GREATER
                <SophonManifestPatchBranch>
#endif
                (client,
                 url,
#if NET6_0_OR_GREATER
                 SophonContext.Default.SophonManifestPatchBranch,
#endif
                 HttpMethod.Post,
                 token);

            if (sophonPatchBranch.Data == null)
            {
                return new SophonChunkManifestInfoPair
                {
                    IsFound       = false,
                    ReturnCode    = sophonPatchBranch.ReturnCode,
                    ReturnMessage = sophonPatchBranch.ReturnMessage
                };
            }

            if (string.IsNullOrEmpty(matchingField))
            {
                matchingField = "game";
            }

            SophonManifestPatchIdentity? sophonPatchIdentity =
                sophonPatchBranch.Data.ManifestIdentityList?.FirstOrDefault(x => x.MatchingField == matchingField);

            if (sophonPatchIdentity == null)
            {
                return new SophonChunkManifestInfoPair
                {
                    IsFound       = false,
                    ReturnCode    = 404,
                    ReturnMessage = $"Sophon patch with matching field: {matchingField} is not found!"
                };
            }

            if (!sophonPatchIdentity
                    .DiffTaggedInfo
                    .TryGetValue(versionUpdateFrom,
                        out SophonManifestChunkInfo? sophonChunkInfo))
            {
                return new SophonChunkManifestInfoPair
                {
                    IsFound = false,
                    ReturnCode = 404,
                    ReturnMessage = $"Sophon patch diff tagged info with version: {versionUpdateFrom} is not found!"
                };
            }

            return new SophonChunkManifestInfoPair
            {
                ChunksInfo = SophonManifest.CreateChunksInfo(sophonPatchIdentity.DiffUrlInfo.UrlPrefix,
                                                             sophonChunkInfo.ChunkCount,
                                                             sophonChunkInfo.FileCount,
                                                             sophonPatchIdentity.DiffUrlInfo.IsCompressed,
                                                             sophonChunkInfo.UncompressedSize,
                                                             sophonChunkInfo.CompressedSize),
                ManifestInfo = SophonManifest.CreateManifestInfo(sophonPatchIdentity.ManifestUrlInfo.UrlPrefix,
                                                                 sophonPatchIdentity.ManifestFileInfo.Checksum,
                                                                 sophonPatchIdentity.ManifestFileInfo.FileName,
                                                                 sophonPatchIdentity.ManifestUrlInfo.IsCompressed,
                                                                 sophonPatchIdentity.ManifestFileInfo.UncompressedSize,
                                                                 sophonPatchIdentity.ManifestFileInfo.CompressedSize),
                OtherSophonBuildData = null,
                OtherSophonPatchData = sophonPatchBranch.Data
            };
        }
    }
}
