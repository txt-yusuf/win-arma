using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinArma.Models;
using WinArma.Services;

namespace WinArma.Views
{
    public class SnippetRow
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public partial class SettingsWindow : Window
    {
        private readonly ObservableCollection<SnippetRow> _snippetRows = new();
        private readonly ObservableCollection<SnippetRow> _commandRows = new();

        public SettingsWindow()
        {
            InitializeComponent();
            SnippetList.ItemsSource = _snippetRows;
            CommandList.ItemsSource = _commandRows;
            Loaded += SettingsWindow_Loaded;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
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

            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            var s = SettingsService.Instance.Current;

            // Hotkey
            SelectComboByTag(HotkeyCombo, s.HotkeyModifier);

            // Startup
            StartupCheckBox.IsChecked = s.RunAtStartup;

            // Search Engine
            SelectComboByContent(SearchEngineCombo, s.DefaultSearchEngine);

            // Modules
            AppSearchCheckBox.IsChecked = s.EnableAppSearch;
            FileSearchCheckBox.IsChecked = s.EnableFileSearch;
            WebSearchCheckBox.IsChecked = s.EnableWebSearch;
            CalcCheckBox.IsChecked = s.EnableCalculator;
            SysCmdCheckBox.IsChecked = s.EnableSystemCommands;

            // Advanced Features
            ClipboardCheckBox.IsChecked = s.EnableClipboardHistory;
            QuickLookCheckBox.IsChecked = s.EnableQuickLook;
            SnippetsCheckBox.IsChecked = s.EnableSnippets;
            WindowSearchCheckBox.IsChecked = s.EnableWindowSearch;
            UpdateCheckBox.IsChecked = s.CheckForUpdates;
            LoadRows(_snippetRows, s.Snippets, SnippetErrorText);
            LoadRows(_commandRows, s.Commands, CommandErrorText);

            // Max Results
            SelectComboByTag(MaxResultsCombo, s.MaxResultsCount.ToString());

            // Theme
            SelectComboByTag(ThemeCombo, s.Theme);
        }

        private static void LoadRows(ObservableCollection<SnippetRow> rows, IDictionary<string, string>? source, TextBlock errorText)
        {
            rows.Clear();
            errorText.Visibility = Visibility.Collapsed;
            if (source == null) return;

            foreach (var kv in source)
            {
                rows.Add(new SnippetRow { Key = kv.Key, Value = kv.Value });
            }
        }

        private void AddSnippet_Click(object sender, RoutedEventArgs e) =>
            AddRow(_snippetRows, SnippetList, "!", SnippetErrorText);

        private void AddCommand_Click(object sender, RoutedEventArgs e) =>
            AddRow(_commandRows, CommandList, "", CommandErrorText);

        private void AddRow(ObservableCollection<SnippetRow> rows, ItemsControl list, string initialKey, TextBlock errorText)
        {
            var row = new SnippetRow { Key = initialKey };
            rows.Add(row);
            errorText.Visibility = Visibility.Collapsed;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (list.ItemContainerGenerator.ContainerFromItem(row) is FrameworkElement container)
                {
                    container.BringIntoView();
                    var keyBox = FindFirstTextBox(container);
                    if (keyBox != null)
                    {
                        keyBox.Focus();
                        keyBox.CaretIndex = keyBox.Text.Length;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SnippetRow row })
            {
                if (_snippetRows.Remove(row)) SnippetErrorText.Visibility = Visibility.Collapsed;
                if (_commandRows.Remove(row)) CommandErrorText.Visibility = Visibility.Collapsed;
            }
        }

        private static System.Windows.Controls.TextBox? FindFirstTextBox(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is System.Windows.Controls.TextBox tb) return tb;
                var nested = FindFirstTextBox(child);
                if (nested != null) return nested;
            }
            return null;
        }

        private void ShowSnippetError(string message) => ShowError(SnippetErrorText, message);

        private static void ShowError(TextBlock target, string message)
        {
            target.Text = message;
            target.Visibility = Visibility.Visible;
            target.BringIntoView();
        }

        // Rows -> dictionary for quick commands. Returns null (and shows an inline error) when a row is invalid.
        private Dictionary<string, string>? BuildCommands()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in _commandRows)
            {
                var key = (row.Key ?? string.Empty).Trim();
                var target = (row.Value ?? string.Empty).Trim();

                // Untouched new row: drop silently
                if (key.Length == 0 && target.Length == 0) continue;

                if (key.Length == 0)
                {
                    ShowError(CommandErrorText, "Bir komutun adı boş olamaz (örn. yt).");
                    return null;
                }

                if (key.Any(char.IsWhiteSpace))
                {
                    ShowError(CommandErrorText, $"\"{key}\" boşluk içeremez.");
                    return null;
                }

                if (key[0] is '!' or '>' or '?' or '/')
                {
                    ShowError(CommandErrorText, $"\"{key}\" ! > ? / ile başlayamaz (bunlar özel önekler).");
                    return null;
                }

                if (target.Length == 0 || target.Contains('\n') || target.Contains('\r'))
                {
                    ShowError(CommandErrorText, $"\"{key}\" için tek satırlık bir adres, klasör ya da program yazın.");
                    return null;
                }

                if (!result.TryAdd(key, target))
                {
                    ShowError(CommandErrorText, $"\"{key}\" birden fazla kez tanımlı.");
                    return null;
                }
            }

            return result;
        }

        // Rows -> dictionary. Returns null (and shows an inline error) when a row is invalid.
        private Dictionary<string, string>? BuildSnippets()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in _snippetRows)
            {
                var key = (row.Key ?? string.Empty).Trim();
                var value = row.Value ?? string.Empty;

                // Untouched new row ("!" and nothing else): drop silently
                if ((key.Length == 0 || key == "!") && value.Length == 0) continue;

                if (key.Length == 0 || key == "!")
                {
                    ShowSnippetError("Bir kısayolun adı boş olamaz (örn. !mail).");
                    return null;
                }

                if (!key.StartsWith("!")) key = "!" + key;

                if (key.Any(char.IsWhiteSpace))
                {
                    ShowSnippetError($"\"{key}\" boşluk içeremez.");
                    return null;
                }

                if (value.Length == 0)
                {
                    ShowSnippetError($"\"{key}\" için içerik boş.");
                    return null;
                }

                if (!result.TryAdd(key, value))
                {
                    ShowSnippetError($"\"{key}\" birden fazla kez tanımlı.");
                    return null;
                }
            }

            return result;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResetDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            var def = new AppSettings();
            SelectComboByTag(HotkeyCombo, def.HotkeyModifier);
            StartupCheckBox.IsChecked = false;
            SelectComboByContent(SearchEngineCombo, def.DefaultSearchEngine);
            AppSearchCheckBox.IsChecked = def.EnableAppSearch;
            FileSearchCheckBox.IsChecked = def.EnableFileSearch;
            WebSearchCheckBox.IsChecked = def.EnableWebSearch;
            CalcCheckBox.IsChecked = def.EnableCalculator;
            SysCmdCheckBox.IsChecked = def.EnableSystemCommands;
            ClipboardCheckBox.IsChecked = def.EnableClipboardHistory;
            QuickLookCheckBox.IsChecked = def.EnableQuickLook;
            SnippetsCheckBox.IsChecked = def.EnableSnippets;
            WindowSearchCheckBox.IsChecked = def.EnableWindowSearch;
            UpdateCheckBox.IsChecked = def.CheckForUpdates;
            LoadRows(_snippetRows, def.Snippets, SnippetErrorText);
            LoadRows(_commandRows, def.Commands, CommandErrorText);
            SelectComboByTag(MaxResultsCombo, def.MaxResultsCount.ToString());
            SelectComboByTag(ThemeCombo, def.Theme);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var snippets = BuildSnippets();
            var commands = BuildCommands();
            if (snippets == null || commands == null) return;

            // Start from the stored settings so fields this window does not edit (opacity, hotkey key,
            // first-run flag, ...) survive the save
            var s = SettingsService.Instance.Current.Clone();
            s.HotkeyModifier = GetSelectedTag(HotkeyCombo, "Alt");
            s.RunAtStartup = StartupCheckBox.IsChecked == true;
            s.DefaultSearchEngine = GetSelectedContent(SearchEngineCombo, "Google");
            s.EnableAppSearch = AppSearchCheckBox.IsChecked == true;
            s.EnableFileSearch = FileSearchCheckBox.IsChecked == true;
            s.EnableWebSearch = WebSearchCheckBox.IsChecked == true;
            s.EnableCalculator = CalcCheckBox.IsChecked == true;
            s.EnableSystemCommands = SysCmdCheckBox.IsChecked == true;
            s.EnableClipboardHistory = ClipboardCheckBox.IsChecked == true;
            s.EnableQuickLook = QuickLookCheckBox.IsChecked == true;
            s.EnableSnippets = SnippetsCheckBox.IsChecked == true;
            s.EnableWindowSearch = WindowSearchCheckBox.IsChecked == true;
            s.CheckForUpdates = UpdateCheckBox.IsChecked == true;
            s.Snippets = snippets;
            s.Commands = commands;
            s.MaxResultsCount = int.TryParse(GetSelectedTag(MaxResultsCombo, "8"), out int max) ? max : 8;
            s.Theme = GetSelectedTag(ThemeCombo, "Dark");

            SettingsService.Instance.SaveSettings(s);
            Close();
        }

        private void SelectComboByTag(System.Windows.Controls.ComboBox combo, string tag)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private void SelectComboByContent(System.Windows.Controls.ComboBox combo, string content)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (string.Equals(item.Content?.ToString(), content, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private string GetSelectedTag(System.Windows.Controls.ComboBox combo, string fallback)
        {
            if (combo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                return item.Tag.ToString() ?? fallback;
            }
            return fallback;
        }

        private string GetSelectedContent(System.Windows.Controls.ComboBox combo, string fallback)
        {
            if (combo.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                return item.Content.ToString() ?? fallback;
            }
            return fallback;
        }
    }
}
