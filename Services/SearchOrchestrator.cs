using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinArma.Models;

namespace WinArma.Services
{
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

        private async Task<List<SearchResultItem>> SearchScopedAsync(char prefix, string rest, CancellationToken token)
        {
            var settings = SettingsService.Instance.Current;

            switch (prefix)
            {
                case '>':
                    return settings.EnableSystemCommands
                        ? _sysService.MatchCommands(rest, explicitMode: true)
                        : new List<SearchResultItem>();

                case '?':
                    return settings.EnableWebSearch && rest.Length > 0
                        ? _webService.GetWebActions(rest)
                        : new List<SearchResultItem>();

                default:
                    if (!settings.EnableFileSearch || rest.Length < 2) return new List<SearchResultItem>();
                    try
                    {
                        var files = await _fileService.SearchAsync(rest, maxResults: settings.MaxResultsCount);
                        return token.IsCancellationRequested ? new List<SearchResultItem>() : files;
                    }
                    catch
                    {
                        return new List<SearchResultItem>();
                    }
            }
        }

        public async Task<List<SearchResultItem>> SearchAsync(string query)
        {
            _currentCts?.Cancel();
            _currentCts = new CancellationTokenSource();
            var token = _currentCts.Token;

            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<SearchResultItem>();
            }

            var settings = SettingsService.Instance.Current;
            var trimmed = query.Trim();
            var combined = new List<SearchResultItem>();

            // Scoped modes: one leading character narrows the search to a single source
            //   >  system commands   ?  web search   /  files
            if (trimmed[0] is '>' or '?' or '/')
            {
                var scoped = await SearchScopedAsync(trimmed[0], trimmed.Substring(1).Trim(), token);
                if (token.IsCancellationRequested) return new List<SearchResultItem>();
                return scoped.OrderByDescending(x => x.Score).Take(settings.MaxResultsCount).ToList();
            }

            // 0. Custom quick commands from Settings (e.g. "yt", "proje")
            combined.AddRange(_commandService.Match(trimmed));

            // 1. Check Snippets (e.g. !mail, !iban, !tarih)
            if (settings.EnableSnippets)
            {
                var snippets = _snippetService.MatchSnippets(trimmed);
                combined.AddRange(snippets);
            }

            // 2. Check Clipboard History (e.g. "pano", "cb", or content match)
            if (settings.EnableClipboardHistory)
            {
                var clipboardResults = ClipboardHistoryService.Instance.Search(trimmed);
                combined.AddRange(clipboardResults);
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
                var sysResults = _sysService.MatchCommands(trimmed);
                combined.AddRange(sysResults);
            }

            // 5. Search Apps (instant in-memory)
            if (settings.EnableAppSearch)
            {
                var appResults = _appService.Search(trimmed);
                combined.AddRange(appResults);
            }

            // 5b. Open windows (title / app name)
            combined.AddRange(_windowService.Match(trimmed));

            // 6. Search Files asynchronously
            if (settings.EnableFileSearch)
            {
                try
                {
                    var fileResults = await _fileService.SearchAsync(trimmed, maxResults: 5);
                    if (!token.IsCancellationRequested)
                    {
                        combined.AddRange(fileResults);
                    }
                }
                catch
                {
                    // File search failure should not break overall search
                }
            }

            if (token.IsCancellationRequested)
            {
                return new List<SearchResultItem>();
            }

            // 7. Add Web Actions
            if (settings.EnableWebSearch)
            {
                var webResults = _webService.GetWebActions(trimmed);
                combined.AddRange(webResults);
            }

            return combined
                .OrderByDescending(x => x.Score)
                .Take(settings.MaxResultsCount)
                .ToList();
        }
    }
}
