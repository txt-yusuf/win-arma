using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using WinArma.Models;

namespace WinArma.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private readonly string _settingsFilePath;
        private AppSettings _currentSettings;

        public event Action<AppSettings>? SettingsChanged;

        public AppSettings Current => _currentSettings;

        public SettingsService()
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinArma");
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            _settingsFilePath = Path.Combine(appDataFolder, "settings.json");
            _currentSettings = LoadSettings();
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        // The JSON deserializer replaces the default dictionary with a case-sensitive one
                        settings.Snippets = new Dictionary<string, string>(
                            settings.Snippets ?? new Dictionary<string, string>(),
                            StringComparer.OrdinalIgnoreCase);
                        settings.Commands = new Dictionary<string, string>(
                            settings.Commands ?? new Dictionary<string, string>(),
                            StringComparer.OrdinalIgnoreCase);

                        // Check actual startup registry status
                        settings.RunAtStartup = CheckStartupRegistry();
                        return settings;
                    }
                }
            }
            catch
            {
                // Fallback to defaults on corrupt config
            }

            var defaultSettings = new AppSettings();
            defaultSettings.RunAtStartup = CheckStartupRegistry();
            return defaultSettings;
        }

        public void SaveSettings(AppSettings settings)
        {
            _currentSettings = settings;

            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch { }

            // Update Startup registry
            SetStartupRegistry(settings.RunAtStartup);

            // Notify listeners
            SettingsChanged?.Invoke(_currentSettings);
        }

        private bool CheckStartupRegistry()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue("WinArma") != null;
            }
            catch
            {
                return false;
            }
        }

        private void SetStartupRegistry(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) return;

                if (enable)
                {
                    var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue("WinArma", $"\"{exePath}\"");
                    }
                }
                else
                {
                    if (key.GetValue("WinArma") != null)
                    {
                        key.DeleteValue("WinArma");
                    }
                }
            }
            catch { }
        }
    }
}
