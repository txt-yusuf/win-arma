<div align="center">

<img src="logo.png" alt="Win-Arma Logo" width="120" />

# ⚡ Win-Arma

**Windows için macOS Spotlight ilhamlı evrensel arama & başlatıcı**  
*A macOS Spotlight-inspired universal launcher for Windows*

[![Build](https://github.com/YOUR_USERNAME/win-arma/actions/workflows/build.yml/badge.svg)](https://github.com/YOUR_USERNAME/win-arma/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/9)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078d7.svg)](https://www.microsoft.com/windows)
[![Release](https://img.shields.io/github/v/release/YOUR_USERNAME/win-arma?color=green)](https://github.com/YOUR_USERNAME/win-arma/releases)

</div>

---

## 🇹🇷 Türkçe

**Win-Arma**, macOS'taki Spotlight gibi çalışan, `Alt + Space` tuşuna basınca ekranın ortasında açılan ve uygulamaları, dosyaları, web aramalarını, hesap makinesini ve çok daha fazlasını tek pencereden sunan bir Windows başlatıcısıdır.

## 🇬🇧 English

**Win-Arma** is a lightweight Spotlight-style launcher for Windows. Press `Alt + Space` to instantly search apps, files, the web, run calculations, paste clipboard history, preview files, and expand custom text snippets — all from one animated window.

---

## ✨ Özellikler / Features

| # | Özellik | Feature |
|---|---------|---------|
| 🔍 | Uygulama arama | App search (Start Menu shortcuts) |
| 📁 | Dosya arama | File search (Windows Search index) |
| 🌐 | Web arama | Web search — Google, Bing, DDG, Yandex, Brave |
| 🧮 | Hesap makinesi | Calculator — `+ - * / ^ %`, `sin cos tan log ln sqrt abs round`, `pi`, `100 + 10%` |
| 📐 | Birim çevirici | Unit converter — `5 km mil`, `100 c f`, `2 gb mb`, `3 saat dk` (offline) |
| 💻 | Sistem komutları | System commands — kilitle, kapat, uyku, yeniden başlat (kapat / çöp için onay sorar) |
| 🪟 | Açık pencereler | Window switcher — `pencere` or any part of a window title |
| 🚀 | Hızlı komutlar | Custom keywords — `yt` opens a site, `proje` opens a folder (Settings) |
| 📋 | Pano geçmişi | Clipboard history — last 50 items, encrypted (DPAPI), password-manager copies skipped |
| 🔔 | Güncelleme | Update notice in the tray (can be turned off) |
| 👀 | Hızlı önizleme | Quick Look — Shift+Space / F3 for images & text files |
| ⚡ | Metin kısayolları | Text snippets — `!mail`, `!iban`, `!tel`, `!guid`, `!tarih`… |
| 🎨 | Temalar | 4 themes: Dark · Midnight · Amoled · Light |
| ⚙️ | Ayarlar | Full settings window via tray icon right-click |
| 🔑 | Özel kısayol | Configurable hotkey (default `Alt + Space`) |
| 🚀 | Başlangıç | Optional Windows startup via registry |

---

## 🎨 Temalar / Themes

<table>
<tr>
<td align="center"><b>🌑 Dark</b></td>
<td align="center"><b>🌃 Midnight</b></td>
<td align="center"><b>⚫ Amoled</b></td>
<td align="center"><b>☀️ Light</b></td>
</tr>
<tr>
<td align="center">Koyu gri zemin</td>
<td align="center">Derin lacivert</td>
<td align="center">Saf siyah (OLED)</td>
<td align="center">Apple beyazı</td>
</tr>
</table>

---

## ⌨️ Klavye Kısayolları / Keyboard Shortcuts

| Kısayol | Eylem |
|---------|-------|
| `Alt + Space` | Pencereyi aç / kapat |
| `↑` `↓` | Sonuçlar arasında gezin |
| `Enter` | Seçili öğeyi çalıştır / uygula |
| `Shift + Space` veya `F3` | Seçili dosyayı Quick Look ile önizle |
| `Escape` | Pencereyi kapat |
| `pano` veya `cb` | Pano geçmişini listele |
| `> kapat` | Yalnızca sistem komutları (`>` tek başına hepsini listeler) |
| `? metin` | Yalnızca web araması |
| `/ metin` | Yalnızca dosya araması |
| `pencere` | Açık pencereleri listele |
| `g metin` | Google'da ara |
| `y metin` | YouTube'da ara |
| `gh metin` | GitHub'da ara |
| `w metin` | Wikipedia'da ara |
| `b metin` | Bing'de ara |
| `!mail` | E-posta snippeti yapıştır |
| `!guid` | Yeni GUID üret |
| `!tarih` | Bugünün tarihini yapıştır |
| `!saat` | Şu anki saati yapıştır |

---

## 📥 Kurulum / Installation

### Hazır Paket (Önerilen) / Pre-built Release (Recommended)

1. [**Releases**](https://github.com/YOUR_USERNAME/win-arma/releases) sayfasından son sürümü indirin  
   *(Download the latest `WinArma-vX.Y.Z.zip` from Releases)*
2. Zip'i bir klasöre çıkartın / Extract to any folder
3. `WinArma.exe` dosyasını çalıştırın / Run `WinArma.exe`
4. Sistem tepsisinde ikon görünür / App appears in system tray
5. `Alt + Space` ile başlatın / Press `Alt + Space` to launch

Program tepside gizli başlar. İsterseniz zip içindeki `install.ps1` ile `%LOCALAPPDATA%\Programs\WinArma` altına kurup Başlat menüsü kısayolu ve açılışta başlatma ekleyebilirsiniz (kaldırmak için `uninstall.ps1`):

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

> **Gereksinim / Requirement:** [.NET 9 Runtime (Windows)](https://dotnet.microsoft.com/download/dotnet/9) yüklü olmalıdır.

---

### Kaynaktan Derleme / Build from Source

```bash
# 1. Repoyu klonla / Clone the repo
git clone https://github.com/YOUR_USERNAME/win-arma.git
cd win-arma

# 2. Derle / Build
dotnet build

# 3. Çalıştır / Run
dotnet run

# 4. Yayımla (tek klasör) / Publish (self-contained folder)
dotnet publish -c Release -o ./publish
# Çıktı: ./publish/WinArma.exe
```

**Veya** hazır script ile / **Or** use the release script:

```powershell
.\scripts\publish.ps1
# Çıktı: WinArma-release.zip
```

---

## ⚙️ Ayarlar / Settings

Sistem tepsisindeki Win-Arma ikonuna **sağ tıklayın** → **⚙️ Ayarlar**

Right-click the tray icon → **⚙️ Settings**

| Ayar | Açıklama |
|------|----------|
| Kısayol tuşu | `Alt+Space`, `Ctrl+Space`, `Win+Space` |
| Tema | Dark / Midnight / Amoled / Light |
| Arama motoru | Google, Bing, DuckDuckGo, Yandex, Brave |
| Maksimum sonuç | 5 – 20 arası |
| Başlangıçta başlat | Windows'la otomatik başlat |
| Pano geçmişi | Etkinleştir / devre dışı bırak |
| Quick Look | Etkinleştir / devre dışı bırak |
| Snippet'ler | Etkinleştir / devre dışı bırak |
| Dosya arama | Etkinleştir / devre dışı bırak |
| Web arama | Etkinleştir / devre dışı bırak |

Ayarlar `%AppData%\WinArma\settings.json` dosyasına kaydedilir.

---

## ⚡ Snippet Kısayolları / Snippet Shortcuts

Kısayolları **Ayarlar → Metin Kısayolları** bölümünden ekleyebilir, düzenleyebilir ve silebilirsiniz
(*Add, edit and delete snippets under* **Settings → Metin Kısayolları**). `!tarih`, `!saat`, `!bugun` ve `!guid`
yerleşiktir ve değiştirilemez.

İsterseniz `settings.json` içindeki `Snippets` bölümünü elle de düzenleyebilirsiniz:

```json
{
  "Snippets": {
    "!mail": "ornek@email.com",
    "!iban": "TR00 0000 0000 0000 0000 00",
    "!tel": "+90 555 000 00 00",
    "!adres": "Örnek Mah. No:1 İstanbul"
  }
}
```

---

## 🏗️ Proje Yapısı / Project Structure

```
win-arma/
├── 📄 WinArma.csproj          # .NET 9 WPF project
├── 📄 App.xaml / App.xaml.cs  # Application entry, tray icon
├── 📄 MainWindow.xaml/.cs     # Main launcher window
├── 📁 Views/
│   ├── SettingsWindow.xaml/.cs   # Settings UI
│   └── QuickLookWindow.xaml/.cs  # File preview window
├── 📁 Services/
│   ├── AppSearchService.cs       # Application search
│   ├── FileSearchService.cs      # File search (Windows Search)
│   ├── WebSearchService.cs       # Web search with prefixes
│   ├── CalculatorService.cs      # Expression evaluator
│   ├── ClipboardHistoryService.cs # Clipboard tracking
│   ├── SnippetService.cs         # Text snippets
│   ├── SystemCommandService.cs   # System commands
│   └── SettingsService.cs        # Settings persistence
├── 📁 Utils/
│   ├── HotkeyManager.cs          # Global hotkey registration
│   ├── IconGenerator.cs          # Programmatic icon/logo creation
│   └── NativeMethods.cs          # P/Invoke declarations
├── 📁 scripts/
│   └── publish.ps1               # Release build script
├── 📁 .github/workflows/
│   └── build.yml                 # GitHub Actions CI/CD
├── 🖼️ logo.png                  # App logo (256×256)
├── 🖼️ app.ico                   # App icon (multi-size)
├── 📄 README.md
├── 📄 LICENSE                    # MIT
├── 📄 CONTRIBUTING.md
└── 📄 CHANGELOG.md
```

---

## 🤝 Katkıda Bulunma / Contributing

Katkılar memnuniyetle karşılanır! Lütfen [CONTRIBUTING.md](CONTRIBUTING.md) dosyasını okuyun.  
Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md).

---

## 📝 Lisans / License

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.  
This project is licensed under the [MIT License](LICENSE).

---

<div align="center">

**Win-Arma** — *Ara. Bul. Başlat.* / *Search. Find. Launch.*

Made with ❤️ for Windows users

</div>
