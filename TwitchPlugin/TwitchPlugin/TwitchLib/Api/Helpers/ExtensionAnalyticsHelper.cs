namespace TwitchLib.Api.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Helix.Models.Helpers;

    public static class ExtensionAnalyticsHelper
    {
        public static async Task<List<ExtensionAnalytics>> HandleUrlAsync(String url)
        {
            var cnts = await GetContentsAsync(url);
            var data = ExtractData(cnts);

            return data.Select(line => new ExtensionAnalytics(line)).ToList();
        }

        private static IEnumerable<String> ExtractData(IEnumerable<String> cnts)
        {
            return cnts.Where(line => line.Any(Char.IsDigit)).ToList();
        }

        private static async Task<String[]> GetContentsAsync(String url)
        {
            var client = new HttpClient();
            var lines = (await client.GetStringAsync(url)).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return lines;
        }
    }
}
