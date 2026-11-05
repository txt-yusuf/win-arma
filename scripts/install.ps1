# Win-Arma - Kurulum betiği / Install script
# Kullanım / Usage (zip'i çıkardığınız klasörde / from the extracted folder):
#   powershell -ExecutionPolicy Bypass -File .\install.ps1 [-NoStartup]   (release zip: next to WinArma.exe)
#   powershell -ExecutionPolicy Bypass -File .\scripts\install.ps1        (repo checkout, after dotnet publish)
#
# Dosyaları %LOCALAPPDATA%\Programs\WinArma altına kopyalar, Başlat menüsü kısayolu oluşturur
# ve (isteğe bağlı) Windows açılışında başlatır. Kaldırmak için uninstall.ps1.

param(
    [string]$Source = $(if (Test-Path (Join-Path $PSScriptRoot "WinArma.exe")) { $PSScriptRoot } else { Join-Path (Split-Path -Parent $PSScriptRoot) "publish" }),
    [switch]$NoStartup
)

$ErrorActionPreference = "Stop"

$exe = Join-Path $Source "WinArma.exe"
if (-not (Test-Path $exe)) {
    throw "WinArma.exe bulunamadı: $Source (zip'i çıkardığınız klasörden çalıştırın)"
}

$target   = Join-Path $env:LOCALAPPDATA "Programs\WinArma"
$startMenu = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\Win-Arma.lnk"

# Çalışan kopyayı kapat / stop a running copy so files can be replaced
Get-Process WinArma -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

New-Item -ItemType Directory -Force $target | Out-Null
Copy-Item (Join-Path $Source "*") $target -Recurse -Force -Exclude "*.zip"

# Başlat menüsü kısayolu (arama çubuğunda ve Windows aramasında görünür)
$shell = New-Object -ComObject WScript.Shell
$lnk = $shell.CreateShortcut($startMenu)
$lnk.TargetPath = Join-Path $target "WinArma.exe"
$lnk.WorkingDirectory = $target
$lnk.Description = "Win-Arma - Spotlight tarzı başlatıcı"
$lnk.Save()

if (-not $NoStartup) {
    Set-ItemProperty "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "WinArma" -Value "`"$target\WinArma.exe`""
}

Start-Process (Join-Path $target "WinArma.exe") -WorkingDirectory $target

Write-Host "Win-Arma kuruldu / installed: $target"
Write-Host "Alt + Space ile açın / Press Alt + Space to open."
