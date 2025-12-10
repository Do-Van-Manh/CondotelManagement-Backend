# Integration Tests - Luồng Nghiệp Vụ

## Tổng quan

File `CompleteBusinessFlowTests.cs` chứa các integration tests được viết theo đúng **Test Cases** trong file `TestCases/BusinessFlowTestCases.md`.

Mỗi test được đánh dấu với:
- **Category**: Module (Authentication, Booking, Review, etc.)
- **TestID**: Mã test case (TC-AUTH-001, TC-BOOKING-001, etc.)

## Cấu trúc Test Cases

### 1. Authentication Flow (TC-AUTH-001 to TC-AUTH-005)
- ✅ TC-AUTH-001: Register New User
- ✅ TC-AUTH-002: Verify Email với OTP
- ✅ TC-AUTH-003: Login với Credentials hợp lệ
- ✅ TC-AUTH-004: Login với Credentials không hợp lệ
- ✅ TC-AUTH-005: Forgot Password Flow

### 2. Tenant Flow - Xem & Tìm kiếm Condotel (TC-TENANT-001 to TC-TENANT-005)
- ✅ TC-TENANT-001: Xem danh sách Condotel
- ✅ TC-TENANT-002: Tìm kiếm Condotel theo tên
- ✅ TC-TENANT-003: Filter Condotel theo giá
- ✅ TC-TENANT-004: Filter Condotel theo beds/bathrooms
- ✅ TC-TENANT-005: Xem chi tiết Condotel

### 3. Booking Flow (TC-BOOKING-001 to TC-BOOKING-011)
- ✅ TC-BOOKING-001: Check Availability - Available
- ✅ TC-BOOKING-002: Check Availability - Not Available
- ✅ TC-BOOKING-003: Tạo Booking hợp lệ
- ✅ TC-BOOKING-004: Tạo Booking với ngày quá khứ
- ✅ TC-BOOKING-005: Tạo Booking với ngày trùng lặp
- ✅ TC-BOOKING-006: Tạo Booking với Promotion
- ✅ TC-BOOKING-008: Xem danh sách Bookings của mình
- ✅ TC-BOOKING-010: Cancel Booking
- ✅ TC-BOOKING-011: Cancel Booking của user khác

### 4. Review Flow (TC-REVIEW-001 to TC-REVIEW-007)
- ✅ TC-REVIEW-001: Tạo Review cho Booking đã Completed
- ✅ TC-REVIEW-002: Tạo Review cho Booking chưa Completed
- ✅ TC-REVIEW-004: Xem danh sách Reviews của mình
- ✅ TC-REVIEW-007: Host Reply Review

### 5. Host Flow (TC-HOST-001 to TC-HOST-006)
- ✅ TC-HOST-001: Tạo Condotel
- ✅ TC-HOST-004: Xem danh sách Condotel của mình

### 6. Voucher Flow (TC-VOUCHER-001 to TC-VOUCHER-003)
- ✅ TC-VOUCHER-001: Host tạo Voucher
- ✅ TC-VOUCHER-002: Tạo Voucher với Code trùng

### 7. Admin Flow (TC-ADMIN-001 to TC-ADMIN-003)
- ✅ TC-ADMIN-001: Xem Dashboard

### 8. Authorization Tests (TC-AUTHZ-001 to TC-AUTHZ-002)
- ✅ TC-AUTHZ-001: Access với Role không đúng
- ✅ TC-AUTHZ-002: Access không có Token

## Chạy Tests

### Chạy tất cả tests
```bash
dotnet test
```

### Chạy tests theo Category
```bash
# Chỉ chạy Authentication tests
dotnet test --filter "Category=Authentication"

# Chỉ chạy Booking tests
dotnet test --filter "Category=Booking"
```

### Chạy test cụ thể theo TestID
```bash
dotnet test --filter "TestID=TC-AUTH-001"
```

### Chạy với output chi tiết
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Mapping với Google Sheets

Mỗi test trong code tương ứng với một dòng trong Google Sheets:

| TestID | Test Case Name | Status |
|--------|---------------|--------|
| TC-AUTH-001 | Register New User | ✅ |
| TC-AUTH-002 | Verify Email với OTP | ✅ |
| TC-AUTH-003 | Login với Credentials hợp lệ | ✅ |
| ... | ... | ... |

## Test Coverage

### Đã cover:
- ✅ Authentication (Register, Login, Verify Email, Forgot Password)
- ✅ Tenant: Xem & Search Condotel
- ✅ Booking: Create, Check Availability, Cancel
- ✅ Review: Create, Get, Host Reply
- ✅ Host: Create Condotel, Get Condotels
- ✅ Voucher: Create, Duplicate Check
- ✅ Admin: Dashboard
- ✅ Authorization: Role-based access

### Chưa cover (có thể thêm sau):
- ⏳ Payment Flow (cần mock PayOS service đầy đủ)
- ⏳ Reward Points Flow
- ⏳ Chat Flow (SignalR)
- ⏳ Blog Flow
- ⏳ Promotion Flow
- ⏳ Service Package Flow

## Notes

