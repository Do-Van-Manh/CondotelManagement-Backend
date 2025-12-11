# Integration Tests cho Condotel Management Backend

## Tổng quan

Dự án này chứa các integration tests cho hệ thống Condotel Management Backend, sử dụng:
- **xUnit** - Testing framework
- **Microsoft.AspNetCore.Mvc.Testing** - TestServer cho integration testing
- **Entity Framework In-Memory Database** - Database test
- **FluentAssertions** - Assertion library
- **Moq** - Mocking framework (cho external services)

## Cấu trúc

```
CondotelManagement.Tests/
├── Integration/
│   ├── TestBase.cs                    # Base class cho tất cả tests
│   ├── AuthIntegrationTests.cs        # Tests cho Authentication
│   ├── BookingIntegrationTests.cs     # Tests cho Booking system
│   ├── CondotelIntegrationTests.cs    # Tests cho Condotel CRUD
│   ├── ReviewIntegrationTests.cs      # Tests cho Review system
│   ├── PaymentIntegrationTests.cs      # Tests cho Payment
│   └── VoucherIntegrationTests.cs     # Tests cho Voucher
└── CondotelManagement.Tests.csproj
```

## Chạy Tests

### Visual Studio
1. Mở Test Explorer (Test > Test Explorer)
2. Click "Run All Tests"

### Command Line
```bash
dotnet test
```

### Chạy một test cụ thể
```bash
dotnet test --filter "FullyQualifiedName~AuthIntegrationTests"
```

## Test Coverage

### Authentication Tests
- ✅ Login với credentials hợp lệ
- ✅ Login với credentials không hợp lệ
- ✅ Register user mới
- ✅ Register với email đã tồn tại
- ✅ Verify email với OTP
- ✅ Verify email với OTP sai
- ✅ Get current user profile
- ✅ Forgot password
- ✅ Reset password với OTP

### Booking Tests
- ✅ Tạo booking hợp lệ
- ✅ Tạo booking với ngày quá khứ
- ✅ Tạo booking với ngày trùng lặp
- ✅ Check availability
- ✅ Lấy danh sách bookings của user
- ✅ Cancel booking
- ✅ Cancel booking của user khác (forbidden)
- ✅ Tạo booking với promotion

### Condotel Tests
- ✅ Lấy danh sách condotels
- ✅ Lấy condotel theo ID
- ✅ Tạo condotel (Host)
- ✅ Update condotel (Host)
- ✅ Delete condotel (Host)
- ✅ Tạo condotel với role không đúng (forbidden)

### Review Tests
- ✅ Tạo review cho booking đã completed
- ✅ Tạo review cho booking pending (should fail)
- ✅ Lấy reviews của condotel
- ✅ Host reply review

### Payment Tests
- ✅ Tạo payment link
- ✅ Tạo payment không có authentication
- ✅ Tạo payment với booking không tồn tại

### Voucher Tests
- ✅ Tạo voucher (Host)
- ✅ Tạo voucher với code trùng
- ✅ Lấy vouchers của condotel
- ✅ Apply voucher khi booking

## Mock Services

### MockEmailService
- Mock email service để không gửi email thật trong tests
- Lưu danh sách emails và OTPs đã gửi để verify

### MockCloudinaryService
- Mock Cloudinary service để không upload ảnh thật
- Trả về mock URL

## Test Data

TestBase tự động seed test data:
- 3 Roles: Admin, Host, Tenant
- 3 Users: admin@test.com, host@test.com, tenant@test.com
- 1 Host
- 1 Location
- 1 Resort
- 1 Condotel

## Best Practices

1. **Isolation**: Mỗi test sử dụng in-memory database riêng (Guid.NewGuid())
2. **Cleanup**: TestBase.Dispose() tự động xóa database sau mỗi test
3. **Authentication**: Helper method `GenerateJwtToken()` và `SetAuthHeader()` để test authenticated endpoints
4. **Assertions**: Sử dụng FluentAssertions cho readable assertions

## Lưu ý

- Tests sử dụng in-memory database, không ảnh hưởng đến database thật
- External services (Email, Cloudinary, PayOS) được mock
- JWT tokens được generate với test key (không dùng production key)

## Mở rộng

Để thêm test mới:
1. Tạo class mới kế thừa từ `TestBase`
2. Implement `IClassFixture<WebApplicationFactory<Program>>`
3. Sử dụng `Client` để gọi API endpoints
4. Sử dụng `DbContext` để verify database state
5. Sử dụng FluentAssertions để assert results

## Troubleshooting

### Lỗi: "Program not accessible"
- Đảm bảo có `public partial class Program { }` trong TestBase.cs

### Lỗi: "Database already exists"
- TestBase tự động tạo database với Guid unique, không nên có lỗi này

### Lỗi: "JWT token invalid"
- Kiểm tra JWT key trong TestBase phải match với appsettings.json















