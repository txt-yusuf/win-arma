# Win-Arma Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

#### ✨ Added
- **Scoped search prefixes** — `>` system commands only (`>` alone lists them all), `?` web only, `/` files only
- **Quick commands** — Settings → Hızlı Komutlar: a keyword opens a URL, file, folder or program (`yt`, `proje`); environment variables such as `%USERPROFILE%` work
- **Window switcher** — `pencere` lists open windows; any 3+ characters of a title or app name jump to it
- **Calculator** — new parser: `sin cos tan asin acos atan log log2 ln exp sqrt abs round floor ceil`, `pi`, `e`, `100 + 10%` = 110, modulo, `-2^2`; dates like `2024-05-10` are no longer subtracted
- **Unit converter** — `5 km mil`, `100 c f`, `2 gb mb`, `3 saat dk`, `60 mph km/h` (length, mass, volume, data, time, speed, temperature; offline)
- **Safer system commands** — kapat, yeniden başlat and çöpü boşalt ask for confirmation; new `kapatmayı iptal` command
- **Tray start** — the launcher starts hidden, with a one-time tray hint; update notice (opt-out in Settings, tray → Güncellemeleri denetle) that opens the release page
- **install.ps1 / uninstall.ps1** shipped in the release zip (installs to `%LOCALAPPDATA%\Programs\WinArma`, Start Menu shortcut, optional startup)
- **Sharper UI** — PerMonitorV2 DPI manifest, layout rounding and `Display` text formatting on all windows
- **Win-Arma entry in the search bar** — typing `win-arma`, `winarma` or `arma` shows Win-Arma itself; Enter opens its settings
- **Snippet editor** — add, edit and delete text snippets from Settings → Metin Kısayolları (multi-line content, inline validation)

#### 🔒 Security
- Clipboard history is stored encrypted (Windows DPAPI, current user) in `clipboard_history.bin`; the old plain `clipboard_history.json` is imported once and removed
- Copies marked sensitive by password managers (`ExcludeClipboardContentFromMonitorProcessing`, `CanIncludeInClipboardHistory=0`) are not recorded; snippets copied by Win-Arma are marked the same way
- The release workflow stamps the app version from the tag (`-p:Version`), so the update check compares against the right number

#### 🐛 Fixed
- Saving settings reset `WindowOpacity` and `HotkeyKey` to their defaults
- Snippets loaded from `settings.json` were case-sensitive
- Calculator read decimal results as integers in comma-decimal cultures (`10/4` gave 25), dropped small values (`0.001` gave 0) and copied digit-grouped answers
- System commands matched a single typed character (`k` listed Kapat / Kilitle); now exact name or a prefix of 3+ characters
- A hotkey the system already owns (e.g. Win+Space) left the launcher with no hotkey; it now falls back to Ctrl+Space
- A slow earlier search could overwrite the results of a newer one
- Results waited for the Windows Search file index (2-4 s for some queries), so the list lagged behind typing and a quick Enter ran an entry of the previous query. Search is now two-phase: everything from memory (commands, snippets, calculator, apps, windows, web) appears within milliseconds, file hits are added when the index answers
- Calculator functions work without parentheses too (`sin 30`, `sqrt 16`, `log 1000`)
- README: `y` prefix is YouTube, and the snippet example no longer lists unsupported `__auto__` markers

---

## [1.0.0] - 2026-10-29

### 🎉 Initial Release

#### ✨ Added
- **Spotlight-style launcher** — `Alt + Space` global hotkey, animated open/close with expanding result list
- **App search** — scans Windows Start Menu shortcuts for installed applications
- **File search** — Windows Search index integration with common folders fallback
- **Web search** — Google, Bing, DuckDuckGo, Yandex, Brave with prefix shortcuts (`g `, `y `, `gh `, `w `, `b `)
- **Calculator** — supports expressions, `sqrt()`, `^` (power), `%` (percentage)
- **System commands** — kilitle (lock), kapat (shutdown), uyku (sleep), yeniden başlat (restart)
- **Clipboard history** — tracks last 50 copied items, triggered with `pano` or `cb`, persisted to `%AppData%\WinArma\clipboard_history.json`
- **Quick Look** — `Shift+Space` / `F3` to preview images, text/code files, and show file info cards
- **Text snippets** — `!mail`, `!iban`, `!tel`, `!adres`, `!tarih`, `!saat`, `!shrug`, `!guid`
- **Settings window** — full settings UI accessible via tray icon right-click
  - All features individually toggleable (on/off switches)
  - Theme selection, hotkey customization, search engine choice
  - Max results slider (5–20)
  - Windows startup toggle (registry-based)
- **4 Themes** — Dark, Midnight, Amoled, Light (Apple-style white)
- **Custom logo & icon** — generated programmatically via WPF `DrawingVisual`, multi-size `.ico` (16–256px) + `logo.png`
- **System tray** — tray icon with right-click context menu (Open, Settings, About, Exit)
- **Settings persistence** — JSON settings at `%AppData%\WinArma\settings.json`
- **Windows startup** — optional auto-start via registry `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`

---

## [Unreleased]

### 🔮 Planned
- Custom snippet editor in Settings UI
- Plugin system for third-party result providers
- More themes (Nord, Solarized, Dracula)
- Fuzzy search ranking improvements
- Multi-monitor positioning support