1. **Test Isolation**: Mỗi test sử dụng in-memory database riêng
2. **Test Data**: TestBase tự động seed data cần thiết
3. **Mock Services**: Email và Cloudinary được mock
4. **JWT Tokens**: Tự động generate cho mỗi test

## Integration Tests Mới Được Bổ Sung

Dựa trên file `TestCases_Reorganized1.csv`, đã bổ sung thêm 2 file integration tests:

### 1. AdditionalIntegrationTests.cs
Chứa các test cases bổ sung cho:
- **TC-AUTH-002 đến TC-AUTH-044**: Các test cases authentication mở rộng (validation, error cases)
- **TC-ADMIN-002 đến TC-ADMIN-030**: Admin management tests (CRUD users, validation)
- **TC-BOOKING-011 đến TC-BOOKING-017**: Booking tests mở rộng (update, host bookings)
- **TC-REVIEW-005 đến TC-REVIEW-018**: Review tests mở rộng (update, delete, host actions)
- **TC-HOST-005 đến TC-HOST-011**: Host tests mở rộng (CRUD condotel, validation)
- **TC-VOUCHER-004 đến TC-VOUCHER-012**: Voucher tests mở rộng (CRUD, validation)
- **TC-PAYMENT-001 đến TC-PAYMENT-004**: Payment tests (create payment link, validation)
- **TC-HOST-004**: Register as Host
- **TC-TENANT-005 đến TC-TENANT-007**: Tenant filter tests mở rộng

### 2. ExtendedIntegrationTests.cs
Chứa các test cases cho các module:
- **TC-LOCATION-001 đến TC-LOCATION-010**: Location management (CRUD, validation)
- **TC-RESORT-001 đến TC-RESORT-007**: Resort management (CRUD, validation)
- **TC-UTILITY-001 đến TC-UTILITY-010**: Utility management (CRUD, validation)
- **TC-BLOG-001 đến TC-BLOG-003**: Blog tests (get posts, categories)
- **TC-PROMO-001 đến TC-PROMO-013**: Promotion management (CRUD, validation)
- **TC-SERVICEPKG-001 đến TC-SERVICEPKG-010**: Service package management
- **TC-PACKAGE-001 đến TC-PACKAGE-002**: Package tests
- **TC-PROFILE-001 đến TC-PROFILE-002**: Profile management
- **TC-HOSTPROFILE-001 đến TC-HOSTPROFILE-005**: Host profile management
- **TC-VOUCHER-003**: View condotel vouchers
- **TC-ADMIN-001**: Admin dashboard

### Đặc Điểm
- ✅ Tất cả tests kiểm tra output tiếng Việt từ API
- ✅ Kiểm tra validation errors với message tiếng Việt
- ✅ Kiểm tra success messages với từ khóa "thành công", "Không tìm thấy", etc.
- ✅ Cover các edge cases và error scenarios

## System Tests

File `SystemTests.cs` chứa các **System Tests** - test các luồng chính của hệ thống end-to-end.

### Các System Tests

1. **SYS-001**: Complete Tenant Booking Flow
   - Đăng ký → Xác thực → Xem Condotel → Đặt phòng

2. **SYS-002**: Complete Host Registration Flow
   - Đăng ký → Đăng ký Host → Tạo Wallet → Tạo Condotel

3. **SYS-003**: Complete Booking with Payment Flow
   - Tạo Booking → Tạo Payment Link → Xử lý Payment

4. **SYS-004**: Complete Review Flow
   - Booking Completed → Tenant tạo Review → Host Reply

5. **SYS-005**: Complete Package Purchase Flow
   - Host xem Packages → Chọn Package → Thanh toán

6. **SYS-006**: Complete Wallet and Payout Flow
   - Host tạo Wallet → Booking Completed → Payout Process

7. **SYS-007**: Complete Admin Management Flow
   - Admin quản lý Users → Locations → Dashboard

8. **SYS-008**: Authorization and Security Flow
   - Unauthorized access → Wrong role → Ownership checks

9. **SYS-009**: Complete Search and Filter Flow
   - Tìm kiếm và lọc Condotel theo nhiều tiêu chí

10. **SYS-010**: Complete Voucher and Promotion Flow
    - Host tạo Voucher → Tenant sử dụng khi booking

### Phân Tích Luồng Hệ Thống

Xem file `SystemFlowsAnalysis.md` để hiểu chi tiết về các luồng chính trong hệ thống:
- Authentication & Authorization Flow
- Host Registration & Verification Flow
- Condotel Management Flow
- Booking & Payment Flow
- Review & Communication Flow
- Package & Service Management Flow
- Wallet & Payout Flow
- Dashboard & Reporting Flow
- Master Data Management Flow
- Marketing Management Flow

### Chạy System Tests

```bash
# Chạy tất cả system tests
dotnet test --filter "Category=System"

# Chạy test cụ thể
dotnet test --filter "TestID=SYS-001"
```

## Thêm Test Mới

Để thêm test mới theo format:

```csharp
[Fact]
[Trait("Category", "ModuleName")]
[Trait("TestID", "TC-XXX-XXX")]
public async Task TC_XXX_XXX_TestName_ShouldExpectedResult()
{
    // Arrange
    // ...
    
    // Act
    // ...
    
    // Assert
    // ...
}
```






