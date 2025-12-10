# Hướng Dẫn Sử Dụng File Excel Test Cases

## 📋 Tổng Quan

File `System_Test_Cases_Template.csv` chứa tất cả 15 system test cases được format theo template Google Sheets, có thể mở trực tiếp bằng Excel hoặc Google Sheets.

## 📁 File Đã Tạo

- **System_Test_Cases_Template.csv** - File CSV chứa test cases (có thể mở bằng Excel)

## 🚀 Cách Sử Dụng

### 1. Mở File trong Excel

1. Double-click vào file `System_Test_Cases_Template.csv`
2. Excel sẽ tự động mở file
3. Nếu Excel hỏi về encoding, chọn **UTF-8**

### 2. Mở File trong Google Sheets

1. Truy cập [Google Sheets](https://sheets.google.com)
2. Click **File** → **Import**
3. Chọn **Upload** và upload file `System_Test_Cases_Template.csv`
4. Chọn **Import location**: "Replace spreadsheet"
5. Click **Import data**

### 3. Format File trong Excel

Sau khi mở file, bạn có thể format như sau:

#### A. Format Header (Row 1-4)
- **Row 1**: Merge cells A1:E1, set background color xanh đậm, text màu trắng
- **Row 2-4**: Set background color xám nhạt cho các cell metadata

#### B. Format Testing Round Summary (Row 6-9)
- **Row 6**: Header row - set background color xanh đậm, text màu trắng, bold
- **Row 7-9**: Set background color xanh nhạt cho các round rows

#### C. Format Test Case Details
- **Header Row (Row 11)**: Set background color xanh đậm, text màu trắng, bold
- **Scenario Rows**: Set background color xanh nhạt (light blue)
- **Test Case Rows**: Set background color trắng

#### D. Column Widths
- **Column A (Test Case ID)**: Width = 15
- **Column B (Description)**: Width = 50
- **Column C (Procedure)**: Width = 60
- **Column D (Expected Results)**: Width = 60
- **Column E (Pre-conditions)**: Width = 40

## 📊 Cấu Trúc File

### 1. Header/Metadata (Row 1-4)
```
Workflow: Condotel Management System - Main Workflows
Test requirement: Test các luồng chính của hệ thống...
Number of TCs: 15
```

### 2. Testing Round Summary (Row 6-9)
```
        | Passed | Failed | Pending | N/A
Round 1 |   0    |   0    |   15    |  0
Round 2 |   0    |   0    |   15    |  0
Round 3 |   0    |   0    |   15    |  0
```

### 3. Test Case Details (Row 11+)

Các test cases được nhóm theo Scenario:

- **Scenario A: Authentication & Tenant Booking**
  - SYS-001: Complete Tenant Booking Flow
  - SYS-002: Complete Host Registration Flow
  - SYS-003: Complete Booking with Payment Flow
  - SYS-015: Complete Multi-Step Booking with Voucher Flow

- **Scenario B: Review & Communication**
  - SYS-004: Complete Review Flow
  - SYS-005: Complete Package Purchase Flow

- **Scenario C: Wallet & Payout**
  - SYS-006: Complete Wallet and Payout Flow
  - SYS-007: Complete Admin Management Flow

- **Scenario D: Security & Authorization**
  - SYS-008: Authorization and Security Flow
  - SYS-009: Complete Search and Filter Flow
  - SYS-011: Complete Authentication Flow

- **Scenario E: Voucher & Promotion**
  - SYS-010: Complete Voucher Flow
  - SYS-013: Complete Promotion Flow

- **Scenario F: Refund & Cancellation**
  - SYS-012: Complete Refund Request Flow

- **Scenario G: Package Management**
  - SYS-014: Complete Package Limit Enforcement Flow

## 📝 Cập Nhật Kết Quả Test

### Cập Nhật Testing Round Summary

Sau khi chạy tests, cập nhật các giá trị trong bảng Testing Round Summary:

1. Đếm số tests đã Pass
2. Đếm số tests đã Fail
3. Đếm số tests còn Pending
4. Đánh dấu N/A nếu test không áp dụng

### Thêm Notes cho Test Cases

Bạn có thể thêm cột **Notes** hoặc **Status** sau cột Pre-conditions để ghi chú:
- ✅ Passed
- ❌ Failed
- ⏳ Pending
- ⏸️ Blocked
- ⏭️ Skipped

## 🔄 Tạo File Excel từ CSV

Nếu muốn tạo file Excel (.xlsx) thực sự với formatting:

### Option 1: Sử dụng Excel
1. Mở file CSV trong Excel
2. Format như hướng dẫn ở trên
3. Save as → Excel Workbook (.xlsx)

### Option 2: Sử dụng Python (nếu có)
```bash
pip install openpyxl pandas
python generate_excel_test_cases.py
```

### Option 3: Sử dụng C# Script
Có thể tạo script C# sử dụng EPPlus hoặc ClosedXML để generate file Excel.

## 📋 Mapping với Code

Mỗi test case trong file Excel tương ứng với một test method trong `SystemTests.cs`:

| Excel Test ID | Code Test Method | Status |
|--------------|------------------|--------|
| SYS-001 | SYS_001_CompleteTenantBookingFlow_ShouldWorkEndToEnd | ✅ |
| SYS-002 | SYS_002_CompleteHostRegistrationFlow_ShouldWorkEndToEnd | ✅ |
| SYS-003 | SYS_003_CompleteBookingWithPaymentFlow_ShouldWorkEndToEnd | ✅ |
| ... | ... | ... |

## 🎯 Best Practices

1. **Version Control**: Commit file CSV vào Git để track changes
2. **Regular Updates**: Cập nhật kết quả test sau mỗi round
3. **Documentation**: Thêm notes nếu test case có thay đổi
4. **Backup**: Giữ backup của file trước khi chỉnh sửa lớn

## 📞 Support

Nếu có vấn đề với file:
1. Check encoding (phải là UTF-8)
2. Verify CSV format (dấu phẩy, dấu ngoặc kép)
3. Check Excel version (nên dùng Excel 2016+)

## 🔗 Liên Kết

- [System Tests Summary](./SYSTEM_TESTS_SUMMARY.md)
- [System Tests Code](./SystemTests.cs)
- [Test Cases Documentation](./README_SYSTEM_TESTS.md)





