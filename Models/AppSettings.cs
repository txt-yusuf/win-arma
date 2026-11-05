using System;
using System.Collections.Generic;

namespace WinArma.Models
{
    public class AppSettings
    {
        // Hotkey Settings
        public string HotkeyModifier { get; set; } = "Alt"; // "Alt", "Ctrl", "Win", "Alt+Shift"
        public string HotkeyKey { get; set; } = "Space";    // "Space", "F1", "F2", etc.

        // Search Engine
        public string DefaultSearchEngine { get; set; } = "Google"; // "Google", "Bing", "DuckDuckGo", "Yandex", "Brave"

        // Search Scope / Modules Enabled
        public bool EnableAppSearch { get; set; } = true;
        public bool EnableFileSearch { get; set; } = true;
        public bool EnableWebSearch { get; set; } = true;
        public bool EnableCalculator { get; set; } = true;
        public bool EnableSystemCommands { get; set; } = true;

        // New Power Features
        public bool EnableClipboardHistory { get; set; } = true;
        public bool EnableQuickLook { get; set; } = true;
        public bool EnableSnippets { get; set; } = true;

        // Snippets Collection (Key -> Text)
        public Dictionary<string, string> Snippets { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            { "!mail", "ornek@sirket.com" },
            { "!iban", "TR00 0000 0000 0000 0000 0000 00" },
            { "!tel", "+90 555 123 4567" },
            { "!adres", "Atatürk Mah. Cumhuriyet Cad. No:1 İstanbul" },
            { "!shrug", @"¯\_(ツ)_/¯" }
        };

        // Custom quick commands (Keyword -> URL / file / folder / program)
        public Dictionary<string, string> Commands { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            { "yt", "https://www.youtube.com" },
            { "github", "https://github.com" },
            { "indirilenler", @"%USERPROFILE%\Downloads" }
        };

        // Open window switcher
        public bool EnableWindowSearch { get; set; } = true;

        // Update notification (asks GitHub for the latest release tag, nothing else is sent)
        public bool CheckForUpdates { get; set; } = true;

        // True once the "I'm running in the tray" hint has been shown
        public bool FirstRunHintShown { get; set; } = false;

        // UI & Appearance
        public int MaxResultsCount { get; set; } = 8;
        public double WindowOpacity { get; set; } = 0.98;
        public string Theme { get; set; } = "Dark"; // "Dark", "Midnight", "Amoled", "Light"

        // Startup
        public bool RunAtStartup { get; set; } = false;

        // Copy with its own dictionaries, so callers can change fields without losing the ones they don't know about
        public AppSettings Clone()
        {
            var copy = (AppSettings)MemberwiseClone();
            copy.Snippets = new Dictionary<string, string>(Snippets ?? new(), StringComparer.OrdinalIgnoreCase);
            copy.Commands = new Dictionary<string, string>(Commands ?? new(), StringComparer.OrdinalIgnoreCase);
            return copy;
        }

        public string GetSearchEngineUrl(string query)
        {
            var encoded = Uri.EscapeDataString(query);
            return DefaultSearchEngine switch
            {
                "Bing" => $"https://www.bing.com/search?q={encoded}",
                "DuckDuckGo" => $"https://duckduckgo.com/?q={encoded}",
                "Yandex" => $"https://yandex.com/search/?text={encoded}",
                "Brave" => $"https://search.brave.com/search?q={encoded}",
                _ => $"https://www.google.com/search?q={encoded}"
            };
        }
    }
}
