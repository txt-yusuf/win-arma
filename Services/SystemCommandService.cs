using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using WinArma.Models;
using WinArma.Utils;

namespace WinArma.Services
{
    public class SystemCommandService
    {
        private readonly List<(string Key, string Title, string Subtitle, Action Action, string? Confirm)> _commands = new();

        public SystemCommandService()
        {
            _commands.Add(("kilitle", "Bilgisayarı Kilitle", "Oturumu güvenli bir şekilde kilitler", () =>
            {
                NativeMethods.LockWorkStation();
            }, null));

            _commands.Add(("kapat", "Bilgisayarı Kapat", "Sistemi 30 saniye içinde kapatır (İptal: 'kapatmayı iptal')", () =>
            {
                Process.Start(new ProcessStartInfo("shutdown.exe", "/s /t 30") { CreateNoWindow = true, UseShellExecute = false });
            }, "Bilgisayar 30 saniye içinde kapatılacak. Devam edilsin mi?"));

            _commands.Add(("yeniden başlat", "Bilgisayarı Yeniden Başlat", "Sistemi yeniden başlatır", () =>
            {
                Process.Start(new ProcessStartInfo("shutdown.exe", "/r /t 10") { CreateNoWindow = true, UseShellExecute = false });
            }, "Bilgisayar 10 saniye içinde yeniden başlatılacak. Devam edilsin mi?"));

            _commands.Add(("kapatmayı iptal", "Kapatma / Yeniden Başlatmayı İptal Et", "Bekleyen kapatma geri sayımını durdurur", () =>
            {
                Process.Start(new ProcessStartInfo("shutdown.exe", "/a") { CreateNoWindow = true, UseShellExecute = false });
            }, null));

            _commands.Add(("çöpü boşalt", "Geri Dönüşüm Kutusunu Boşalt", "Silinmiş tüm geçici dosyaları temizler", () =>
            {
                NativeMethods.SHEmptyRecycleBin(IntPtr.Zero, null, NativeMethods.SHERB_NOCONFIRMATION | NativeMethods.SHERB_NOPROGRESSUI);
            }, "Geri Dönüşüm Kutusu kalıcı olarak boşaltılacak. Devam edilsin mi?"));

            _commands.Add(("uyku", "Uyku Modu", "Bilgisayarı düşük güç tasarrufu moduna alır", () =>
            {
                Process.Start(new ProcessStartInfo("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0") { CreateNoWindow = true, UseShellExecute = false });
            }, null));
        }

        // explicitMode: the query came after the ">" prefix, so an empty query lists everything and
        // short prefixes are fine. Otherwise only an exact name or a 3+ character prefix matches, so
        // ordinary typing never surfaces shutdown entries.
        public List<SearchResultItem> MatchCommands(string query, bool explicitMode = false)
        {
            var results = new List<SearchResultItem>();
            if (!explicitMode && string.IsNullOrWhiteSpace(query)) return results;

            var clean = query.Trim().ToLowerInvariant();

            foreach (var cmd in _commands)
            {
                bool match = explicitMode
                    ? clean.Length == 0 || cmd.Key.Contains(clean, StringComparison.Ordinal)
                    : clean == cmd.Key || (clean.Length >= 3 && cmd.Key.StartsWith(clean, StringComparison.Ordinal));
                if (!match) continue;

                var action = cmd.Action;
                var confirm = cmd.Confirm;
                var title = cmd.Title;

                results.Add(new SearchResultItem
                {
                    Title = cmd.Title,
                    Subtitle = cmd.Subtitle,
                    Category = SearchResultCategory.SystemCommand,
                    IconSymbol = "⚙️",
                    Score = 850,
                    CustomAction = confirm == null
                        ? action
                        : () =>
                        {
                            var answer = System.Windows.MessageBox.Show(confirm, title,
                                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                            if (answer == MessageBoxResult.Yes) action();
                        }
                });
            }

            return results;
        }
    }
}
