using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hi3Helper.Sophon.Structs;
using Hi3Helper.Sophon;
using System.Threading.Tasks.Dataflow;

namespace Core
{
    internal class Downloader
    {
        private static string _cancelMessage = string.Empty;
        private static bool _isRetry = true;

        public static async Task<int> StartDownload(string prevManifestUrl, string newManifestUrl, int threads, int maxHttpHandle, string outputDir, string matchingField)
        {

        InternalStartDownload:
            _isRetry = false;
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                _cancelMessage = "[\"C\"] Stop or [\"R\"] Restart";
                using (HttpClientHandler httpHandler = new HttpClientHandler
                {
                    MaxConnectionsPerServer = maxHttpHandle
                })
                using (HttpClient httpClient = new HttpClient(httpHandler)
                {
                    DefaultRequestVersion = HttpVersion.Version30,
                    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
                })
                {
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

                    // fetch assets
                    if (!Program.silent) Console.WriteLine("Fetching assets...");

                    //(List<SophonAsset> sophonAssets, long updateSize) = await Assets.GetAssetsFromManifests(
                    //    httpClient,
                    //    matchingField,
                    //    prevManifestUrl,
                    //    newManifestUrl,
                    //    tokenSource
                    //);
                    (List<Asset> sophonAssets, long updateSize) = await Manifest.GetAssetsFromManifest(
                        httpClient,
                        prevManifestUrl,
                        tokenSource.Token
                    );

                    //long totalSizeDiff = sophonAssets.GetCalculatedDiffSize(true);
                    long totalSizeDiff = 0;
                    string totalSizeDiffUnit = Utils.FormatSize(totalSizeDiff);
                    string totalSizeUnit = Utils.FormatSize(updateSize);

                    if (!Program.silent)
                    {
                        Console.WriteLine($"* Found {sophonAssets.Count} assets");
                        if (newManifestUrl != "")
                        {
                            Console.WriteLine($"* Update data is {totalSizeDiffUnit}");
                            Console.WriteLine($"* Because the full assets will be downloaded, total download size is {totalSizeUnit}");
                        }
                        else
                        {
                            Console.WriteLine($"* Total download size is {totalSizeUnit}");
                        }

                        Console.Write("Continue? (y/n): ");
                        var input = Console.ReadLine()?.Trim().ToLower();
                        bool yes = input == "y" || input == "yes";

                        if (!yes)
                        {
                            Console.WriteLine("Aborting...");
                            return 0;
                        }
                    }

                    long currentRead = 0;
                    Task.Run(() => AppExitTrigger(tokenSource));

                    ParallelOptions parallelOptions = new ParallelOptions
                    {
                        CancellationToken = tokenSource.Token,
                        MaxDegreeOfParallelism = threads
                    };

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    try
                    {
                        foreach (string fileTemp in Directory.EnumerateFiles(outputDir, "*_tempUpdate", SearchOption.AllDirectories))
                        {
                            File.Delete(fileTemp);
                        }

                        _isRetry = false;

                        ActionBlock<Tuple<Asset, HttpClient>> downloadTaskQueue = new(
                            async ctx =>
                            {
                                Asset asset = ctx.Item1;
                                HttpClient client = ctx.Item2;

                                //await asset.WriteUpdateAsync(
                                //    client,
                                //    outputDir,
                                //    outputDir,
                                //    outputDir,
                                //    false,
                                //    read =>
                                //    {
                                //        Interlocked.Add(ref currentRead, read);
                                //        string sizeUnit = Utils.FormatSize(currentRead);
                                //        string speedUnit = Utils.FormatSize(currentRead / stopwatch.Elapsed.TotalSeconds);
                                //        if (!Program.silent) Console.Write($"{_cancelMessage} | {sizeUnit}/{totalSizeUnit} ({totalSizeDiffUnit} diff) ({speedUnit}/s) \r");
                                //    },
                                //    null,
                                //    null,
                                //    tokenSource.Token
                                //);

                                //string outputPath = Path.Combine(outputDir, asset.AssetName);
                                //string outputTempPath = outputPath + "_tempUpdate";

                                //System.IO.File.Move(outputTempPath, outputPath, true);
                            },
                        new ExecutionDataflowBlockOptions
                        {
                                CancellationToken = tokenSource.Token,
                                MaxDegreeOfParallelism = threads,
                                MaxMessagesPerTask = threads,
                            });

                        foreach (Asset asset in sophonAssets)
                        {
                            await downloadTaskQueue.SendAsync(new Tuple<Asset, HttpClient>(asset, httpClient), tokenSource.Token);
                        }

                        downloadTaskQueue.Complete();
                        await downloadTaskQueue.Completion;
                    }
                    catch (OperationCanceledException)
                    {
                        if (!Program.silent)
                        {
                            Console.WriteLine(""); // failsafe
                            Console.WriteLine("Cancelled !");
                        }
                    }
                    finally
                    {
                        stopwatch.Stop();
                    }
                };
            }

            if (_isRetry)
                goto InternalStartDownload;

            return 0;
        }

        private static void AppExitTrigger(CancellationTokenSource tokenSource)
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.C:
                        _cancelMessage = "Canceling...";
                        tokenSource.Cancel();
                        return;
                    case ConsoleKey.R:
                        _isRetry = true;
                        tokenSource.Cancel();
                        return;
                }
            }
        }
    }
}
