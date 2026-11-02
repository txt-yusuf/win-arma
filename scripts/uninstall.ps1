# Win-Arma - Kaldırma betiği / Uninstall script
# Programı, Başlat menüsü kısayolunu ve açılışta başlatma kaydını siler.
# Ayarlar (%AppData%\WinArma) silinmez; tamamen silmek için -RemoveSettings kullanın.

param(
    [switch]$RemoveSettings
)

$ErrorActionPreference = "Stop"

$target    = Join-Path $env:LOCALAPPDATA "Programs\WinArma"
$startMenu = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\Win-Arma.lnk"

Get-Process WinArma -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

Remove-ItemProperty "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "WinArma" -ErrorAction SilentlyContinue
if (Test-Path $startMenu) { Remove-Item $startMenu -Force }
if (Test-Path $target)    { Remove-Item $target -Recurse -Force }

if ($RemoveSettings) {
    $data = Join-Path $env:APPDATA "WinArma"
    if (Test-Path $data) { Remove-Item $data -Recurse -Force }
}

Write-Host "Win-Arma kaldırıldı / uninstalled."
