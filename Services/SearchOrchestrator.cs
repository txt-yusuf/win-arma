using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinArma.Models;

namespace WinArma.Services
{
    // Two phases: Search() answers from memory in a few milliseconds (commands, snippets, calculator,
    // apps, windows, web), SearchFilesAsync() adds file hits afterwards. The Windows Search index can take
    // seconds, and nothing else should wait for it.
    public class SearchOrchestrator
    {
        private readonly AppSearchService _appService = new();
        private readonly FileSearchService _fileService = new();
        private readonly WebSearchService _webService = new();
        private readonly CalculatorService _calcService = new();
        private readonly SystemCommandService _sysService = new();
        private readonly SnippetService _snippetService = new();
        private readonly CustomCommandService _commandService = new();
        private readonly WindowSearchService _windowService = new();

        private CancellationTokenSource? _currentCts;

        public async Task InitializeAsync()
        {
            await _appService.InitializeAsync();
        }

        // Phase 1: everything that does not touch the disk index
        public List<SearchResultItem> Search(string query)
        {
            _currentCts?.Cancel();
            _currentCts = new CancellationTokenSource();

            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<SearchResultItem>();
            }

            var settings = SettingsService.Instance.Current;
            var trimmed = query.Trim();
            var combined = new List<SearchResultItem>();

            // Scoped modes: one leading character narrows the search to a single source
            //   >  system commands   ?  web search   /  files (phase 2)
            if (trimmed[0] is '>' or '?' or '/')
            {
                var rest = trimmed.Substring(1).Trim();

                if (trimmed[0] == '>' && settings.EnableSystemCommands)
                {
                    combined.AddRange(_sysService.MatchCommands(rest, explicitMode: true));
                }
                else if (trimmed[0] == '?' && settings.EnableWebSearch && rest.Length > 0)
                {
                    combined.AddRange(_webService.GetWebActions(rest));
                }

                return Finish(combined);
            }

            // 0. Custom quick commands from Settings (e.g. "yt", "proje")
            combined.AddRange(_commandService.Match(trimmed));

            // 1. Check Snippets (e.g. !mail, !iban, !tarih)
            if (settings.EnableSnippets)
            {
                combined.AddRange(_snippetService.MatchSnippets(trimmed));
            }

            // 2. Check Clipboard History (e.g. "pano", "cb", or content match)
            if (settings.EnableClipboardHistory)
            {
                combined.AddRange(ClipboardHistoryService.Instance.Search(trimmed));
            }

            // 3. Check Calculator
            if (settings.EnableCalculator)
            {
                var calcResult = _calcService.TryCalculate(trimmed);
                if (calcResult != null)
                {
                    combined.Add(calcResult);
                }
            }

            // 4. Check System commands
            if (settings.EnableSystemCommands)
            {
                combined.AddRange(_sysService.MatchCommands(trimmed));
            }

            // 5. Search Apps (instant in-memory)
            if (settings.EnableAppSearch)
            {
                combined.AddRange(_appService.Search(trimmed));
            }

            // 5b. Open windows (title / app name)
            combined.AddRange(_windowService.Match(trimmed));

            // 6. Add Web Actions
            if (settings.EnableWebSearch)
            {
                combined.AddRange(_webService.GetWebActions(trimmed));
            }

            return Finish(combined);
        }

        // Phase 2: file hits. Returns nothing when a newer Search() has started in the meantime.
        public async Task<List<SearchResultItem>> SearchFilesAsync(string query)
        {
            var token = _currentCts?.Token ?? CancellationToken.None;
            var settings = SettingsService.Instance.Current;

            if (!settings.EnableFileSearch || string.IsNullOrWhiteSpace(query)) return new List<SearchResultItem>();

            var trimmed = query.Trim();
            int max = 5;

            if (trimmed[0] is '>' or '?') return new List<SearchResultItem>();
            if (trimmed[0] == '/')
            {
                trimmed = trimmed.Substring(1).Trim();
                max = settings.MaxResultsCount;
                if (trimmed.Length < 2) return new List<SearchResultItem>();
            }

            try
            {
                var files = await _fileService.SearchAsync(trimmed, maxResults: max);
                return token.IsCancellationRequested ? new List<SearchResultItem>() : files;
            }
            catch
            {
                // File search failure should not break overall search
                return new List<SearchResultItem>();
            }
        }

        // Re-ranks phase-1 results together with the late file hits
        public List<SearchResultItem> Merge(List<SearchResultItem> fast, List<SearchResultItem> files) =>
            Finish(fast.Concat(files).ToList());

        private static List<SearchResultItem> Finish(List<SearchResultItem> items) =>
            items
                .OrderByDescending(x => x.Score)
                .Take(SettingsService.Instance.Current.MaxResultsCount)
                .ToList();
    }
}
