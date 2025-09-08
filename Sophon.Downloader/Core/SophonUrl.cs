using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core
{
    #region getGamesBranches structure
    public class BranchesRoot
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public BranchesData data { get; set; }
    }

    public class BranchesData
    {
        public List<BranchesGameBranch> game_branches { get; set; }
    }

    public class BranchesGameBranch
    {
        public BranchesGame game { get; set; }
        public BranchesMain main { get; set; }
        public BranchesMain pre_download { get; set; }
    }

    public class BranchesGame
    {
        public string id { get; set; }
        public string biz { get; set; }
    }

    public class BranchesMain
    {
        public string package_id { get; set; }
        public string branch { get; set; }
        public string password { get; set; }
        public string tag { get; set; }
        public List<string> diff_tags { get; set; }
        public List<BranchesCategory> categories { get; set; }
    }

    public class BranchesCategory
    {
        public string category_id { get; set; }
        public string matching_field { get; set; }
    }
    #endregion

    #region getBuild structure
    public class BuildResponse
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public BuildData data { get; set; }
    }

    public class BuildData
    {
        public string BuildId { get; set; }
        public string Tag { get; set; }
        public List<ManifestEntry> Manifests { get; set; }
    }

    public class ManifestEntry
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public ManifestInfo Manifest { get; set; }
        public DownloadInfo ChunkDownload { get; set; }
        public DownloadInfo ManifestDownload { get; set; }
        public string MatchingField { get; set; }
        public StatsInfo Stats { get; set; }
        public StatsInfo DeduplicatedStats { get; set; }
    }


    public class ManifestInfo
    {
        public string Id { get; set; }
        public string Checksum { get; set; }
        public string CompressedSize { get; set; }
        public string UncompressedSize { get; set; }
    }

    public class DownloadInfo
    {
        public int Encryption { get; set; }
        public string Password { get; set; }
        public int Compression { get; set; }
        public string UrlPrefix { get; set; }
        public string UrlSuffix { get; set; }
    }

    public class StatsInfo
    {
        public string CompressedSize { get; set; }
        public string UncompressedSize { get; set; }
        public string FileCount { get; set; }
        public string ChunkCount { get; set; }
    }
        #endregion

    public enum Region
    {
        OSREL,
        CNREL
    }

    public enum BranchType
    {
        Main,
        PreDownload
    }

    public class SophonUrl
    {
        private string apiBase { get; set; }
        private string sophonBase { get; set; }
        private string gameId { get; set; }
        private BranchType branch {  get; set; }
        private string launcherId { get; set; }
        private string platApp { get; set; }
        private string gameBiz { get; set; } = "";
        private string packageId { get; set; } = "";
        private string password { get; set; } = "";
        private BranchesRoot branchBackup { get; set; } = new BranchesRoot();

        public SophonUrl(Region region, string gameId, BranchType branch = BranchType.Main, string launcherId = "", string platApp = "")
        {
            UpdateRegion(region);
            this.gameId = gameId;
            this.branch = branch;
            if (!string.IsNullOrEmpty(launcherId)) this.launcherId = launcherId;
            if (!string.IsNullOrEmpty(platApp)) this.platApp = platApp;
        }

        public void UpdateRegion(Region region)
        {
            switch (region)
            {
                case Region.OSREL:
                    this.apiBase = "https://sg-hyp-api.hoyoverse.com/hyp/hyp-connect/api/getGameBranches";
                    this.sophonBase = "https://sg-public-api.hoyoverse.com/downloader/sophon_chunk/api/getBuild";
                    this.launcherId = "VYTpXlbWo8";
                    this.platApp = "ddxf6vlr1reo";
                    break;
                case Region.CNREL:
                    this.apiBase = "https://hyp-api.mihoyo.com/hyp/hyp-connect/api/getGameBranches";
                    this.sophonBase = "https://api-takumi.mihoyo.com/downloader/sophon_chunk/api/getBuild";
                    this.launcherId = "jGHBHlcOq1";
                    this.platApp = "ddxf5qt290cg";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(region), region, null);
            }
        }

        public async Task<int> GetBranchData()
        {
            var uri = new UriBuilder(apiBase);
            var query = HttpUtility.ParseQueryString(uri.Query);

            query["game_ids[]"] = this.gameId;
            query["launcher_id"] = this.launcherId;
            uri.Query = query.ToString();

            var json = await FetchUrl(uri.ToString());
            var obj = JsonSerializer.Deserialize<BranchesRoot>(json);

            string[] data = ParseBranchData(obj, this.branch);

            if (data[0] != "OK")
            {
                Console.WriteLine($"Error: {data[1]}");
                return -1;
            }

            this.gameBiz = data[1];
            this.packageId = data[2];
            this.password = data[3];

            this.branchBackup = obj;

            return 0;
        }

        private string[] ParseBranchData(BranchesRoot obj, BranchType searchBranch)
        {
            if ((!obj.retcode.Equals(0) && obj.message == "OK"))
            {
                return ["ERROR", obj.message];
            }

            var branchObj = GetBranch(obj, searchBranch);
            if (branchObj == null)
            {
                return ["ERROR", $"Branch {searchBranch} not found"];
            }

            var gameObj = GetBranchGame(obj);

            return ["OK", gameObj.biz, branchObj.package_id, branchObj.password];
        }

        public string GetBuildUrl(string version, bool isUpdate = false)
        {
            var uri = new UriBuilder(sophonBase);
            var query = HttpUtility.ParseQueryString(uri.Query);

            if (Program.action == "update" && !isUpdate)
            {
                query["branch"] = BranchType.Main.ToString().ToLower();
                string[] data = ParseBranchData(this.branchBackup, BranchType.Main);
                query["package_id"] = data[2];
                query["password"] = data[3];
            } else
            {
                query["branch"] = this.branch.ToString().ToLower();
                query["package_id"] = this.packageId;
                query["password"] = this.password;
            }

            query["plat_app"] = this.platApp;
            query["tag"] = version;

            uri.Query = query.ToString();
            return uri.ToString();
        }

        public async Task<(string, string)> GetBuildData(string buildUrl, string matchingField)
        {
            Console.WriteLine($"Fetching build data from {buildUrl}");
            var json = await FetchUrl(buildUrl.ToString());
            Console.WriteLine(json);
            var obj = JsonSerializer.Deserialize<BuildResponse>(json);

            if ((!obj.Retcode.Equals(0) && obj.Message == "OK"))
            {
                Console.WriteLine($"Error: {obj.Message}");
                return ("ERROR", obj.Message);
            }

            ManifestEntry manifest = default;

            foreach (var e in obj.Data.Manifests)
            {
                if (e.MatchingField == matchingField)
                {
                    manifest = e;
                    break;
                }
            }

            string manifestUrl = $"{manifest.ManifestDownload.UrlPrefix}/{manifest.Manifest.Id}";
            string downloadUrl = $"{manifest.ChunkDownload.UrlPrefix}";

            return (manifestUrl, downloadUrl);
        }

        private static async Task<string> FetchUrl(string url)
        {
            using var client = new HttpClient();
            return await client.GetStringAsync(url);
        }

        private static BranchesGame? GetBranchGame(BranchesRoot obj)
        {
            return obj.data.game_branches.FirstOrDefault()?.game;
        }

        private static BranchesMain? GetBranch(BranchesRoot obj, BranchType searchBranch)
        {
            // since response is deserialized, searching is useless, we assume we have main and predownload only

            if (searchBranch is BranchType.Main)
            {
                return obj.data.game_branches.FirstOrDefault().main;
            }
            else if (searchBranch is BranchType.PreDownload)
            {
                return obj.data.game_branches.FirstOrDefault().pre_download;
            }
            else
            {
                return null;
            }
        }
    }
}
