using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDK
{
    public class UpdateChecker
    {
        public static Version? GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static string GetUrl()
        {
            return "https://github.com/aserwotka/DikiDictionaryScrapper/releases";
        }

        public async Task<bool> CheckNewVersionAvailable()
        {
            HttpClient client = new HttpClient();
            List<Version> versions;

            using (HttpResponseMessage response = await client.GetAsync(GetUrl()))
            {
                using (HttpContent content = response.Content)
                {
                    var pageContent = await content.ReadAsStringAsync();
                    HtmlDocument htmlSnippet = new();
                    htmlSnippet.LoadHtml(pageContent);
                    htmlSnippet.OptionEmptyCollection = true;
                    versions = parseVersions(htmlSnippet);
                }
            }

            var currentVersion = GetCurrentVersion();

            if(currentVersion == null)
            {
                return false;
            }

            return versions.Any(version => currentVersion.CompareTo(version) < 0);
        }

        private List<Version> parseVersions(HtmlDocument document)
        {
            var nodes = document.DocumentNode.Descendants("a").Where(x => x.Attributes["href"] != null && x.Attributes["href"].Value.Contains("/aserwotka/DikiDictionaryScrapper/tree/"));
            var versions = new List<Version>();
            foreach (var node in nodes)
            {
                var str = node.GetAttributeValue("href", "???");
                var parts = str.Split("/");
                if (parts.Length > 0)
                {
                    string pattern = @"^\d+\.\d+(\.\d+)?$";
                    var trimmed = Regex.Replace(parts.Last(), "^[a-zA-Z]*", "");

                    if (Regex.IsMatch(trimmed, pattern))
                    {
                        versions.Add(new Version(trimmed));
                    }
                }
            }

            return versions;
        }
    }
}
