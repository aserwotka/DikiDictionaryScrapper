using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Printing.IndexedProperties;
using System.Xml;
using static SDK.Translation.TranslationGroup;
using static SDK.Translation;
using System.Threading;
using System.Threading.Tasks;

namespace SDK
{
    public class Translation
    {
        public class TranslationGroup
        {
            public enum GroupType
            {
                None,
                Native,
                Foreign
            }

            public class TranslationUnit
            {
                public string Phrase { get; set; } = string.Empty;
                public string Link { get; set; } = string.Empty;
                public List<string> Plurals { get; set; } = new();
                public List<string> Meanings { get; set; } = new();
            }

            public string Phrase { get; set; } = string.Empty;
            public GroupType Type { get; set; } = GroupType.None;
            public string PartOfSpeech { get; set; } = string.Empty;
            public List<TranslationUnit> Units { get; set; } = new();
        }

        public string SearchedPhrase { get; set; } = string.Empty;
        public List<TranslationGroup> Groups { get; set; } = new();
    }

    public class DikiAccessor
    {
        public async Task<List<Translation>> Request(List<string> phrases, IProgress<int> progress, CancellationToken cancellationToken)
        {
            List<Translation> results = new();

            int progressCount = 0;
            foreach (var phrase in phrases)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                results.Add(await Request(phrase));
                progress.Report(++progressCount);
            }

            return results;
        }

        private async Task<Translation> Request(string phrase)
        {
            var formatted = phrase.Trim().Replace(' ', '+');
            string url = $"https://www.diki.pl/slownik-niemieckiego?q={formatted}";
            HttpClient client = new HttpClient();

            using (HttpResponseMessage response = await client.GetAsync(url))
            {
                using (HttpContent content = response.Content)
                {
                    var pageContent = await content.ReadAsStringAsync();
                    Translation translation = await ParseTranslation(pageContent);
                    translation.SearchedPhrase = phrase;

                    return translation;
                }
            }
        }

        private async Task<List<string>> ParseForeignForPlurals(string phrase, string link)
        {
            if (link == string.Empty)
            {
                return new List<string>();
            }

            HttpClient client = new HttpClient();
            using (HttpResponseMessage response = await client.GetAsync(link))
            {
                using (HttpContent content = response.Content)
                {
                    string pageContent = await content.ReadAsStringAsync();
                    var lines = pageContent.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    HtmlDocument htmlSnippet = new();
                    htmlSnippet.LoadHtml(pageContent);
                    htmlSnippet.OptionEmptyCollection = true;

                    List<string> plurals = new();

                    foreach (HtmlNode node in htmlSnippet.DocumentNode.SelectNodes("//div[@class = 'dictionaryEntity']"))
                    {
                        var h1 = node.SelectSingleNode("div[@class = 'hws']/h1/span[@class = 'hw']");
                        if (h1 == null || !h1.InnerText.Contains(phrase))
                        {
                            continue;
                        }

                        foreach (HtmlNode pluralNode in node.SelectNodes("div[@class = 'pf']/span[@class]"))
                        {
                            plurals.Add(pluralNode.InnerText);
                        }
                    }

                    return plurals;
                }
            }
        }

        private async Task<TranslationUnit> parseUnit(HtmlNode dictionaryEntityNode, HtmlNode listItemNode, bool isNativeToForeign)
        {
            TranslationUnit unit = new();

            var linkNode = listItemNode.SelectSingleNode("a[@href]");
            if (linkNode != null)
            {
                var linkSufix = linkNode.GetAttributeValue("href", "");
                unit.Link = $"https://www.diki.pl{linkSufix}";
            }

            unit.Phrase = listItemNode.InnerText.Trim();

            if (isNativeToForeign)
            {
                unit.Plurals = await ParseForeignForPlurals(unit.Phrase, unit.Link);
            }
            else
            {
                var pluralNodes = dictionaryEntityNode.SelectNodes("div[@class = 'pf']/span[@class]");
                foreach (HtmlNode pluralNode in pluralNodes)
                {
                    unit.Plurals.Add(pluralNode.InnerText);
                }
            }

            var meaningNodes = listItemNode.SelectNodes("parent::li/ul/li[contains(@class, 'meaning')]/span[@class = 'hw']");
            foreach (HtmlNode meaningNode in meaningNodes)
            {
                unit.Meanings.Add(meaningNode.InnerText);
            }

            return unit;
        }

        private async Task<TranslationGroup> parseGroup(HtmlNode dictionaryEntityNode, bool isNativeToForeign)
        {
            TranslationGroup group = new();
            var partOfSpeech = dictionaryEntityNode.SelectSingleNode("div/span[@class = 'partOfSpeech']");
            group.PartOfSpeech = partOfSpeech?.InnerText ?? "";

            string token = isNativeToForeign ? "nativeToForeignEntrySlices" : "foreignToNativeMeanings";

            foreach (HtmlNode li in dictionaryEntityNode.SelectNodes($"ol[@class = '{token}']/li/span[@class = 'hw']"))
            {
                var unit = await parseUnit(dictionaryEntityNode, li, isNativeToForeign);
                group.Units.Add(unit);
                group.Type = isNativeToForeign ? GroupType.Native : GroupType.Foreign;
            }

            return group;
        }

        private async Task<Translation> ParseTranslation(string pageContent)
        {
            HtmlDocument htmlSnippet = new();
            htmlSnippet.LoadHtml(pageContent);
            htmlSnippet.OptionEmptyCollection = true;
            Translation translation = new();

            foreach (HtmlNode node in htmlSnippet.DocumentNode.SelectNodes("//div[@class = 'dictionaryEntity']"))
            {
                var nativeToForeignNode = node.SelectSingleNode("ol[@class = 'nativeToForeignEntrySlices']");
                var foreignToNativeNode = node.SelectSingleNode("ol[@class = 'foreignToNativeMeanings']");
                var h1 = node.SelectSingleNode("div[@class = 'hws']/h1/span[@class = 'hw']");

                bool groupNode = true;
                groupNode &= nativeToForeignNode is not null || foreignToNativeNode is not null;
                groupNode &= h1 is not null;

                if (!groupNode)
                {
                    continue;
                }

                var isNativeToForeign = nativeToForeignNode is not null;
                var group = await parseGroup(node, isNativeToForeign);
                group.Phrase = h1!.InnerText.Trim();

                translation.Groups.Add(group);
            }

            return translation;
        }
    }
}