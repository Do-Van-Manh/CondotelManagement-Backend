# System Tests - Hướng Dẫn Sử Dụng

## 📋 Tổng Quan

Thư mục này chứa các **System Tests** cho hệ thống Condotel Management, test các luồng chính từ frontend đến backend end-to-end.

## 📁 Cấu Trúc Files

```
Integration/
├── SystemTests.cs                      # File chứa tất cả system tests
├── SystemFlowsAnalysis.md              # Phân tích chi tiết các luồng hệ thống
├── SystemFlowsDetailedAnalysis.md      # Phân tích chi tiết backend & frontend
├── SYSTEM_TESTS_SUMMARY.md            # Tóm tắt các system tests
└── README_SYSTEM_TESTS.md             # File này
```

## 🎯 Mục Đích

System Tests được thiết kế để:
- ✅ Test các luồng nghiệp vụ chính end-to-end
- ✅ Verify integration giữa các modules
- ✅ Đảm bảo hệ thống hoạt động đúng từ đầu đến cuối
- ✅ Test cả success và failure cases
- ✅ Verify security và authorization

## 🚀 Cách Chạy Tests

### Chạy tất cả system tests
```bash
cd CondotelManagement-Backend
dotnet test --filter "Category=System"
```

### Chạy test cụ thể
```bash
# Chạy test SYS-001
dotnet test --filter "TestID=SYS-001"

# Chạy test SYS-011
dotnet test --filter "TestID=SYS-011"
```

### Chạy với output chi tiết
```bash
dotnet test --filter "Category=System" --logger "console;verbosity=detailed"
```

### Chạy và lưu kết quả vào file
```bash
dotnet test --filter "Category=System" --logger "trx;LogFileName=SystemTests.trx"
```

## 📊 Danh Sách System Tests

### Core Flows (SYS-001 đến SYS-010)
- **SYS-001**: Complete Tenant Booking Flow
- **SYS-002**: Complete Host Registration Flow
- **SYS-003**: Complete Booking with Payment Flow
- **SYS-004**: Complete Review Flow
- **SYS-005**: Complete Package Purchase Flow
- **SYS-006**: Complete Wallet and Payout Flow
- **SYS-007**: Complete Admin Management Flow
- **SYS-008**: Authorization and Security Flow
- **SYS-009**: Complete Search and Filter Flow
- **SYS-010**: Complete Voucher Flow

### Extended Flows (SYS-011 đến SYS-015) - Mới thêm
- **SYS-011**: Complete Authentication Flow
- **SYS-012**: Complete Refund Request Flow
- **SYS-013**: Complete Promotion Flow
- **SYS-014**: Complete Package Limit Enforcement Flow
- **SYS-015**: Complete Multi-Step Booking with Voucher Flow

Xem chi tiết trong file [SYSTEM_TESTS_SUMMARY.md](./SYSTEM_TESTS_SUMMARY.md)

## 📖 Tài Liệu Tham Khảo

### 1. SystemFlowsAnalysis.md
Phân tích các luồng chính trong hệ thống:
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

### 2. SystemFlowsDetailedAnalysis.md
Phân tích chi tiết backend và frontend:
- Backend endpoints cho từng module
- Frontend routes và API calls
- Luồng chi tiết từng bước
- Mapping frontend routes → backend endpoints
- Integration points với external services

### 3. SYSTEM_TESTS_SUMMARY.md
Tóm tắt tất cả system tests:
- Mô tả từng test
- Các bước thực hiện
- Status và notes
- Test coverage summary

## 🔧 Test Infrastructure

### TestBase Class
Tất cả system tests kế thừa từ `TestBase`:
- In-memory database (mỗi test có DB riêng)
- Mock services (Email, Cloudinary)
- JWT token generation
- Test data seeding

### Mock Services
- **MockEmailService**: Lưu emails và OTPs vào memory
- **MockCloudinaryService**: Trả về mock image URLs

### Test Data
Tự động seed trong `TestBase.SeedTestData()`:
- Roles: Admin, Host, Tenant
- Users: admin@test.com, host@test.com, tenant@test.com
- Host, Location, Resort, Condotel records

## 📝 Thêm System Test Mới

### Bước 1: Thêm test method vào SystemTests.cs

```csharp
[Fact]
[Trait("Category", "System")]
[Trait("TestID", "SYS-XXX")]
public async Task SYS_XXX_TestName_ShouldExpectedResult()
{
    // Arrange
    // Setup test data
    
    // Act
    // Execute test steps
    
    // Assert
    // Verify results
}
```

### Bước 2: Cập nhật SYSTEM_TESTS_SUMMARY.md
Thêm entry mới vào bảng danh sách tests

### Bước 3: Chạy test và verify
```bash
dotnet test --filter "TestID=SYS-XXX"
```

## ⚠️ Lưu Ý

1. **Test Isolation**: Mỗi test độc lập, không phụ thuộc vào test khác
2. **Database**: Mỗi test sử dụng in-memory database riêng (GUID-based)
3. **External Services**: PayOS, Email, Cloudinary được mock
4. **Error Handling**: Một số tests chấp nhận multiple status codes
5. **Test Data**: Tự động seed trước mỗi test

## 🐛 Troubleshooting

### Test fails với "Database not found"
- Đảm bảo đang chạy trong test project
- Check TestBase đã setup in-memory database đúng

### Test fails với "Unauthorized"
- Check JWT token đã được set trong header
- Verify user đã login và có role đúng

### Test fails với "Forbidden"
- Check user có đúng role không
- Verify ownership checks (chỉ owner mới được sửa/xóa)

### Test fails với "Not Found"
- Check test data đã được seed chưa
- Verify IDs trong test match với seeded data

## 📈 Test Coverage

### Đã Cover:
- ✅ Authentication (Register, Verify, Login, Forgot Password)
- ✅ Tenant Booking (Search, View, Book, Cancel, Refund)
- ✅ Host Registration & Condotel Management
- ✅ Payment Flow (PayOS)
- ✅ Review Flow (Create, Reply)
- ✅ Package Purchase Flow
- ✅ Wallet & Payout Flow
- ✅ Admin Management Flow
- ✅ Authorization & Security
- ✅ Search & Filter
- ✅ Voucher & Promotion Flow
- ✅ Refund Request Flow
- ✅ Package Limit Enforcement

### Chưa Cover:
- ⏳ Chat Flow (SignalR) - Cần test real-time
- ⏳ ID Card Verification (DeepSeek OCR) - Cần mock OCR
- ⏳ Blog Flow - Có thể thêm sau

## 🔗 Liên Kết

- [Integration Tests README](./README.md)
- [Business Flow Tests](./README_BUSINESS_FLOWS.md)
- [Test Cases](./TestCases/)

## 📞 Support

Nếu có vấn đề hoặc câu hỏi về system tests, vui lòng:
1. Check documentation trong các file .md
2. Review test code trong SystemTests.cs
3. Check test output với verbosity=detailed





