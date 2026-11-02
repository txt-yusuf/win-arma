using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinArma.Models;
using WinArma.Utils;

namespace WinArma.Services
{
    // User-defined keywords from Settings -> Hızlı Komutlar (e.g. "yt" opens YouTube, "proje" opens a folder)
    public class CustomCommandService
    {
        public List<SearchResultItem> Match(string query)
        {
            var results = new List<SearchResultItem>();
            var commands = SettingsService.Instance.Current.Commands;
            if (commands == null || commands.Count == 0 || string.IsNullOrWhiteSpace(query)) return results;

            var clean = query.Trim();

            foreach (var kv in commands)
            {
                bool exact = string.Equals(clean, kv.Key, StringComparison.OrdinalIgnoreCase);
                bool prefix = clean.Length >= 2 && kv.Key.StartsWith(clean, StringComparison.OrdinalIgnoreCase);
                if (!exact && !prefix) continue;

                var target = Environment.ExpandEnvironmentVariables(kv.Value.Trim());
                bool isFolder = Directory.Exists(target);
                bool isUrl = target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                             target.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

                results.Add(new SearchResultItem
                {
                    Title = kv.Key,
                    Subtitle = target,
                    Category = SearchResultCategory.Command,
                    Icon = isUrl ? null : IconHelper.GetIconForFile(target),
                    IconSymbol = isUrl ? "🔗" : isFolder ? "📁" : "🚀",
                    ActionTarget = target,
                    Score = exact ? 1400 : 930
                });
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }
    }
}
