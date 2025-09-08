using Hi3Helper.Sophon;
using Hi3Helper.Sophon.Structs;

namespace Core
{
    internal class Assets
    {
        public static async Task<Tuple<List<SophonAsset>, long>> GetAssetsFromManifests(HttpClient httpClient, string matchingField, string prevManifestUrl, string newManifestUrl, CancellationTokenSource tokenSource)
        {
            List<SophonAsset> assets = new List<SophonAsset>();

            SophonChunkManifestInfoPair manifestPairFrom = await SophonManifest.CreateSophonChunkManifestInfoPair(
                httpClient,
                prevManifestUrl,
                matchingField,
                tokenSource.Token
            );

            SophonChunkManifestInfoPair manifestPairTo = new();

            if (newManifestUrl != "")
            {
                manifestPairTo = await SophonManifest.CreateSophonChunkManifestInfoPair(
                    httpClient,
                    newManifestUrl,
                    matchingField,
                    tokenSource.Token
                );
            }

            // process assets
            long updateSize = 0;

            if (newManifestUrl != "")
            {
                await foreach (SophonAsset asset in SophonUpdate.EnumerateUpdateAsync(
                    httpClient,
                    manifestPairFrom,
                    manifestPairTo,
                    true,
                    null,
                    tokenSource.Token
                ))
                {
                    ProcessUpdateAsset(asset, ref updateSize, ref assets);
                }
            } else
            {
                await foreach (SophonAsset asset in SophonManifest.EnumerateAsync(
                    httpClient,
                    manifestPairFrom,
                    null,
                    tokenSource.Token
                ))
                {
                    ProcessAsset(asset, ref updateSize, ref assets);
                }
            }
            

            return new Tuple<List<SophonAsset>, long>(assets, updateSize);
        }

        private static void ProcessAsset(SophonAsset asset, ref long updateSize, ref List<SophonAsset> assets)
        {
            if (!asset.IsDirectory)
            {
                updateSize = updateSize + asset.AssetSize;
                assets.Add(asset);
            }
        }

        private static void ProcessUpdateAsset(SophonAsset asset, ref long updateSize, ref List<SophonAsset> assets)
        {
            if (!asset.IsDirectory)
            {
                bool isUpdated = false;
                SophonChunk[] chunks = asset.Chunks;
                int chunksLen = chunks.Length;

                for (int i = 0; i < chunksLen; i++)
                {
                    if (chunks[i].ChunkOldOffset != -1)
                    {
                        continue;
                    }

                    isUpdated = true;
                }

                if (isUpdated)
                {
                    updateSize = updateSize + asset.AssetSize;
                    assets.Add(asset);
                }
            }
        }
    }
}
