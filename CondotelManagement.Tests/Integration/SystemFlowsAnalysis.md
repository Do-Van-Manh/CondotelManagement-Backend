# Phân Tích Các Luồng Chính Trong Hệ Thống Condotel Management

## Tổng Quan

Hệ thống Condotel Management là một nền tảng quản lý đặt phòng condotel với 3 vai trò chính:
- **Admin**: Quản trị hệ thống
- **Host**: Chủ sở hữu condotel
- **Tenant**: Khách hàng đặt phòng

## 1. Luồng Authentication & Authorization

### 1.1. Đăng ký và Xác thực Tài khoản
```
1. User đăng ký tài khoản mới (Register)
   → Tạo user với status "Pending"
   → Gửi OTP qua email
   
2. User xác thực email (Verify Email/OTP)
   → Kiểm tra OTP hợp lệ
   → Cập nhật status thành "Active"
   
3. User đăng nhập (Login)
   → Xác thực credentials
   → Trả về JWT token
   
4. User quên mật khẩu (Forgot Password)
   → Gửi OTP reset password
   → Reset password với OTP
```

### 1.2. Phân quyền
- **Role-based Access Control**: Mỗi endpoint yêu cầu role cụ thể
- **JWT Token**: Chứa thông tin user và role
- **Authorization Checks**: Kiểm tra quyền truy cập ở mỗi endpoint

## 2. Luồng Host Registration & Verification

### 2.1. Đăng ký Host
```
1. User đăng nhập với role Tenant
2. Đăng ký làm Host (Register as Host)
   → Tạo Host record
   → Yêu cầu thông tin công ty
   
3. Xác minh CCCD (Verify with ID Card)
   → Upload ảnh mặt trước và mặt sau CCCD
   → OCR để trích xuất thông tin
   → So sánh với thông tin user
   → Cập nhật trạng thái verification
```

### 2.2. Quản lý Wallet (Tài khoản ngân hàng)
```
1. Host tạo wallet (Tài khoản ngân hàng)
   → Thêm thông tin ngân hàng
   → Đặt wallet mặc định
   
2. Host cập nhật/xóa wallet
   → Chỉ host sở hữu mới được thao tác
```

## 3. Luồng Condotel Management

### 3.1. Host tạo và quản lý Condotel
```
1. Host đăng nhập
2. Kiểm tra gói dịch vụ (Package)
   → Xác định số lượng condotel được phép đăng
   
3. Tạo Condotel mới
   → Nhập thông tin cơ bản (tên, mô tả, giá, beds, bathrooms)
   → Chọn Resort và Location
   → Upload ảnh
   → Thêm amenities và utilities
   
4. Cập nhật Condotel
   → Chỉ host sở hữu mới được cập nhật
   
5. Xóa Condotel (Soft delete)
   → Đánh dấu status = "Deleted"
```

### 3.2. Tenant xem và tìm kiếm Condotel
```
1. Xem danh sách Condotel (Public)
   → Lọc theo: tên, giá, location, date range, beds/bathrooms
   
2. Xem chi tiết Condotel
   → Thông tin đầy đủ, ảnh, amenities, reviews
```

## 4. Luồng Booking & Payment

### 4.1. Tạo Booking
```
1. Tenant đăng nhập
2. Kiểm tra availability
   → Check-in date, check-out date
   → Kiểm tra không trùng với booking khác
   
3. Tạo Booking
   → Tính toán giá (có thể áp dụng promotion/voucher)
   → Tạo booking với status "Pending"
   → Tính TotalPrice = PricePerNight * số đêm
   
4. Tạo Payment Link
   → Gọi PayOS API
   → Tạo payment link
   → Trả về URL thanh toán
   
5. Xử lý Payment Callback
   → PayOS gọi webhook
   → Xác thực chữ ký
   → Cập nhật booking status = "Confirmed"
   → Gửi email xác nhận
```

### 4.2. Hủy và Hoàn tiền
```
1. Tenant hủy booking
   → Kiểm tra quyền sở hữu
   → Tạo RefundRequest
   → Cập nhật booking status = "Cancelled"
   
2. Admin xử lý refund
   → Duyệt/từ chối refund request
   → Nếu duyệt: Gọi PayOS refund API
   → Cập nhật trạng thái refund
```

## 5. Luồng Review & Communication

