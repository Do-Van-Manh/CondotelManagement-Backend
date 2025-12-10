# Hướng Dẫn Integration Tests

## Tổng Quan

Dự án có 3 file integration tests chính:

1. **CompleteBusinessFlowTests.cs**: Các test cases cơ bản theo luồng nghiệp vụ
2. **AdditionalIntegrationTests.cs**: Các test cases bổ sung từ CSV (validation, error cases)
3. **ExtendedIntegrationTests.cs**: Các test cases cho Location, Resort, Utility, Blog, Promotion, Package, Profile
4. **SystemTests.cs**: System tests end-to-end cho các luồng chính

## Cấu Trúc Test

Mỗi test case được đánh dấu với:
- `[Trait("Category", "ModuleName")]`: Module (Authentication, Booking, Review, etc.)
- `[Trait("TestID", "TC-XXX-XXX")]`: Mã test case từ CSV

## Kiểm Tra Output Tiếng Việt

Tất cả tests đều kiểm tra output tiếng Việt từ API:

```csharp
// Ví dụ: Kiểm tra message tiếng Việt
var content = await response.Content.ReadAsStringAsync();
content.Should().Contain("thành công"); // Success message
content.Should().Contain("Không tìm thấy"); // Not found message
content.Should().ContainAny("không hợp lệ", "sai", "hết hạn"); // Error messages
```

## Chạy Tests

### Chạy tất cả integration tests
```bash
dotnet test --filter "Category=Authentication|Category=Booking|Category=Review|Category=Host|Category=Admin|Category=Voucher|Category=Payment|Category=Location|Category=Resort|Category=Utility|Category=Blog|Category=Promotion|Category=ServicePackage|Category=Package|Category=Profile|Category=HostProfile|Category=System"
```

### Chạy tests theo module
```bash
# Authentication tests
dotnet test --filter "Category=Authentication"

# Booking tests
dotnet test --filter "Category=Booking"

# Admin tests
dotnet test --filter "Category=Admin"
```

### Chạy test cụ thể theo TestID
```bash
dotnet test --filter "TestID=TC-AUTH-001"
```

### Chạy với output chi tiết
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Mapping với CSV

Mỗi test trong code tương ứng với một dòng trong `TestCases_Reorganized1.csv`:

| TestID | Test Case Name | File | Status |
|--------|---------------|------|--------|
| TC-AUTH-001 | Register New User | CompleteBusinessFlowTests.cs | ✅ |
| TC-AUTH-002 | Register with Existing Email | AdditionalIntegrationTests.cs | ✅ |
| TC-ADMIN-002 | Get All Users | AdditionalIntegrationTests.cs | ✅ |
| TC-LOCATION-001 | Get All Locations | ExtendedIntegrationTests.cs | ✅ |
| ... | ... | ... | ... |

## Test Coverage

### Đã Cover:
- ✅ Authentication (Register, Login, Verify, Reset Password, Google Login, Logout)
- ✅ Admin Management (CRUD Users, Dashboard, Reports)
- ✅ Booking (Create, Update, Cancel, Check Availability, Payment)
- ✅ Review (Create, Update, Delete, Host Reply, Report)
- ✅ Host Management (Register, Condotel CRUD, Profile)
- ✅ Voucher (CRUD, View)
- ✅ Payment (Create Payment Link, Validation)
- ✅ Location (CRUD, Validation)
- ✅ Resort (CRUD, Validation)
- ✅ Utility (CRUD, Validation)
- ✅ Blog (Get Posts, Categories)
- ✅ Promotion (CRUD, Validation)
- ✅ Service Package (CRUD)
- ✅ Package (Get Available)
- ✅ Profile (Get, Update)
- ✅ Host Profile (Get, Update)
- ✅ System Flows (End-to-end)

### Chưa Cover (có thể thêm sau):
- ⏳ Chat Flow (SignalR - cần mock SignalR)
- ⏳ Reward Points Flow (nếu có trong code)
- ⏳ Upload Image Tests (cần mock Cloudinary)
- ⏳ Payout Flow (cần mock payment gateway)
- ⏳ Refund Flow

## Notes

1. **Test Isolation**: Mỗi test sử dụng in-memory database riêng
2. **Test Data**: TestBase tự động seed data cần thiết (Users, Roles, Host, Location, Resort, Condotel)
3. **Mock Services**: Email và Cloudinary được mock trong TestBase
4. **JWT Tokens**: Tự động generate cho mỗi test với `GenerateJwtToken()`
5. **Vietnamese Output**: Tất cả tests kiểm tra output tiếng Việt từ API

## Thêm Test Mới

Để thêm test mới theo format:

```csharp
[Fact]
[Trait("Category", "ModuleName")]
[Trait("TestID", "TC-XXX-XXX")]
public async Task TC_XXX_XXX_TestName_ShouldExpectedResult()
{
    // Arrange
    // Setup test data và authentication
    
    // Act
    // Call API endpoint
    
    // Assert
    // Kiểm tra status code và output tiếng Việt
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadAsStringAsync();
    content.Should().Contain("thành công"); // Kiểm tra message tiếng Việt
}
```

## Best Practices

1. **Luôn kiểm tra output tiếng Việt**: Sử dụng `Contain()`, `ContainAny()` để kiểm tra messages
2. **Kiểm tra cả success và error cases**: Cover cả happy path và error scenarios
3. **Sử dụng TestID từ CSV**: Đảm bảo mapping chính xác với test cases
4. **Clean up test data**: Mỗi test tự động cleanup nhờ in-memory database
5. **Kiểm tra database state**: Verify data được lưu đúng trong database sau mỗi operation








