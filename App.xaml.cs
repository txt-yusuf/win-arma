using System;
using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;
using WinArma.Services;
using WinArma.Views;

namespace WinArma
{
    public partial class App : System.Windows.Application
    {
        private Forms.NotifyIcon? _notifyIcon;
        private MainWindow? _mainWindow;
        private SettingsWindow? _settingsWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var icoPath = System.IO.Path.Combine(baseDir, "app.ico");
                var pngPath = System.IO.Path.Combine(baseDir, "logo.png");
                Utils.IconGenerator.EnsureIconExists(icoPath, pngPath);

                // Start hidden in the tray: creating the handle registers the hotkey and starts indexing,
                // the launcher itself only appears on the hotkey / tray click
                _mainWindow = new MainWindow();
                new System.Windows.Interop.WindowInteropHelper(_mainWindow).EnsureHandle();
                SetupTrayIcon();

                SettingsService.Instance.SettingsChanged += OnSettingsChanged;

                ShowFirstRunHint();
                _ = CheckForUpdateAsync(manual: false);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Başlatma hatası: {ex.Message}", "Win-Arma", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSettingsChanged(Models.AppSettings settings)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Text = $"Win-Arma ({settings.HotkeyModifier} + Space)";
            }
        }

        private void SetupTrayIcon()
        {
            var initialSettings = SettingsService.Instance.Current;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var icoPath = System.IO.Path.Combine(baseDir, "app.ico");

            System.Drawing.Icon appIcon = SystemIcons.Application;
            if (System.IO.File.Exists(icoPath))
            {
                try
                {
                    appIcon = new System.Drawing.Icon(icoPath);
                }
                catch { }
            }

            _notifyIcon = new Forms.NotifyIcon
            {
                Icon = appIcon,
                Visible = true,
                Text = $"Win-Arma ({initialSettings.HotkeyModifier} + Space)"
            };

            var contextMenu = new Forms.ContextMenuStrip();

            var openItem = new Forms.ToolStripMenuItem("Arama Çubuğunu Aç");
            openItem.Font = new Font(openItem.Font, System.Drawing.FontStyle.Bold);
            openItem.Click += (s, e) => _mainWindow?.ToggleVisibility();

            var settingsItem = new Forms.ToolStripMenuItem("⚙️ Ayarlar");
            settingsItem.Click += (s, e) => OpenSettings();

            var updateCheckItem = new Forms.ToolStripMenuItem("Güncellemeleri denetle");
            updateCheckItem.Click += async (s, e) => await CheckForUpdateAsync(manual: true);

            var aboutItem = new Forms.ToolStripMenuItem("Hakkında");
            aboutItem.Click += (s, e) =>
            {
                System.Windows.MessageBox.Show(
                    "Win-Arma Spotlight\nmacOS Spotlight benzeri hızlı arama ve başlatıcı.\n\n© 2026",
                    "Win-Arma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            };

            var exitItem = new Forms.ToolStripMenuItem("Çıkış");
            exitItem.Click += (s, e) =>
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                Shutdown();
            };

            contextMenu.Items.Add(openItem);
            contextMenu.Items.Add(new Forms.ToolStripSeparator());
            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(updateCheckItem);
            contextMenu.Items.Add(aboutItem);
            contextMenu.Items.Add(new Forms.ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => _mainWindow?.ToggleVisibility();
            _notifyIcon.BalloonTipClicked += (s, e) => OpenUpdatePage();
        }

        private string _updateUrl = string.Empty;
        private Forms.ToolStripMenuItem? _updateItem;

        private void OpenUpdatePage()
        {
            if (string.IsNullOrEmpty(_updateUrl)) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_updateUrl) { UseShellExecute = true });
            }
            catch { }
        }

        // One-time "I live in the tray" hint, so a hidden start does not look like nothing happened
        private void ShowFirstRunHint()
        {
            var current = SettingsService.Instance.Current;
            if (current.FirstRunHintShown || _notifyIcon == null) return;

            _notifyIcon.ShowBalloonTip(5000, "Win-Arma çalışıyor",
                $"{current.HotkeyModifier} + Space ile açın. Ayarlar için simgeye sağ tıklayın.", Forms.ToolTipIcon.Info);

            var updated = current.Clone();
            updated.FirstRunHintShown = true;
            SettingsService.Instance.SaveSettings(updated);
        }

        private async System.Threading.Tasks.Task CheckForUpdateAsync(bool manual)
        {
            if (!manual)
            {
                if (!SettingsService.Instance.Current.CheckForUpdates) return;
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(20));
                if (!SettingsService.Instance.Current.CheckForUpdates) return;
            }

            var info = await UpdateService.CheckAsync();

            if (info == null)
            {
                if (manual)
                {
                    _notifyIcon?.ShowBalloonTip(3000, "Win-Arma", $"Güncelsiniz (v{UpdateService.CurrentVersion}).", Forms.ToolTipIcon.Info);
                }
                return;
            }

            _updateUrl = info.Url;

            if (_updateItem == null && _notifyIcon?.ContextMenuStrip != null)
            {
                _updateItem = new Forms.ToolStripMenuItem();
                _updateItem.Click += (s, e) => OpenUpdatePage();
                _notifyIcon.ContextMenuStrip.Items.Insert(1, _updateItem);
            }

            if (_updateItem != null) _updateItem.Text = $"⬆ Yeni sürüm {info.Tag} — indir";

            _notifyIcon?.ShowBalloonTip(6000, "Win-Arma güncellemesi var",
                $"{info.Tag} yayınlandı. İndirmek için tıklayın.", Forms.ToolTipIcon.Info);
        }

        public void OpenSettings()
        {
            if (_settingsWindow != null && _settingsWindow.IsLoaded)
            {
                _settingsWindow.Activate();
                _settingsWindow.Focus();
                return;
            }

            _settingsWindow = new SettingsWindow();
            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            base.OnExit(e);
        }
    }
}
