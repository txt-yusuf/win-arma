using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WinArma.Models;
using WinArma.Services;
using WinArma.Utils;
using WinArma.Views;

namespace WinArma
{
    public partial class MainWindow : Window
    {
        private readonly HotkeyManager _hotkeyManager = new();
        private readonly SearchOrchestrator _orchestrator = new();
        private bool _isInitialized = false;
        private int _searchVersion = 0;
        private double _currentListHeight = 0;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Deactivated += MainWindow_Deactivated;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PositionWindow();
            ApplySettings(SettingsService.Instance.Current);
            SettingsService.Instance.SettingsChanged += ApplySettings;

            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var icoPath = System.IO.Path.Combine(baseDir, "app.ico");
                if (System.IO.File.Exists(icoPath))
                {
                    Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(icoPath));
                }
            }
            catch { }

            await _orchestrator.InitializeAsync();
            _isInitialized = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            var settings = SettingsService.Instance.Current;

            uint mod = settings.HotkeyModifier switch
            {
                "Ctrl" => NativeMethods.MOD_CONTROL,
                "Win" => NativeMethods.MOD_WIN,
                "Alt+Shift" => NativeMethods.MOD_ALT | NativeMethods.MOD_SHIFT,
                "Ctrl+Alt" => NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT,
                _ => NativeMethods.MOD_ALT
            };

            bool success = _hotkeyManager.Register(helper.Handle, mod, NativeMethods.VK_SPACE);

            if (!success)
            {
                // Fallback
                _hotkeyManager.Register(helper.Handle, NativeMethods.MOD_CONTROL, NativeMethods.VK_SPACE);
            }

            _hotkeyManager.HotkeyPressed += OnHotkeyPressed;

            var hwndSource = HwndSource.FromHwnd(helper.Handle);
            if (hwndSource != null)
            {
                ClipboardHistoryService.Instance.Attach(helper.Handle, hwndSource);
            }
        }

        private void ApplySettings(AppSettings settings)
        {
            Dispatcher.Invoke(() =>
            {
                HotkeyBadgeText.Text = $"{settings.HotkeyModifier} + Space";
                _hotkeyManager.UpdateHotkey(settings.HotkeyModifier, settings.HotkeyKey);

                ApplyTheme(settings.Theme);
            });
        }

        private void ApplyTheme(string theme)
        {
            if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                // Crisp, elegant Apple-style Light Theme
                SetResourceColor("ThemeWindowBg", "#FFFFFF");
                SetResourceColor("ThemeWindowBorder", "#E2E8F0");
                SetResourceColor("ThemeTextPrimary", "#0F172A");
                SetResourceColor("ThemeTextSecondary", "#64748B");
                SetResourceColor("ThemePlaceholder", "#94A3B8");
                SetResourceColor("ThemeDivider", "#F1F5F9");
                SetResourceColor("ThemeFooterBg", "#F8FAFC");
                SetResourceColor("ThemeFooterBorder", "#E2E8F0");
                SetResourceColor("ThemeFooterText", "#64748B");
                SetResourceColor("ThemeItemHover", "#F1F5F9");
                SetResourceColor("ThemeItemSelected", "#E0E7FF");
                SetResourceColor("ThemeIconBg", "#F1F5F9");
                SetResourceColor("ThemeBadgeBg", "#F1F5F9");
                SetResourceColor("ThemeHotkeyBg", "#F1F5F9");
                SetResourceColor("ThemeHotkeyBorder", "#CBD5E1");
                SetResourceColor("ThemeHotkeyText", "#475569");

                MainShadowEffect.Opacity = 0.15;
                MainShadowEffect.BlurRadius = 38;
            }
            else if (string.Equals(theme, "Midnight", StringComparison.OrdinalIgnoreCase))
            {
                SetResourceColor("ThemeWindowBg", "#0F172A");
                SetResourceColor("ThemeWindowBorder", "#1E293B");
                SetResourceColor("ThemeTextPrimary", "#FFFFFF");
                SetResourceColor("ThemeTextSecondary", "#94A3B8");
                SetResourceColor("ThemePlaceholder", "#64748B");
                SetResourceColor("ThemeDivider", "#1E293B");
                SetResourceColor("ThemeFooterBg", "#0A0F1D");
                SetResourceColor("ThemeFooterBorder", "#1E293B");
                SetResourceColor("ThemeFooterText", "#64748B");
                SetResourceColor("ThemeItemHover", "#1E293B");
                SetResourceColor("ThemeItemSelected", "#2B3B55");
                SetResourceColor("ThemeIconBg", "#1E293B");
                SetResourceColor("ThemeBadgeBg", "#1E293B");
                SetResourceColor("ThemeHotkeyBg", "#1E293B");
                SetResourceColor("ThemeHotkeyBorder", "#334155");
                SetResourceColor("ThemeHotkeyText", "#94A3B8");

                MainShadowEffect.Opacity = 0.60;
                MainShadowEffect.BlurRadius = 32;
            }
            else if (string.Equals(theme, "Amoled", StringComparison.OrdinalIgnoreCase))
            {
                SetResourceColor("ThemeWindowBg", "#08080C");
                SetResourceColor("ThemeWindowBorder", "#1F1F28");
                SetResourceColor("ThemeTextPrimary", "#FFFFFF");
                SetResourceColor("ThemeTextSecondary", "#9CA3AF");
                SetResourceColor("ThemePlaceholder", "#6B7280");
                SetResourceColor("ThemeDivider", "#1F1F28");
                SetResourceColor("ThemeFooterBg", "#040406");
                SetResourceColor("ThemeFooterBorder", "#1F1F28");
                SetResourceColor("ThemeFooterText", "#6B7280");
                SetResourceColor("ThemeItemHover", "#171720");
                SetResourceColor("ThemeItemSelected", "#252533");
                SetResourceColor("ThemeIconBg", "#171720");
                SetResourceColor("ThemeBadgeBg", "#171720");
                SetResourceColor("ThemeHotkeyBg", "#171720");
                SetResourceColor("ThemeHotkeyBorder", "#2A2A38");
                SetResourceColor("ThemeHotkeyText", "#9CA3AF");

                MainShadowEffect.Opacity = 0.65;
                MainShadowEffect.BlurRadius = 32;
            }
            else
            {
                // Default Dark Graphite Theme
                SetResourceColor("ThemeWindowBg", "#181920");
                SetResourceColor("ThemeWindowBorder", "#2E303F");
                SetResourceColor("ThemeTextPrimary", "#FFFFFF");
                SetResourceColor("ThemeTextSecondary", "#9CA3AF");
                SetResourceColor("ThemePlaceholder", "#6B7280");
                SetResourceColor("ThemeDivider", "#262835");
                SetResourceColor("ThemeFooterBg", "#13141B");
                SetResourceColor("ThemeFooterBorder", "#232533");
                SetResourceColor("ThemeFooterText", "#6B7280");
                SetResourceColor("ThemeItemHover", "#2A2C3D");
                SetResourceColor("ThemeItemSelected", "#32364E");
                SetResourceColor("ThemeIconBg", "#222433");
                SetResourceColor("ThemeBadgeBg", "#242636");
                SetResourceColor("ThemeHotkeyBg", "#232530");
                SetResourceColor("ThemeHotkeyBorder", "#374151");
                SetResourceColor("ThemeHotkeyText", "#9CA3AF");

                MainShadowEffect.Opacity = 0.55;
                MainShadowEffect.BlurRadius = 32;
            }
        }

        private void SetResourceColor(string key, string hexColor)
        {
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
                Resources[key] = new SolidColorBrush(color);
            }
            catch { }
        }

        private void OnHotkeyPressed()
        {
            Dispatcher.Invoke(() =>
            {
                ToggleVisibility();
            });
        }

        private void PositionWindow()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Left = (screenWidth - Width) / 2;
            Top = screenHeight * 0.18; // Upper-center position like macOS Spotlight
        }

        public void ToggleVisibility()
        {
            if (Visibility == Visibility.Visible)
            {
                HideLauncher();
            }
            else
            {
                ShowLauncher();
            }
        }

        public void ShowLauncher()
        {
            PositionWindow();
            Visibility = Visibility.Visible;
            Show();
            Activate();
            Focus();

            var helper = new WindowInteropHelper(this);
            NativeMethods.SetForegroundWindow(helper.Handle);

            SearchBox.Focus();
            SearchBox.SelectAll();

            // Play Smooth macOS Scale-Up & Fade-In Animation
            PlayOpenAnimation();
        }

        private void PlayOpenAnimation()
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            var scaleXAnim = new DoubleAnimation(0.95, 1.0, TimeSpan.FromMilliseconds(150)) { EasingFunction = ease };
            var scaleYAnim = new DoubleAnimation(0.95, 1.0, TimeSpan.FromMilliseconds(150)) { EasingFunction = ease };
            var opacityAnim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(130)) { EasingFunction = ease };

            WindowScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            WindowScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
            MainContainerBorder.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        }

        public void HideLauncher()
        {
            Visibility = Visibility.Hidden;
            SearchBox.Text = string.Empty;
            ResultsList.ItemsSource = null;
            AnimateListHeight(0, () =>
            {
                ResultsList.Visibility = Visibility.Collapsed;
                DividerLine.Visibility = Visibility.Collapsed;
            });
        }

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            // Auto-hide when clicking anywhere outside
            if (Visibility == Visibility.Visible)
            {
                HideLauncher();
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = SearchBox.Text;
            int version = ++_searchVersion;
            PlaceholderText.Visibility = string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(text))
            {
                ResultsList.ItemsSource = null;
                AnimateListHeight(0, () =>
                {
                    ResultsList.Visibility = Visibility.Collapsed;
                    DividerLine.Visibility = Visibility.Collapsed;
                });
                return;
            }

            if (!_isInitialized) return;

            var results = await _orchestrator.SearchAsync(text);

            // A newer keystroke superseded this search; its results would overwrite the fresher list
            if (version != _searchVersion) return;

            // Add quick access to Win-Arma settings if user searches for "ayar", "settings"
            var trimmed = text.Trim().ToLowerInvariant();
            // "win-arma", "winarma", "win arma", "arma" or a 4+ char piece of it ("win" alone stays free for Windows apps)
            var compact = trimmed.Replace("-", "").Replace(" ", "");
            bool nameMatch = compact.Length >= 4 && "winarma".Contains(compact, StringComparison.Ordinal);
            bool settingsMatch = trimmed.Contains("ayar") || trimmed.Contains("setting") || trimmed.Contains("konfig");

            if (nameMatch || settingsMatch)
            {
                results.Insert(0, new SearchResultItem
                {
                    Title = settingsMatch ? "Win-Arma Ayarları" : "Win-Arma",
                    Subtitle = settingsMatch
                        ? "Kısayol, arama motoru, tema ve modül ayarlarını aç"
                        : "Çalışıyor — ayarları açmak için Enter",
                    Category = SearchResultCategory.SystemCommand,
                    IconSymbol = "⚙️",
                    Score = 990,
                    CustomAction = () =>
                    {
                        if (System.Windows.Application.Current is App app)
                        {
                            app.OpenSettings();
                        }
                    }
                });
            }

            if (results.Count > 0)
            {
                ResultsList.ItemsSource = results;
                ResultsList.SelectedIndex = 0;
                ResultsList.Visibility = Visibility.Visible;
                DividerLine.Visibility = Visibility.Visible;

                // Smooth dynamic expanding animation
                double targetHeight = Math.Min(results.Count * 54 + 16, 380);
                AnimateListHeight(targetHeight);
            }
            else
            {
                ResultsList.ItemsSource = null;
                AnimateListHeight(0, () =>
                {
                    ResultsList.Visibility = Visibility.Collapsed;
                    DividerLine.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void AnimateListHeight(double targetHeight, Action? onCompleted = null)
        {
            if (Math.Abs(_currentListHeight - targetHeight) < 1)
            {
                onCompleted?.Invoke();
                return;
            }

            _currentListHeight = targetHeight;
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            var anim = new DoubleAnimation
            {
                To = targetHeight,
                Duration = TimeSpan.FromMilliseconds(targetHeight > 0 ? 180 : 130),
                EasingFunction = ease
            };

            if (onCompleted != null)
            {
                anim.Completed += (s, e) => onCompleted();
            }

            ResultsList.BeginAnimation(FrameworkElement.HeightProperty, anim);
        }

        private void SearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (!string.IsNullOrEmpty(SearchBox.Text))
                {
                    SearchBox.Text = string.Empty;
                }
                else
                {
                    HideLauncher();
                }
                e.Handled = true;
                return;
            }

            if (e.Key == System.Windows.Input.Key.Down)
            {
                if (ResultsList.Items.Count > 0)
                {
                    int next = (ResultsList.SelectedIndex + 1) % ResultsList.Items.Count;
                    ResultsList.SelectedIndex = next;
                    ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                }
                e.Handled = true;
                return;
            }

            if (e.Key == System.Windows.Input.Key.Up)
            {
                if (ResultsList.Items.Count > 0)
                {
                    int prev = ResultsList.SelectedIndex - 1;
                    if (prev < 0) prev = ResultsList.Items.Count - 1;
                    ResultsList.SelectedIndex = prev;
                    ResultsList.ScrollIntoView(ResultsList.SelectedItem);
                }
                e.Handled = true;
                return;
            }

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                bool reveal = (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control;
                ExecuteSelectedItem(reveal);
                e.Handled = true;
                return;
            }

            if (e.Key == System.Windows.Input.Key.Tab)
            {
                if (ResultsList.SelectedItem is SearchResultItem item)
                {
                    SearchBox.Text = item.Title;
                    SearchBox.CaretIndex = SearchBox.Text.Length;
                }
                e.Handled = true;
                return;
            }

            if ((e.Key == System.Windows.Input.Key.Space && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) == System.Windows.Input.ModifierKeys.Shift) ||
                e.Key == System.Windows.Input.Key.F3)
            {
                if (TryOpenQuickLook())
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void ResultsList_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                bool reveal = (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control;
                ExecuteSelectedItem(reveal);
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                HideLauncher();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Space || e.Key == System.Windows.Input.Key.F3)
            {
                if (TryOpenQuickLook())
                {
                    e.Handled = true;
                }
            }
        }

        private bool TryOpenQuickLook()
        {
            if (!SettingsService.Instance.Current.EnableQuickLook) return false;

            if (ResultsList.SelectedItem is SearchResultItem item)
            {
                if (item.Category == SearchResultCategory.File || item.Category == SearchResultCategory.Folder)
                {
                    if (File.Exists(item.ActionTarget) || Directory.Exists(item.ActionTarget))
                    {
                        var ql = new QuickLookWindow(item.ActionTarget);
                        ql.Show();
                        return true;
                    }
                }
            }
            return false;
        }

        private void ResultsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ExecuteSelectedItem(false);
        }

        private void ExecuteSelectedItem(bool revealInExplorer)
        {
            if (ResultsList.SelectedItem is not SearchResultItem item)
            {
                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    var fallbackUrl = SettingsService.Instance.Current.GetSearchEngineUrl(SearchBox.Text.Trim());
                    LaunchTarget(fallbackUrl);
                    HideLauncher();
                }
                return;
            }

            try
            {
                if (revealInExplorer && (item.Category == SearchResultCategory.File || item.Category == SearchResultCategory.Folder))
                {
                    if (File.Exists(item.ActionTarget) || Directory.Exists(item.ActionTarget))
                    {
                        Process.Start("explorer.exe", $"/select,\"{item.ActionTarget}\"");
                    }
                }
                else if (item.CustomAction != null)
                {
                    item.CustomAction.Invoke();
                }
                else if (!string.IsNullOrWhiteSpace(item.ActionTarget))
                {
                    LaunchTarget(item.ActionTarget);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Başlatılamadı: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                HideLauncher();
            }
        }

        private void LaunchTarget(string target)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            ClipboardHistoryService.Instance.Dispose();
            _hotkeyManager.Dispose();
            base.OnClosed(e);
        }
    }
}
