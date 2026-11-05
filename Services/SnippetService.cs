using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WinArma.Models;

namespace WinArma.Services
{
    public class SnippetService
    {
        public List<SearchResultItem> MatchSnippets(string query)
        {
            if (!SettingsService.Instance.Current.EnableSnippets)
            {
                return new List<SearchResultItem>();
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<SearchResultItem>();
            }

            var clean = query.Trim().ToLowerInvariant();
            bool isSnippetTrigger = clean == "!" || clean == "snip" || clean == "snippet" || clean == "kısayol";
            bool startsWithBang = clean.StartsWith("!");

            var results = new List<SearchResultItem>();
            var settings = SettingsService.Instance.Current;

            // Built-in dynamic snippets
            var dynamicSnippets = new Dictionary<string, Func<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "!tarih", () => DateTime.Now.ToString("dd.MM.yyyy") },
                { "!saat", () => DateTime.Now.ToString("HH:mm") },
                { "!bugun", () => DateTime.Now.ToString("dd MMMM yyyy, dddd") },
                { "!guid", () => Guid.NewGuid().ToString() }
            };

            // Check dynamic snippets
            foreach (var kv in dynamicSnippets)
            {
                if (isSnippetTrigger || kv.Key.StartsWith(clean, StringComparison.OrdinalIgnoreCase) || clean.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
                {
                    string evaluated = kv.Value();
                    results.Add(CreateSnippetItem(kv.Key, evaluated, clean == kv.Key ? 1500 : 950));
                }
            }

            // Check user custom snippets
            if (settings.Snippets != null)
            {
                foreach (var kv in settings.Snippets)
                {
                    string key = kv.Key.StartsWith("!") ? kv.Key : "!" + kv.Key;
                    if (isSnippetTrigger || key.StartsWith(clean, StringComparison.OrdinalIgnoreCase) || clean.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                    {
                        int score = string.Equals(clean, key, StringComparison.OrdinalIgnoreCase) ? 1500 : 920;
                        results.Add(CreateSnippetItem(key, kv.Value, score));
                    }
                }
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }

        private SearchResultItem CreateSnippetItem(string key, string value, int score)
        {
            string preview = value.Length > 60 ? value.Substring(0, 57) + "..." : value;
            var capturedVal = value;

            return new SearchResultItem
            {
                Title = $"{key} ➔ {preview}",
                Subtitle = $"Metin Kısayolu (Panoya kopyalamak için Enter)",
                Category = SearchResultCategory.Snippet,
                IconSymbol = "⚡",
                ActionTarget = value,
                Score = score,
                CustomAction = () =>
                {
                    try
                    {
                        // Snippets often hold an IBAN or phone number: keep them out of the clipboard history
                        ClipboardHistoryService.SetTextPrivate(capturedVal);
                    }
                    catch { }
                }
            };
        }
    }
}
