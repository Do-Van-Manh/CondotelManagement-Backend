# Script để lấy lại các file refund từ nhánh truongnd
# Chạy script này khi đang ở nhánh master

Write-Host "Đang lấy lại các file refund từ nhánh truongnd..." -ForegroundColor Yellow

# Danh sách các file refund cần lấy
$refundFiles = @(
    "Controllers/Admin/AdminRefundController.cs",
    "DTOs/Admin/RefundRequestDTO.cs",
    "DTOs/Booking/RefundBookingRequestDTO.cs",
    "DTOs/Payment/PayOSRefundRequest.cs",
    "Models/RefundRequest.cs"
)

# Lấy từng file từ nhánh truongnd
foreach ($file in $refundFiles) {
    Write-Host "Đang lấy: $file" -ForegroundColor Cyan
    git checkout truongnd -- $file
}

Write-Host "`nHoàn tất! Các file đã được lấy lại từ nhánh truongnd." -ForegroundColor Green
Write-Host "Kiểm tra trạng thái: git status" -ForegroundColor Yellow