### 5.1. Review Flow
```
1. Tenant tạo Review
   → Chỉ review được sau khi booking "Completed"
   → Rating (1-5) và Comment
   
2. Host Reply Review
   → Host có thể trả lời review
   → Chỉ host sở hữu condotel mới được reply
   
3. Host Report Review
   → Báo cáo review không phù hợp
   → Admin xử lý báo cáo
```

### 5.2. Chat Flow (SignalR)
```
1. Tenant gửi message cho Host
2. Host nhận và trả lời
3. Lưu conversation và messages
```

## 6. Luồng Package & Service Management

### 6.1. Host mua Package
```
1. Host xem danh sách packages
   → Xem các gói dịch vụ available
   
2. Host chọn package
   → Tạo HostPackage với status "PendingPayment"
   
3. Tạo Payment Link cho Package
   → Gọi PayOS API
   → OrderCode = HostId * 1_000_000_000 + PackageId * 1_000_000
   
4. Xử lý Payment Callback
   → Cập nhật HostPackage status = "Active"
   → Set StartDate và EndDate
   → Host có thể đăng condotel theo giới hạn package
```

### 6.2. Admin quản lý Packages
```
1. Admin tạo/cập nhật/xóa ServicePackage
2. Định nghĩa features (số lượng condotel được phép đăng)
```

## 7. Luồng Wallet & Payout

### 7.1. Host quản lý Wallet
```
1. Host tạo wallet (Tài khoản ngân hàng)
   → BankName, AccountNumber, AccountHolderName
   → Đặt wallet mặc định
   
2. Host cập nhật/xóa wallet
```

### 7.2. Payout Flow
```
1. Booking completed >= 15 ngày
2. Admin/Host trigger payout
   → Kiểm tra booking đã completed
   → Kiểm tra không có refund request
   → Lấy wallet mặc định của host
   → Đánh dấu IsPaidToHost = true
   → Gửi email thông báo cho host
```

## 8. Luồng Dashboard & Reporting

### 8.1. Admin Dashboard
```
1. Overview
   → Tổng số users, bookings, condotels, revenue
   
2. Revenue Chart
   → Doanh thu theo tháng/năm
   
3. Top Condotels
   → Condotel có nhiều booking nhất
   
4. Tenant Analytics
   → Phân tích hành vi khách hàng
```

### 8.2. Host Dashboard
```
1. Overview
   → Tổng bookings, revenue, condotels của host
   
2. Revenue Chart
   → Doanh thu theo thời gian
   
3. Report
   → Báo cáo doanh thu và booking theo date range
```

## 9. Luồng Master Data Management

### 9.1. Location Management
```
1. Admin/Host tạo Location
2. Admin/Host cập nhật Location
3. Admin xóa Location
```

### 9.2. Resort Management
```
1. Admin/Host tạo Resort
2. Admin/Host cập nhật Resort
3. Admin xóa Resort
```

### 9.3. Utility Management
```
1. Admin/Host tạo Utility (Amenities)
2. Admin/Host cập nhật Utility
3. Admin xóa Utility
```

## 10. Luồng Marketing Management

### 10.1. Promotion Flow
```
1. Admin tạo Promotion
   → Discount percentage
   → Date range
   → Áp dụng cho condotel hoặc toàn hệ thống
   
2. Tenant sử dụng Promotion khi booking
   → Tự động áp dụng discount
```

### 10.2. Voucher Flow
```
1. Host tạo Voucher
   → Code, discount, max uses, expiry date
   
2. Tenant sử dụng Voucher khi booking
   → Validate voucher code
   → Áp dụng discount
```

### 10.3. Blog Flow
```
1. Admin tạo Blog Post
2. Public xem Blog Posts
3. Admin quản lý Blog Categories
```

## Các Điểm Quan Trọng

### Security
- JWT Authentication cho tất cả protected endpoints
- Role-based Authorization
- Ownership checks (chỉ owner mới được sửa/xóa)

### Business Rules
- Booking availability check
- Package limits (số lượng condotel)
- Payout rules (15 ngày sau khi completed)
- Review rules (chỉ review sau khi completed)

### Integration Points
- PayOS Payment Gateway
- Email Service (OTP, notifications)
- Cloudinary (Image upload)
- DeepSeek OCR (ID Card verification)

## Test Coverage

Các luồng này cần được test đầy đủ với:
- Unit Tests: Test từng service riêng lẻ
- Integration Tests: Test các luồng end-to-end
- System Tests: Test toàn bộ hệ thống như một khối thống nhất









