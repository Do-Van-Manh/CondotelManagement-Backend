# Test Cases Excel File - Hướng Dẫn Sử Dụng

## File Excel

File **`TestCases_AllModules.csv`** chứa **94 test cases** cho tất cả các modules trong hệ thống Condotel Management.

## Cách Mở File Excel

### Option 1: Mở trực tiếp bằng Excel
1. Double-click file `TestCases_AllModules.csv`
2. Excel sẽ tự động mở file
3. Nếu Excel hỏi về encoding, chọn **UTF-8**

### Option 2: Import vào Excel
1. Mở Excel
2. Data > Get Data > From File > From Text/CSV
3. Chọn file `TestCases_AllModules.csv`
4. Chọn encoding: **UTF-8**
5. Click "Load"

### Option 3: Import vào Google Sheets
1. Mở Google Sheets
2. File > Import > Upload
3. Chọn file `TestCases_AllModules.csv`
4. Chọn "Replace spreadsheet"
5. Import location: "Replace current sheet"

## Cấu Trúc File

File có các cột sau:

| Cột | Mô tả |
|-----|-------|
| **STT** | Số thứ tự |
| **Test Case ID** | Mã test case (TC-XXX-XXX) |
| **Test Case Name** | Tên test case |
| **Test Scenario** | Mô tả scenario |
| **Precondition** | Điều kiện tiên quyết |
| **Test Steps** | Các bước thực hiện |
| **Expected Result** | Kết quả mong đợi |
| **Priority** | Độ ưu tiên (High/Medium/Low) |
| **Status** | Trạng thái (✅ Implemented / ⏳ Not Implemented) |
| **Module** | Module thuộc về |

## Thống Kê Test Cases

### Theo Module:
- **Authentication**: 6 test cases
- **Tenant**: 5 test cases
- **Booking**: 11 test cases
- **Payment**: 7 test cases
- **Review**: 8 test cases
- **Host**: 6 test cases
- **Voucher**: 5 test cases
- **Admin**: 5 test cases
- **Authorization**: 2 test cases
- **RewardPoints**: 4 test cases
- **Chat**: 3 test cases
- **Blog**: 5 test cases
- **Promotion**: 5 test cases
- **ServicePackage**: 4 test cases
- **Location**: 4 test cases
- **Resort**: 3 test cases
- **Utility**: 4 test cases
- **Profile**: 2 test cases
- **Upload**: 3 test cases
- **HostPackage**: 2 test cases

### Theo Status:
- ✅ **Implemented**: 60+ test cases
- ⏳ **Not Implemented**: 30+ test cases (cần thêm tests)

### Theo Priority:
- **High**: 45+ test cases
- **Medium**: 45+ test cases
- **Low**: 0 test cases

## Mapping với Code

Mỗi test case trong Excel tương ứng với một test method trong code:

| Test Case ID | Test Method | File |
|--------------|-------------|------|
| TC-AUTH-001 | `TC_AUTH_001_RegisterNewUser_ShouldCreatePendingUser` | `CompleteBusinessFlowTests.cs` |
| TC-AUTH-002 | `TC_AUTH_002_VerifyEmailWithOTP_ShouldActivateUser` | `CompleteBusinessFlowTests.cs` |
| TC-BOOKING-001 | `TC_BOOKING_001_CheckAvailability_Available_ShouldReturnTrue` | `CompleteBusinessFlowTests.cs` |
| TC-REWARD-001 | `TC_REWARD_001_GetMyPoints_ShouldReturnPoints` | `AllModulesIntegrationTests.cs` |
| ... | ... | ... |

## Cách Sử Dụng

### 1. Tracking Test Progress
- Cập nhật cột **Status** khi implement test mới
- ✅ = Đã implement và test pass
- ⏳ = Chưa implement
- ❌ = Test fail (cần fix)

### 2. Filter Tests
Trong Excel, bạn có thể:
- Filter theo **Module** để xem tests của module cụ thể
- Filter theo **Status** để xem tests chưa implement
- Filter theo **Priority** để ưu tiên implement

### 3. Export Test Plan
- Có thể export từng module ra file riêng
- Có thể tạo test plan document từ file này

## Chạy Tests

### Chạy tất cả tests đã implement:
```bash
dotnet test
```

### Chạy tests theo module:
```bash
# Authentication tests
dotnet test --filter "Category=Authentication"

# Booking tests
dotnet test --filter "Category=Booking"
```

### Chạy test cụ thể:
```bash
dotnet test --filter "TestID=TC-AUTH-001"
```

## Notes

1. File CSV sử dụng **UTF-8 encoding** để hỗ trợ tiếng Việt
2. Các test cases được sắp xếp theo thứ tự logic nghiệp vụ
3. Mỗi test case có thể trace về code implementation qua TestID
4. File có thể được import vào bất kỳ tool quản lý test nào (Jira, TestRail, etc.)

## Cập Nhật File

Khi thêm test case mới:
1. Thêm dòng mới vào file CSV
2. Đảm bảo Test Case ID là unique
3. Cập nhật STT
4. Implement test trong code với `[Trait("TestID", "TC-XXX-XXX")]`
5. Cập nhật Status = ✅ Implemented

## Troubleshooting

### Lỗi: File không hiển thị tiếng Việt đúng
- **Giải pháp**: Mở file bằng Excel và chọn encoding UTF-8 khi import

### Lỗi: Dấu phẩy trong nội dung bị lỗi
- **Giải pháp**: File CSV sử dụng dấu phẩy làm delimiter, nội dung có dấu phẩy được đặt trong dấu ngoặc kép

### Lỗi: Không mở được trong Excel
- **Giải pháp**: Thử import thay vì mở trực tiếp, hoặc dùng Google Sheets














