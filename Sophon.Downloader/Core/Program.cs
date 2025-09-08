using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;

namespace Core
{
    public class Program
    {
        public static bool silent = false;
        public static string action = "";
        public static string updateFrom = "";

        public static async Task<int> Main(params string[] args)
        {
            bool showHelp = false;
            // arguments
            string gameId = "";
            string updateFrom = "";
            string updateTo = "";
            string outputDir = "";
            string matchingField = "";
            // options
            string region = "OSREL"; // default region
            string branch = "main"; // default branch
            string launcherId = "";
            string platApp = "";
            int threads = Environment.ProcessorCount;
            int maxHttpHandle = 128;

            var options = new OptionSet {
                { "region=", "", v => region = v },
                { "branch=", "", v => branch = v },
                { "launcherId=", "", v => launcherId = v },
                { "platApp=", "", v => platApp = v },
                { "threads=", "", v => threads = int.Parse(v) },
                { "handles=", "", v => maxHttpHandle = int.Parse(v) },
                { "silent", "", v => silent = v != null },
                { "h|help", "show help", v => showHelp = v != null },
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);
                action = "";

                if (extra.Count > 1)
                {
                    action = extra[0].ToLower();
                }

                if (action == "full" && extra.Count >= 5)
                {
                    gameId = extra[1];
                    matchingField = extra[2];
                    updateFrom = extra[3];
                    outputDir = extra[4];
                }
                else if (action == "update" && extra.Count >= 6)
                {
                    gameId = extra[1];
                    matchingField = extra[2];
                    updateFrom = extra[3];
                    updateTo = extra[4];
                    outputDir = extra[5];
                }
                else
                {
                    showHelp = true;
                }
            }
            catch (OptionException e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine("Use --help for usage.");
                return 0;
            }

            if (!silent) Console.WriteLine($"Sophon.Downloader v{Assembly.GetExecutingAssembly().GetName().Version} - Made with love by @Escartem <3");

            if (showHelp)
            {
                string exeName = Process.GetCurrentProcess().ProcessName + ".exe";
                Console.WriteLine($"""
                    Usage:
                        {exeName} full <gameId> <package> <version> <outputDir> [options]                     Download full game assets
                        {exeName} update <gameId> <package> <updateFrom> <updateTo> <outputDir> [options]     Download update assets

                    Arguments:
                        <gameId>        Game ID, either hoyo id (hk4e, hkrpg, nap, bh2) or REL id (gopR6Cufr3, ...)
                        <package>       What to download, either "game" or for audio "zh-cn", "en-us", "ja-jp" or "ko-kr"
                        <version>       Version to download
                        <updateFrom>    Version to update from
                        <updateTo>      Version to update to
                        <outputDir>     Output directory to save the downloaded files

                    Options:
                        --region=<value>            Region to use, either OSREL (overseas) or CNREL (china), defaults to OSREL
                        --branch=<value>            Override branch name of the game data
                        --launcherId=<value>        Override launcher ID used when fetching packages
                        --platApp=<value>           Override platform application ID used when fetching packages
                        --threads=<value>           Number of threads to use, defaults to the number of processors
                        --handles=<value>           Number of HTTP handles to use, defaults to 128
                        --silent                    Suppress confirmation message and output
                        -h, --help                  Show this help message
                 """);
                return 0;
            }

            // main
            Enum.TryParse(region, out Region curRegion);
            BranchType curBranch = Enum.Parse<BranchType>(branch, true);
            Game game = new Game(curRegion, gameId);
            SophonUrl sophonUrl = new SophonUrl(curRegion, game.GetGameId(), curBranch, launcherId, platApp);
            await sophonUrl.GetBranchData();

            if (!silent) Console.WriteLine($"Running with {threads} threads and {maxHttpHandle} handles");

            if (updateFrom.Count(c => c == '.') == 1) updateFrom += ".0";
            string prevBuildUrl = sophonUrl.GetBuildUrl(updateFrom, false);
            (string prevManifest, string prevDownload) = await sophonUrl.GetBuildData(prevBuildUrl, matchingField);
            string newManifest = "";
            //if (action == "update")
            //{
            //    if (updateTo.Count(c => c == '.') == 1) updateTo += ".0";
            //    newManifest = sophonUrl.GetBuildUrl(updateTo, true);
            //}

            await Downloader.StartDownload(prevManifest, newManifest, threads, maxHttpHandle, outputDir, matchingField);

            return 0;
        }
    }
}
