# Tóm Tắt System Tests - Các Luồng Chính

## Tổng Quan

File `SystemTests.cs` chứa các **System Tests** - test các luồng chính của hệ thống end-to-end, từ frontend đến backend, bao gồm cả các tương tác với external services (PayOS, Email, Cloudinary).

## Danh Sách System Tests

### ✅ SYS-001: Complete Tenant Booking Flow
**Mô tả**: Test luồng hoàn chỉnh từ đăng ký tenant đến đặt phòng
**Các bước**:
1. Register new tenant
2. Verify email với OTP
3. Login
4. View condotels (public)
5. View condotel detail
6. Check availability
7. Create booking
8. Get my bookings

**Status**: ✅ Hoàn thành

---

### ✅ SYS-002: Complete Host Registration Flow
**Mô tả**: Test luồng đăng ký Host và quản lý Condotel
**Các bước**:
1. Register new user
2. Verify email
3. Login
4. Register as Host
5. Create Wallet (bank account)
6. Get wallets
7. Create Condotel
8. Get my condotels

**Status**: ✅ Hoàn thành

---

### ✅ SYS-003: Complete Booking with Payment Flow
**Mô tả**: Test luồng tạo booking và thanh toán
**Các bước**:
1. Login as tenant
2. Create booking
3. Create payment link (PayOS)
4. Verify booking status

**Status**: ✅ Hoàn thành
**Note**: Payment link creation có thể fail nếu PayOS service chưa được cấu hình (acceptable trong test environment)

---

### ✅ SYS-004: Complete Review Flow
**Mô tả**: Test luồng review từ tenant và reply từ host
**Các bước**:
1. Create completed booking
2. Tenant login và tạo review
3. Get reviews
4. Host login và reply review
5. Verify reply

**Status**: ✅ Hoàn thành

---

### ✅ SYS-005: Complete Package Purchase Flow
**Mô tả**: Test luồng Host mua package dịch vụ
**Các bước**:
1. Host login
2. Get available packages
3. Get my current package

**Status**: ✅ Hoàn thành
**Note**: Actual package purchase requires PayOS integration

---

### ✅ SYS-006: Complete Wallet and Payout Flow
**Mô tả**: Test luồng tạo wallet và xử lý payout
**Các bước**:
1. Host login
2. Create wallet
3. Get wallets
4. Create completed booking (>= 15 days ago)
5. Admin login và process payout

**Status**: ✅ Hoàn thành

---

### ✅ SYS-007: Complete Admin Management Flow
**Mô tả**: Test luồng quản lý của Admin
**Các bước**:
1. Admin login
2. Get all users
3. Get user by ID
4. Get all locations
5. Get all resorts
6. Get dashboard overview
7. Get revenue chart

**Status**: ✅ Hoàn thành

---

### ✅ SYS-008: Authorization and Security Flow
**Mô tả**: Test các luồng bảo mật và phân quyền
**Các bước**:
1. Access protected endpoint without token → 401 Unauthorized
2. Access with wrong role → 403 Forbidden
3. Access other user's booking → 403/404
4. Admin can access admin endpoints → 200 OK

**Status**: ✅ Hoàn thành

---

### ✅ SYS-009: Complete Search and Filter Flow
**Mô tả**: Test luồng tìm kiếm và lọc Condotel
**Các bước**:
1. Search by name
2. Filter by price
3. Filter by beds and bathrooms
4. Filter by location
5. Combined filters

**Status**: ✅ Hoàn thành

---

### ✅ SYS-010: Complete Voucher Flow
**Mô tả**: Test luồng Host tạo voucher và Tenant sử dụng
**Các bước**:
1. Host login
2. Host creates voucher
3. Get vouchers by host
4. Public view vouchers for condotel
5. Tenant login và create booking (voucher support depends on implementation)

**Status**: ✅ Hoàn thành

---

### ✅ SYS-011: Complete Authentication Flow
**Mô tả**: Test luồng authentication hoàn chỉnh
**Các bước**:
1. Register new user
2. Verify email với OTP
3. Login với verified account
4. Get current user info
5. Forgot password flow
6. Reset password với OTP
7. Login với password mới

**Status**: ✅ Hoàn thành (Mới thêm)

---

### ✅ SYS-012: Complete Refund Request Flow
**Mô tả**: Test luồng Tenant yêu cầu refund và Admin xử lý
**Các bước**:
1. Create confirmed booking
2. Tenant login và request refund
3. Verify refund request was created
4. Admin login và view refund requests
5. Admin approve/reject (nếu có endpoint)

**Status**: ✅ Hoàn thành (Mới thêm)

---

### ✅ SYS-013: Complete Promotion Flow
**Mô tả**: Test luồng Host tạo promotion và Tenant xem
**Các bước**:
1. Host login
2. Host creates promotion
3. Get promotions by host
4. Public view promotions for condotel
5. Tenant can see promotions when viewing condotel detail

**Status**: ✅ Hoàn thành (Mới thêm)

---

### ✅ SYS-014: Complete Package Limit Enforcement Flow
**Mô tả**: Test việc enforce giới hạn số lượng condotel theo package
**Các bước**:
1. Host login
2. Check current package
3. Get current condotel count
4. Try to create condotel
5. Verify response (403 nếu vượt quá giới hạn)
6. Verify condotel count

