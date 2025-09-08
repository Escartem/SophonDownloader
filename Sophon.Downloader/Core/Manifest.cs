using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdStream = ZstdNet.DecompressionStream;

namespace Core
{
    public class Asset
    {
        public string Name;
        public long Size;
        public byte[] HashMd5;
        public List<Chunk> Chunks = new();
        public bool IsDirectory => false;
    }

    public class Chunk
    {
        public string Name;
        public long SizeDecompressed;
        public byte[] HashMd5;
        public long Offset;
        public byte[] Data;
    }

    internal class Manifest
    {
        public static async Task<Tuple<List<Asset>, long>> GetAssetsFromManifest(
            HttpClient httpClient,
            string manifestUrl,
            CancellationToken token)
        {
            Console.WriteLine(manifestUrl);
            byte[] manifestCompressed = await httpClient.GetByteArrayAsync(manifestUrl, token);

            var compressedStream = new MemoryStream(manifestCompressed);
            using var decompressedStream = new ZstdStream(compressedStream);
            var manifestProto = await Task<SophonManifestProto>.Factory.StartNew(
                () => SophonManifestProto.Parser.ParseFrom(decompressedStream),
                token,
                TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default
            );

            List<Asset> assets = new();
            long totalSize = 0;

            foreach (var protoAsset in manifestProto.Assets)
            {
                Asset asset = new()
                {
                    Name = protoAsset.AssetName,
                    Size = protoAsset.AssetSize,
                    HashMd5 = Convert.FromHexString(protoAsset.AssetHashMd5)
                };

                foreach (var protoChunk in protoAsset.AssetChunks)
                {
                    asset.Chunks.Add(new Chunk
                    {
                        Name = protoChunk.ChunkName,
                        SizeDecompressed = protoChunk.ChunkSizeDecompressed,
                        Offset = protoChunk.ChunkOnFileOffset,
                        HashMd5 = Convert.FromHexString(protoChunk.ChunkDecompressedHashMd5)
                    });
                }

                totalSize += asset.Size;
                assets.Add(asset);
            }

            return new Tuple<List<Asset>, long>(assets, totalSize);
        }
    }
}
