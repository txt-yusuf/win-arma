using System;
using System.Windows.Media;

namespace WinArma.Models
{
    public enum SearchResultCategory
    {
        Application,
        File,
        Folder,
        WebSearch,
        Calculator,
        SystemCommand,
        Url,
        ClipboardItem,
        Snippet,
        Command,
        Window
    }

    public class SearchResultItem
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public SearchResultCategory Category { get; set; }
        public ImageSource? Icon { get; set; }
        public string? IconSymbol { get; set; } // Fallback emoji / symbol like 🚀, 📄, 🌐, 🧮, ⚙️, 📋, ⚡
        public string ActionTarget { get; set; } = string.Empty;
        public Action? CustomAction { get; set; }
        public int Score { get; set; }

        public string CategoryBadgeText => Category switch
        {
            SearchResultCategory.Application => "Uygulama",
            SearchResultCategory.File => "Dosya",
            SearchResultCategory.Folder => "Klasör",
            SearchResultCategory.WebSearch => "Web",
            SearchResultCategory.Calculator => "Hesap",
            SearchResultCategory.SystemCommand => "Sistem",
            SearchResultCategory.Url => "Bağlantı",
            SearchResultCategory.ClipboardItem => "Pano",
            SearchResultCategory.Snippet => "Kısayol",
            SearchResultCategory.Command => "Komut",
            SearchResultCategory.Window => "Pencere",
            _ => "Genel"
        };

        public string CategoryBadgeColor => Category switch
        {
            SearchResultCategory.Application => "#3B82F6", // Blue
            SearchResultCategory.File => "#10B981",        // Emerald
            SearchResultCategory.Folder => "#F59E0B",      // Amber
            SearchResultCategory.WebSearch => "#8B5CF6",   // Purple
            SearchResultCategory.Calculator => "#EC4899",  // Pink
            SearchResultCategory.SystemCommand => "#EF4444",// Red
            SearchResultCategory.Url => "#06B6D4",         // Cyan
            SearchResultCategory.ClipboardItem => "#F97316",// Orange
            SearchResultCategory.Snippet => "#14B8A6",     // Teal
            SearchResultCategory.Command => "#6366F1",     // Indigo
            SearchResultCategory.Window => "#0EA5E9",      // Sky
            _ => "#6B7280"
        };
    }
}
