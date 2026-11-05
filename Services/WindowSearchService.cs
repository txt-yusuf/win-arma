using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WinArma.Models;
using WinArma.Utils;

namespace WinArma.Services
{
    // Lists open top-level windows so a title or app name jumps straight to them ("pencere" lists all)
    public class WindowSearchService
    {
        private record OpenWindow(IntPtr Handle, string Title, string Process);

        public List<SearchResultItem> Match(string query)
        {
            var results = new List<SearchResultItem>();
            if (!SettingsService.Instance.Current.EnableWindowSearch || string.IsNullOrWhiteSpace(query)) return results;

            var clean = query.Trim();
            bool listAll = clean.Equals("pencere", StringComparison.OrdinalIgnoreCase) ||
                           clean.Equals("pencereler", StringComparison.OrdinalIgnoreCase) ||
                           clean.Equals("windows", StringComparison.OrdinalIgnoreCase);

            if (!listAll && clean.Length < 3) return results;

            int score = listAll ? 960 : 420;

            foreach (var w in EnumerateWindows())
            {
                if (!listAll &&
                    !w.Title.Contains(clean, StringComparison.OrdinalIgnoreCase) &&
                    !w.Process.Contains(clean, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var handle = w.Handle;
                results.Add(new SearchResultItem
                {
                    Title = w.Title,
                    Subtitle = $"{w.Process} (Pencereye geçmek için Enter)",
                    Category = SearchResultCategory.Window,
                    IconSymbol = "🪟",
                    ActionTarget = w.Title,
                    Score = score--,
                    CustomAction = () => NativeMethods.BringToFront(handle)
                });

                if (results.Count >= 8) break;
            }

            return results;
        }

        private static List<OpenWindow> EnumerateWindows()
        {
            var found = new List<OpenWindow>();
            uint self = (uint)Environment.ProcessId;

            NativeMethods.EnumWindows((hWnd, _) =>
            {
                if (!NativeMethods.IsWindowVisible(hWnd)) return true;

                // Tool windows, windows owned by another window and DWM-cloaked ones (other virtual desktops,
                // suspended UWP shells) are not what the taskbar shows
                long exStyle = NativeMethods.GetWindowLongPtr(hWnd, NativeMethods.GWL_EXSTYLE).ToInt64();
                if ((exStyle & NativeMethods.WS_EX_TOOLWINDOW) != 0) return true;
                if (NativeMethods.GetWindow(hWnd, NativeMethods.GW_OWNER) != IntPtr.Zero) return true;
                if (NativeMethods.DwmGetWindowAttribute(hWnd, NativeMethods.DWMWA_CLOAKED, out int cloaked, sizeof(int)) == 0 && cloaked != 0) return true;

                int len = NativeMethods.GetWindowTextLength(hWnd);
                if (len == 0) return true;
                var sb = new StringBuilder(len + 1);
                NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
                var title = sb.ToString();
                if (string.IsNullOrWhiteSpace(title)) return true;

                NativeMethods.GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid == self) return true;

                string proc;
                try { proc = Process.GetProcessById((int)pid).ProcessName; }
                catch { proc = "?"; }

                if (proc.Equals("TextInputHost", StringComparison.OrdinalIgnoreCase) ||
                    title.Equals("Program Manager", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                found.Add(new OpenWindow(hWnd, title, proc));
                return true;
            }, IntPtr.Zero);

            return found;
        }
    }
}
