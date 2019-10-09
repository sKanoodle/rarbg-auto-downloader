using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TorrentAPI;

namespace RarbgAutoDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (Config.Instance.NoConfigFound)
            {
                Console.WriteLine("No config found. Please edit 'config.json' in the program directory.");
                Config.Instance.Save();
                Console.ReadKey();
                return;
            }

            var client = new RarbgApiClient("https://torrentapi.org/pubapi_v2.php", "rarbg_auto_downloader");
            var settings = new Settings()
            {
                Limit = 100,
                Mode = Mode.SearchString,
                Filters = new[] { Filter.XXX },
            };

            var links = GetWantedMagnetLinksAsync(client, settings);

            if (Config.Instance.TransmissionConfig.Address != default)
                await UploadMagnetLinksAsync(links);
            else
                File.WriteAllText(Path.GetFullPath("magnetlinks.txt"), String.Join(Environment.NewLine, await links.ToArrayAsync()));
            Console.WriteLine("finished");
            Console.ReadKey();
        }

        private static async IAsyncEnumerable<string> GetWantedMagnetLinksAsync(RarbgApiClient client, Settings baseSettings)
        {
            foreach (var series in Config.Instance.Series)
                foreach (var link in await GetWantedMagnetLinksAsync(new SeriesHandler(Path.Combine(Config.Instance.BasePath, series)), series, client, baseSettings))
                    yield return link;
            foreach (var actor in Config.Instance.Actors)
                foreach (var link in await GetWantedMagnetLinksAsync(new ActorHandler(Config.Instance.BasePath, actor), actor, client, baseSettings))
                    yield return link;
        }

        private static async Task<string[]> GetWantedMagnetLinksAsync(BaseHandler handler, string series, RarbgApiClient client, Settings baseSettings)
        {
            baseSettings.Search = $"{series}+1080";
            Console.WriteLine($"searching: {series}");

            var response = await client.GetResponseAsync(baseSettings);
            return response.Torrents
                .Where(t => handler.EpisodeWanted(t.Title) && !Config.Instance.IgnoreTorrentsStartingWith.Any(s => t.Title.StartsWith(s)))
                .Select(t => t.Download)
                .ToArray();
        }

        private static async Task UploadMagnetLinksAsync(IAsyncEnumerable<string> links)
        {
            var config = Config.Instance.TransmissionConfig;
            var client = new Transmission.Api.Client(config.Address, config.UserName, config.Password);
            await foreach (var link in links)
                await client.TorrentAddAsync(link, paused: !Config.Instance.TransmissionConfig.AutoStartTorrents);
        }
    }
}
