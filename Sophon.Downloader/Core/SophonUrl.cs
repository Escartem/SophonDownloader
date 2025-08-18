using System.Text.Json;
using System.Web;

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

        public async Task<int> GetBuildData()
        {
            var uri = new UriBuilder(apiBase);
            var query = HttpUtility.ParseQueryString(uri.Query);

            query["game_ids[]"] = this.gameId;
            query["launcher_id"] = this.launcherId;
            uri.Query = query.ToString();

            var json = await FetchUrl(uri.ToString());
            var obj = JsonSerializer.Deserialize<BranchesRoot>(json);

            string[] data = ParseBuildData(obj, this.branch);

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

        private string[] ParseBuildData(BranchesRoot obj, BranchType searchBranch)
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
                string[] data = ParseBuildData(this.branchBackup, BranchType.Main);
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
