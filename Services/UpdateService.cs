using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace WinArma.Services
{
    public record UpdateInfo(string Tag, string Url);

    // Asks GitHub for the latest release tag and compares it with this build's version.
    // Nothing is downloaded or installed; the user opens the release page from the tray.
    public static class UpdateService
    {
        private const string Repo = "txt-yusuf/win-arma";

        public static Version CurrentVersion
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
                return new Version(v.Major, v.Minor, Math.Max(v.Build, 0));
            }
        }

        // Returns null when up to date or when GitHub cannot be reached
        public static async Task<UpdateInfo?> CheckAsync()
        {
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Win-Arma-UpdateCheck");
                http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

                var json = await http.GetStringAsync($"https://api.github.com/repos/{Repo}/releases/latest");
                using var doc = JsonDocument.Parse(json);

                var tag = doc.RootElement.GetProperty("tag_name").GetString() ?? string.Empty;
                var url = doc.RootElement.GetProperty("html_url").GetString() ?? $"https://github.com/{Repo}/releases";

                // Only ever open our own repo's release page
                if (!url.StartsWith($"https://github.com/{Repo}/", StringComparison.OrdinalIgnoreCase))
                {
                    url = $"https://github.com/{Repo}/releases";
                }

                if (Version.TryParse(tag.TrimStart('v', 'V'), out var latest) && latest > CurrentVersion)
                {
                    return new UpdateInfo(tag, url);
                }
            }
            catch
            {
                // Offline, rate-limited or malformed: stay silent
            }

            return null;
        }
    }
}