**Status**: ✅ Hoàn thành (Mới thêm)

---

### ✅ SYS-015: Complete Multi-Step Booking with Voucher Flow
**Mô tả**: Test luồng phức tạp từ tìm kiếm đến đặt phòng với voucher
**Các bước**:
1. Search condotels
2. View condotel detail
3. View vouchers for condotel
4. Tenant login
5. Check availability
6. Create booking (voucher support depends on implementation)
7. Verify booking was created

**Status**: ✅ Hoàn thành (Mới thêm)

---

## Cách Chạy System Tests

### Chạy tất cả system tests
```bash
dotnet test --filter "Category=System"
```

### Chạy test cụ thể theo TestID
```bash
dotnet test --filter "TestID=SYS-001"
dotnet test --filter "TestID=SYS-011"
```

### Chạy với output chi tiết
```bash
dotnet test --filter "Category=System" --logger "console;verbosity=detailed"
```

### Chạy và xem kết quả dạng tr
```bash
dotnet test --filter "Category=System" --logger "trx;LogFileName=SystemTests.trx"
```

## Test Coverage Summary

### Đã Cover:
- ✅ Authentication (Register, Verify, Login, Forgot Password, Reset Password)
- ✅ Tenant Booking (Search, View, Book, Check Availability)
- ✅ Host Registration & Condotel Management
- ✅ Payment Flow (PayOS integration)
- ✅ Review Flow (Create, Reply)
- ✅ Package Purchase Flow
- ✅ Wallet & Payout Flow
- ✅ Admin Management Flow
- ✅ Authorization & Security
- ✅ Search & Filter
- ✅ Voucher Flow
- ✅ Promotion Flow
- ✅ Refund Request Flow
- ✅ Package Limit Enforcement

### Chưa Cover (Có thể thêm sau):
- ⏳ Chat Flow (SignalR) - Cần test real-time connection
- ⏳ ID Card Verification Flow (DeepSeek OCR) - Cần mock OCR service
- ⏳ Email Service Integration - Đã mock nhưng có thể test chi tiết hơn
- ⏳ Image Upload Flow (Cloudinary) - Đã mock nhưng có thể test chi tiết hơn

## Test Data

Tất cả tests sử dụng in-memory database với test data được seed trong `TestBase.SeedTestData()`:
- 3 Roles: Admin, Host, Tenant
- 3 Users: admin@test.com, host@test.com, tenant@test.com
- 1 Host record
- 1 Location
- 1 Resort
- 1 Condotel

## Mock Services

Các external services được mock:
- **MockEmailService**: Lưu emails và OTPs vào memory
- **MockCloudinaryService**: Trả về mock URLs

## Notes

1. **Test Isolation**: Mỗi test sử dụng in-memory database riêng (GUID-based)
2. **JWT Tokens**: Tự động generate cho mỗi test
3. **Test Data**: Tự động seed trước mỗi test
4. **Error Handling**: Một số tests chấp nhận multiple status codes (OK, Created, Forbidden) vì business logic có thể thay đổi

## Mapping với Google Sheets

Mỗi test trong code tương ứng với một dòng trong Google Sheets:

| TestID | Test Case Name | Status | Notes |
|--------|---------------|--------|-------|
| SYS-001 | Complete Tenant Booking Flow | ✅ | |
| SYS-002 | Complete Host Registration Flow | ✅ | |
| SYS-003 | Complete Booking with Payment Flow | ✅ | Payment may fail if PayOS not configured |
| SYS-004 | Complete Review Flow | ✅ | |
| SYS-005 | Complete Package Purchase Flow | ✅ | |
| SYS-006 | Complete Wallet and Payout Flow | ✅ | |
| SYS-007 | Complete Admin Management Flow | ✅ | |
| SYS-008 | Authorization and Security Flow | ✅ | |
| SYS-009 | Complete Search and Filter Flow | ✅ | |
| SYS-010 | Complete Voucher Flow | ✅ | |
| SYS-011 | Complete Authentication Flow | ✅ | Mới thêm |
| SYS-012 | Complete Refund Request Flow | ✅ | Mới thêm |
| SYS-013 | Complete Promotion Flow | ✅ | Mới thêm |
| SYS-014 | Complete Package Limit Enforcement Flow | ✅ | Mới thêm |
| SYS-015 | Complete Multi-Step Booking with Voucher Flow | ✅ | Mới thêm |

## Thêm Test Mới

Để thêm system test mới, follow format:

```csharp
[Fact]
[Trait("Category", "System")]
[Trait("TestID", "SYS-XXX")]
public async Task SYS_XXX_TestName_ShouldExpectedResult()
{
    // Arrange
    // ...
    
    // Act
    // ...
    
    // Assert
    // ...
}
```

## Best Practices

1. **Test Naming**: Sử dụng format `SYS_XXX_TestName_ShouldExpectedResult`
2. **Test Isolation**: Mỗi test độc lập, không phụ thuộc vào test khác
3. **Assertions**: Sử dụng FluentAssertions cho readable assertions
4. **Error Handling**: Test cả success và failure cases
5. **Documentation**: Thêm XML comments cho mỗi test method





