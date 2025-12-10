# Script to fix CSV encoding for Excel - Run this after closing Excel
param(
    [string]$FilePath = "SystemWorkflowTests.csv"
)

Write-Host "Fixing CSV encoding for Excel compatibility..."
Write-Host "Please make sure Excel is closed before running this script!"
Write-Host ""

if (-not (Test-Path $FilePath)) {
    Write-Host "Error: File not found: $FilePath" -ForegroundColor Red
    exit 1
}

# Read content with UTF-8
$content = Get-Content $FilePath -Raw -Encoding UTF8

# Create backup
$backupFile = $FilePath + ".backup"
Copy-Item $FilePath $backupFile -Force
Write-Host "Backup created: $backupFile"

# Write with UTF-8 BOM using .NET
$utf8WithBom = New-Object System.Text.UTF8Encoding $true
$tempFile = $FilePath + ".tmp"
[System.IO.File]::WriteAllText($tempFile, $content, $utf8WithBom)

# Wait a moment
Start-Sleep -Milliseconds 500

# Replace original file
Remove-Item $FilePath -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 200
Rename-Item $tempFile $FilePath -Force

Write-Host ""
Write-Host "File encoding fixed!" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANT: To open in Excel correctly:"
Write-Host "1. Open Excel"
Write-Host "2. Data > Get Data > From File > From Text/CSV"
Write-Host "3. Select the file"
Write-Host "4. In preview window, set 'File Origin' to '65001: Unicode (UTF-8)'"
Write-Host "5. Click 'Load'"
Write-Host ""







