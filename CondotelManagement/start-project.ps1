# Script để khởi động dự án Condotel Management
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Khởi động dự án Condotel Management" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Di chuyển vào thư mục dự án
$projectPath = "D:\CondotelManagement\CondotelManagement\chisfis-booking-main"
Set-Location $projectPath

Write-Host "Thư mục hiện tại: $(Get-Location)" -ForegroundColor Green
Write-Host ""

# Kiểm tra package.json
if (-not (Test-Path "package.json")) {
    Write-Host "LỖI: Không tìm thấy package.json!" -ForegroundColor Red
    Write-Host "Vui lòng kiểm tra đường dẫn dự án." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Đã tìm thấy package.json" -ForegroundColor Green
Write-Host ""

# Kiểm tra node_modules
if (-not (Test-Path "node_modules")) {
    Write-Host "Chưa cài đặt dependencies. Đang cài đặt..." -ForegroundColor Yellow
    Write-Host ""
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "LỖI: Cài đặt dependencies thất bại!" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
    Write-Host "✓ Đã cài đặt dependencies thành công" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "✓ Dependencies đã được cài đặt" -ForegroundColor Green
    Write-Host ""
}

# Khởi động dự án
Write-Host "Đang khởi động server development..." -ForegroundColor Yellow
Write-Host "Server sẽ chạy tại: http://localhost:3000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Nhấn Ctrl+C để dừng server" -ForegroundColor Yellow
Write-Host ""

npm start


