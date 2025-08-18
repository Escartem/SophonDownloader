using Hi3Helper.Sophon.Infos;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
namespace Hi3Helper.Sophon.Infos
{
    public class SophonManifestInfo
    {
        public   string          ManifestBaseUrl        { get; internal set; }
        public   string          ManifestId             { get; internal set; }
        public   string          ManifestChecksumMd5    { get; internal set; }
        public   bool            IsUseCompression       { get; internal set; }
        public   long            ManifestSize           { get; internal set; }
        public   long            ManifestCompressedSize { get; internal set; }

        public string ManifestFileUrl => ManifestBaseUrl.TrimEnd('/') + '/' + ManifestId;
    }
}

namespace Hi3Helper.Sophon
{
    public static partial class SophonManifest
    {
        /// <summary>
        ///     Create Sophon Build Manifest Information. Please refer the API response to set the argument value.
        /// </summary>
        /// <param name="manifestBaseUrl">
        ///     The base URL for the manifest. To find the value, See the API section called: <c>manifest_download</c> ->
        ///     <c>url_prefix</c>
        /// </param>
        /// <param name="manifestChecksumMd5">
        ///     The MD5 hash of the manifest. To find the value, See the API section called: <c>manifest</c> -> <c>checksum</c>
        /// </param>
        /// <param name="manifestId">
        ///     The file name/id of the manifest. To find the value, See the API section called: <c>manifest</c> -> <c>id</c>
        /// </param>
        /// <param name="isUseCompression">
        ///     Determine the use of compression within the manifest. To find the value, See the API section called:
        ///     <c>manifest_download</c> -> <c>compression</c>
        /// </param>
        /// <param name="manifestSize">
        ///     The decompressed size of the manifest file. To find the value, See the API section called: <c>stats</c> ->
        ///     <c>uncompressed_size</c>
        /// </param>
        /// <param name="manifestCompressedSize">
        ///     The compressed size of the manifest file. To find the value, See the API section called: <c>stats</c> ->
        ///     <c>compressed_size</c>
        /// </param>
        /// <returns>Sophon Manifest Build Information instance</returns>
        public static SophonManifestInfo CreateManifestInfo(string manifestBaseUrl,
                                                            string manifestChecksumMd5,
                                                            string manifestId,
                                                            bool   isUseCompression,
                                                            long   manifestSize,
                                                            long   manifestCompressedSize = 0)
            => new()
            {
                ManifestBaseUrl        = manifestBaseUrl,
                ManifestChecksumMd5    = manifestChecksumMd5,
                ManifestId             = manifestId,
                IsUseCompression       = isUseCompression,
                ManifestSize           = manifestSize,
                ManifestCompressedSize = manifestCompressedSize
            };
    }
}