# Phân Tích Chi Tiết Các Luồng Chính - Backend & Frontend

## Tổng Quan Hệ Thống

### Backend (C# .NET)
- **Framework**: ASP.NET Core Web API
- **Database**: SQL Server (Entity Framework Core)
- **Authentication**: JWT Bearer Token
- **Authorization**: Role-based (Admin, Host, Tenant)
- **External Services**: PayOS (Payment), Cloudinary (Image), DeepSeek OCR (ID Verification), Email Service

### Frontend (React/TypeScript)
- **Framework**: React 18.2.0 với TypeScript
- **Routing**: React Router DOM v6
- **State Management**: Context API (AuthContext)
- **HTTP Client**: Axios
- **Real-time**: SignalR (@microsoft/signalr)
- **UI**: Tailwind CSS, Headless UI

---

## 1. LUỒNG AUTHENTICATION & AUTHORIZATION

### 1.1. Backend Endpoints
```
POST /api/Auth/register          - Đăng ký tài khoản
POST /api/Auth/verify-email      - Xác thực email với OTP
POST /api/Auth/login             - Đăng nhập
POST /api/Auth/google-login      - Đăng nhập bằng Google
POST /api/Auth/forgot-password   - Quên mật khẩu
POST /api/Auth/send-otp          - Gửi OTP reset password
POST /api/Auth/reset-password-with-otp - Reset password với OTP
GET  /api/Auth/me                - Lấy thông tin user hiện tại
GET  /api/Auth/admin-check       - Kiểm tra quyền Admin
POST /api/Auth/logout            - Đăng xuất
```

### 1.2. Frontend Routes & API Calls
```typescript
// Routes
/login                    - PageLogin
/signup                   - PageSignUp
/forgot-pass              - PageForgotPassword

// API Calls (src/api/auth.ts)
authAPI.login()
authAPI.register()
authAPI.verifyEmail()
authAPI.googleLogin()
authAPI.forgotPassword()
authAPI.resetPassword()
authAPI.getMe()
```

### 1.3. Luồng Chi Tiết

#### Luồng Đăng Ký
```
1. User điền form đăng ký (email, password, fullName, phone)
2. Frontend gọi POST /api/Auth/register
3. Backend:
   - Validate input
   - Hash password (BCrypt)
   - Tạo User với Status = "Pending"
   - Generate OTP và lưu vào PasswordResetToken
   - Gửi OTP qua email (MockEmailService trong test)
   - Trả về 201 Created
4. Frontend hiển thị thông báo "Kiểm tra email để lấy OTP"
5. User nhập OTP
6. Frontend gọi POST /api/Auth/verify-email
7. Backend:
   - Validate OTP
   - Cập nhật Status = "Active"
   - Trả về 200 OK
8. User có thể đăng nhập
```

#### Luồng Đăng Nhập
```
1. User điền email/password
2. Frontend gọi POST /api/Auth/login
3. Backend:
   - Validate credentials
   - Kiểm tra Status = "Active"
   - Generate JWT token (chứa UserId, Email, Role)
   - Trả về token + user info
4. Frontend:
   - Lưu token vào localStorage
   - Lưu user info vào AuthContext
   - Redirect theo role (Admin → /admin, Host → /host-dashboard, Tenant → /)
```

#### Luồng Quên Mật Khẩu
```
1. User nhập email
2. Frontend gọi POST /api/Auth/send-otp
3. Backend gửi OTP qua email
4. User nhập OTP + password mới
5. Frontend gọi POST /api/Auth/reset-password-with-otp
6. Backend:
   - Validate OTP
   - Hash password mới
   - Cập nhật PasswordHash
   - Trả về 200 OK
```

---

## 2. LUỒNG TENANT BOOKING

### 2.1. Backend Endpoints
```
GET  /api/Tenant/condotels              - Xem danh sách condotel (public)
GET  /api/Tenant/condotels/{id}         - Xem chi tiết condotel
GET  /api/Booking/check-availability    - Kiểm tra availability
POST /api/Booking                       - Tạo booking
GET  /api/Booking/my                    - Xem bookings của mình
GET  /api/Booking/{id}                  - Xem chi tiết booking
POST /api/Booking/{id}/refund           - Yêu cầu refund
DELETE /api/Booking/{id}                - Hủy booking
```

### 2.2. Frontend Routes & API Calls
```typescript
// Routes
/listing-stay                    - ListingStayPage (danh sách)
/listing-stay-detail/:id         - ListingStayDetailPage
/checkout                        - CheckOutPage
/my-bookings                     - PageTenantBookings
/booking-history                 - PageBookingHistory
/booking-history/:id             - PageBookingHistoryDetail
/request-refund/:id              - PageRequestRefund

// API Calls
condotelAPI.getCondotels()
condotelAPI.getCondotelDetail()
bookingAPI.checkAvailability()
bookingAPI.createBooking()
bookingAPI.getMyBookings()
bookingAPI.cancelBooking()
bookingAPI.requestRefund()
```

### 2.3. Luồng Chi Tiết

#### Luồng Tìm Kiếm & Xem Condotel
```
1. Tenant truy cập /listing-stay
2. Frontend gọi GET /api/Tenant/condotels?name=...&minPrice=...&maxPrice=...&beds=...
3. Backend:
   - Filter condotel theo criteria
   - Chỉ trả về Status = "Active"
   - Trả về danh sách CondotelDTO
4. Frontend hiển thị danh sách với pagination
5. Tenant click vào condotel
6. Frontend gọi GET /api/Tenant/condotels/{id}
7. Backend trả về CondotelDetailDTO (gồm images, amenities, reviews)
8. Frontend hiển thị chi tiết
```

#### Luồng Đặt Phòng
```
1. Tenant chọn ngày check-in/check-out
2. Frontend gọi GET /api/Booking/check-availability?condotelId=1&checkIn=...&checkOut=...
3. Backend:
   - Kiểm tra không có booking nào trùng ngày
   - Trả về { available: true/false }
4. Nếu available:
   - Tenant click "Đặt phòng"
   - Frontend redirect đến /checkout
   - Frontend gọi POST /api/Booking với BookingDTO
5. Backend:
   - Validate dates (không quá khứ, checkOut > checkIn)
   - Kiểm tra availability lại
   - Tính TotalPrice = PricePerNight * số đêm
   - Tạo Booking với Status = "Pending"
   - Trả về BookingDTO
6. Frontend hiển thị thông tin booking và nút "Thanh toán"
```

#### Luồng Thanh Toán
```
1. Tenant click "Thanh toán"
2. Frontend gọi POST /api/Payment/payos/create với { bookingId }
3. Backend:
   - Lấy booking details
   - Gọi PayOS API tạo payment link
   - Trả về paymentUrl
4. Frontend redirect đến paymentUrl (PayOS)
5. User thanh toán trên PayOS
6. PayOS gọi webhook POST /api/Payment/payos/callback
7. Backend:
   - Validate signature
   - Cập nhật Booking Status = "Confirmed"
   - Gửi email xác nhận
8. PayOS redirect về /payment/success hoặc /payment/cancel
```

#### Luồng Hủy & Refund
```
1. Tenant vào /my-bookings
2. Click "Hủy" hoặc "Yêu cầu hoàn tiền"
3. Frontend gọi POST /api/Booking/{id}/refund hoặc DELETE /api/Booking/{id}
4. Backend:
   - Kiểm tra quyền sở hữu
   - Tạo RefundRequest (nếu refund)
   - Cập nhật Booking Status = "Cancelled"
5. Admin xử lý refund:
   - Xem danh sách refund requests tại /admin/refunds
   - Duyệt/từ chối
   - Nếu duyệt: Gọi PayOS refund API
```

---

## 3. LUỒNG HOST REGISTRATION & CONDOEL MANAGEMENT

### 3.1. Backend Endpoints
```
POST /api/Host/register-as-host        - Đăng ký làm Host
POST /api/Host/verify-id-card          - Xác minh CCCD (OCR)
POST /api/Host/condotel                - Tạo condotel
GET  /api/Host/condotel                - Xem danh sách condotel của mình
GET  /api/Host/condotel/{id}           - Xem chi tiết condotel
PUT  /api/Host/condotel/{id}           - Cập nhật condotel
DELETE /api/Host/condotel/{id}         - Xóa condotel (soft delete)
GET  /api/Host/dashboard               - Dashboard của Host
```

### 3.2. Frontend Routes & API Calls
```typescript
// Routes
/become-a-host                  - BecomeAHostPage
/host-dashboard                 - HostCondotelDashboard
/add-condotel                  - PageAddListingSimple
/edit-condotel/:id             - PageEditCondotel

// API Calls
hostAPI.registerAsHost()
hostAPI.verifyIdCard()
hostAPI.createCondotel()
hostAPI.getMyCondotels()
hostAPI.updateCondotel()
hostAPI.deleteCondotel()
hostAPI.getDashboard()
```

### 3.3. Luồng Chi Tiết

#### Luồng Đăng Ký Host
```
1. User đã đăng nhập (Tenant role)
2. Truy cập /become-a-host
3. Điền thông tin công ty (CompanyName, TaxCode, Address)
4. Frontend gọi POST /api/Host/register-as-host
5. Backend:
   - Tạo Host record với Status = "Pending"
   - Cập nhật User Role = "Host" (hoặc thêm Host role)
6. Upload ảnh CCCD mặt trước/sau
7. Frontend gọi POST /api/Host/verify-id-card
8. Backend:
   - Upload ảnh lên Cloudinary
   - Gọi DeepSeek OCR API để trích xuất thông tin
   - So sánh với thông tin User
   - Cập nhật Host Status = "Verified"
```

#### Luồng Tạo Condotel
```
1. Host đăng nhập
2. Kiểm tra package: GET /api/Host/service-package/my
3. Nếu có package active và chưa đạt giới hạn:
   - Truy cập /add-condotel
   - Điền form:
     * Name, Description
     * Chọn Resort và Location
     * PricePerNight, Beds, Bathrooms
     * Upload images
     * Chọn Amenities và Utilities
4. Frontend gọi POST /api/Host/condotel
5. Backend:
   - Validate input
   - Kiểm tra package limit
   - Upload images lên Cloudinary
   - Tạo Condotel với Status = "Active"
   - Tạo CondotelImage, CondotelAmenity, CondotelUtility
   - Trả về CondotelDTO
6. Frontend redirect đến /host-dashboard
```

---

## 4. LUỒNG PAYMENT

### 4.1. Backend Endpoints
```
POST /api/Payment/payos/create         - Tạo payment link cho booking
POST /api/Payment/payos/callback       - Webhook từ PayOS
POST /api/Payment/payos/create-package - Tạo payment link cho package
```

### 4.2. Frontend Routes & API Calls
```typescript
// Routes
/checkout                        - CheckOutPage
/payment/success                 - PaymentSuccess
/payment/cancel                  - PaymentCancelPage

// API Calls
paymentAPI.createPaymentLink()
paymentAPI.handleCallback()
```

### 4.3. Luồng Chi Tiết

#### Luồng Thanh Toán Booking
```
1. Tenant tạo booking (Status = "Pending")
2. Frontend gọi POST /api/Payment/payos/create với { bookingId }
3. Backend:
   - Lấy booking details
   - Tính OrderCode = BookingId
   - Gọi PayOS API:
     POST https://api-merchant.payos.vn/v2/payment-requests
     {
       orderCode: bookingId,
       amount: booking.TotalPrice,
       description: "Thanh toán đặt phòng",
       returnUrl: "https://domain.com/payment/success",
       cancelUrl: "https://domain.com/payment/cancel"
     }
   - Trả về paymentUrl
4. Frontend redirect đến paymentUrl
5. User thanh toán trên PayOS
6. PayOS gọi webhook:
   POST /api/Payment/payos/callback
   {
     code: "00",
     desc: "success",
     data: { orderCode, transactionDateTime, ... }
   }
7. Backend:
   - Validate signature (HMAC SHA256)
   - Tìm booking theo orderCode
   - Cập nhật Booking Status = "Confirmed"
   - Gửi email xác nhận
8. PayOS redirect về returnUrl hoặc cancelUrl
```

---

## 5. LUỒNG REVIEW

### 5.1. Backend Endpoints
```
POST /api/Tenant/reviews                - Tenant tạo review
GET  /api/Tenant/reviews                - Xem reviews của mình
PUT  /api/Host/review/{id}/reply        - Host reply review
POST /api/Host/review/{id}/report      - Host report review
GET  /api/Admin/review/reports          - Admin xem reported reviews
```

### 5.2. Frontend Routes & API Calls
```typescript
// Routes
/write-review/:id               - PageWriteReview
/my-reviews                     - PageMyReviews

// API Calls
reviewAPI.createReview()
reviewAPI.getMyReviews()
reviewAPI.replyReview()
```

### 5.3. Luồng Chi Tiết

#### Luồng Tạo Review
```
1. Booking đã Completed
2. Tenant vào /write-review/:bookingId
3. Điền rating (1-5) và comment
4. Frontend gọi POST /api/Tenant/reviews
5. Backend:
   - Kiểm tra booking đã Completed
   - Kiểm tra chưa có review cho booking này
   - Tạo Review
   - Cập nhật average rating của Condotel
   - Trả về ReviewDTO
6. Frontend hiển thị thông báo thành công
```

#### Luồng Host Reply Review
```
1. Host xem reviews tại dashboard
2. Click "Trả lời" cho review
3. Frontend gọi PUT /api/Host/review/{id}/reply với { reply }
4. Backend:
   - Kiểm tra Host sở hữu condotel
   - Cập nhật Review.Reply
   - Trả về ReviewDTO
```

---

## 6. LUỒNG PACKAGE PURCHASE

### 6.1. Backend Endpoints
```
GET  /api/Host/service-package/available - Xem packages available
GET  /api/Host/service-package/my        - Xem package hiện tại
POST /api/Package/purchase               - Mua package
POST /api/Payment/payos/create-package   - Tạo payment link cho package
```

### 6.2. Frontend Routes & API Calls
```typescript
// Routes
/pricing                        - PricingPage
/subscription                    - PageSubcription
/payment/success                 - PaymentSuccess

// API Calls
packageAPI.getAvailablePackages()
packageAPI.getMyPackage()
packageAPI.purchasePackage()
paymentAPI.createPackagePaymentLink()
```

### 6.3. Luồng Chi Tiết

#### Luồng Mua Package
```
1. Host truy cập /pricing
2. Frontend gọi GET /api/Host/service-package/available
3. Backend trả về danh sách ServicePackage (Basic, Premium, Enterprise)
4. Host chọn package
5. Frontend gọi POST /api/Package/purchase với { packageId }
6. Backend:
   - Tạo HostPackage với Status = "PendingPayment"
   - Tính OrderCode = HostId * 1_000_000_000 + PackageId * 1_000_000
7. Frontend gọi POST /api/Payment/payos/create-package
8. Backend tạo payment link (tương tự booking payment)
9. User thanh toán
10. PayOS callback:
    - Cập nhật HostPackage Status = "Active"
    - Set StartDate = DateTime.Now
    - Set EndDate = StartDate + Package.Duration
    - Host có thể đăng condotel theo giới hạn package
```

---

## 7. LUỒNG WALLET & PAYOUT

### 7.1. Backend Endpoints
```
POST /api/Host/wallet                 - Tạo wallet (bank account)
GET  /api/Host/wallet                  - Xem wallets
PUT  /api/Host/wallet/{id}             - Cập nhật wallet
DELETE /api/Host/wallet/{id}           - Xóa wallet
POST /api/Admin/payout/process/{id}    - Admin xử lý payout
GET  /api/Admin/payout/pending         - Xem pending payouts
```

### 7.2. Frontend Routes & API Calls
```typescript
// Routes
/account-billing               - AccountBilling
/admin/payouts                 - PageAdminPayout

// API Calls
walletAPI.createWallet()
walletAPI.getWallets()
walletAPI.updateWallet()
payoutAPI.processPayout()
```

### 7.3. Luồng Chi Tiết

#### Luồng Tạo Wallet
```
1. Host vào /account-billing
2. Click "Thêm tài khoản ngân hàng"
3. Điền BankName, AccountNumber, AccountHolderName
4. Frontend gọi POST /api/Host/wallet
5. Backend:
   - Tạo Wallet
   - Nếu là wallet đầu tiên: Set IsDefault = true
   - Trả về WalletDTO
```

#### Luồng Payout
```
1. Booking đã Completed >= 15 ngày
2. Admin vào /admin/payouts
3. Xem danh sách pending payouts:
   GET /api/Admin/payout/pending
4. Admin click "Xử lý payout" cho booking
5. Frontend gọi POST /api/Admin/payout/process/{bookingId}
6. Backend:
   - Kiểm tra booking đã Completed
   - Kiểm tra không có RefundRequest
   - Lấy wallet mặc định của Host
   - Đánh dấu Booking.IsPaidToHost = true
   - Gửi email thông báo cho Host
   - Trả về 200 OK
```

---

## 8. LUỒNG ADMIN MANAGEMENT

### 8.1. Backend Endpoints
```
GET  /api/Admin/users                  - Xem danh sách users
GET  /api/Admin/users/{id}             - Xem chi tiết user
PUT  /api/Admin/users/{id}/status      - Cập nhật status user
GET  /api/Admin/location                - Quản lý locations
POST /api/Admin/location                - Tạo location
GET  /api/Admin/resort                  - Quản lý resorts
GET  /api/Admin/dashboard/overview      - Dashboard overview
GET  /api/Admin/dashboard/revenue/chart - Revenue chart
GET  /api/Admin/refunds                 - Xem refund requests
PUT  /api/Admin/refunds/{id}/approve    - Duyệt refund
PUT  /api/Admin/refunds/{id}/reject     - Từ chối refund
```

### 8.2. Frontend Routes & API Calls
```typescript
// Routes
/admin/*                    - AdminPage (nested routes)
/admin/refunds              - PageAdminRefund
/admin/payouts              - PageAdminPayout

// API Calls
adminAPI.getUsers()
adminAPI.updateUserStatus()
adminAPI.getDashboard()
adminAPI.getRefunds()
adminAPI.approveRefund()
```

### 8.3. Luồng Chi Tiết

#### Luồng Quản Lý Users
```
1. Admin đăng nhập
2. Truy cập /admin/users
3. Frontend gọi GET /api/Admin/users
4. Backend trả về danh sách users với pagination
5. Admin có thể:
   - Xem chi tiết user
   - Cập nhật status (Active/Inactive/Banned)
   - Xem bookings của user
```

#### Luồng Dashboard
```
1. Admin vào /admin
2. Frontend gọi GET /api/Admin/dashboard/overview
3. Backend trả về:
   {
     totalUsers: number,
     totalBookings: number,
     totalCondotels: number,
     totalRevenue: number,
     recentBookings: [...],
     topCondotels: [...]
   }
4. Frontend hiển thị charts và statistics
```

---

## 9. LUỒNG VOUCHER & PROMOTION

### 9.1. Backend Endpoints
```
POST /api/Host/voucher                 - Host tạo voucher
GET  /api/Host/voucher                  - Xem vouchers của host
PUT  /api/Host/voucher/{id}             - Cập nhật voucher
DELETE /api/Host/voucher/{id}           - Xóa voucher
GET  /api/Vouchers/condotel/{id}        - Xem vouchers của condotel (public)
POST /api/Host/promotion                - Host tạo promotion
GET  /api/promotions                    - Xem promotions (public)
GET  /api/promotions/condotel/{id}      - Xem promotions của condotel
```

### 9.2. Frontend Routes & API Calls
```typescript
// Routes
/manage-vouchers               - PageVoucherList
/manage-vouchers/add           - PageVoucherAdd
/my-vouchers                   - PageMyVouchers

// API Calls
voucherAPI.createVoucher()
voucherAPI.getMyVouchers()
voucherAPI.getCondotelVouchers()
promotionAPI.getPromotions()
```

### 9.3. Luồng Chi Tiết

#### Luồng Tạo Voucher
```
1. Host vào /manage-vouchers
2. Click "Thêm voucher"
3. Điền form:
   - Code (unique)
   - CondotelId
   - DiscountPercentage
   - MaxUses
   - ExpiryDate
4. Frontend gọi POST /api/Host/voucher
5. Backend:
   - Validate code không trùng
   - Tạo Voucher
   - Trả về VoucherDTO
```

#### Luồng Sử Dụng Voucher Khi Booking
```
1. Tenant xem condotel detail
2. Frontend gọi GET /api/Vouchers/condotel/{id}
3. Backend trả về danh sách vouchers available
4. Tenant chọn voucher
5. Khi tạo booking:
   - Frontend gửi voucherCode trong BookingDTO
6. Backend:
   - Validate voucher:
     * Code tồn tại
     * Chưa hết hạn
     * Chưa đạt MaxUses
     * Thuộc condotel đang đặt
   - Tính TotalPrice với discount
   - Tăng Voucher.UsedCount
```

---

## 10. LUỒNG CHAT (SignalR)

### 10.1. Backend
```
Hub: /hubs/chat
Methods:
- SendMessage(conversationId, message)
- JoinConversation(conversationId)
- GetConversations()
```

### 10.2. Frontend Routes & API Calls
```typescript
// Routes
/chat                        - PageChat

// API Calls (REST)
chatAPI.getConversations()
chatAPI.getMessages()
chatAPI.createConversation()

// SignalR
useChat hook:
- connect()
- sendMessage()
- onMessage()
```

### 10.3. Luồng Chi Tiết

#### Luồng Chat
```
1. Tenant xem condotel detail
2. Click "Liên hệ Host"
3. Frontend:
   - Tạo conversation (nếu chưa có)
   - Kết nối SignalR hub
4. Tenant gửi message
5. SignalR gửi message đến Host
6. Host nhận message real-time
7. Host reply
8. Messages được lưu vào database (ChatMessage)
```

---

## CÁC ĐIỂM QUAN TRỌNG

### Security
- JWT Authentication cho tất cả protected endpoints
- Role-based Authorization (Admin, Host, Tenant)
- Ownership checks (chỉ owner mới được sửa/xóa)
- Input validation (FluentValidation hoặc Data Annotations)
- CORS chỉ cho phép frontend origin

### Business Rules
- Booking availability check (không trùng ngày)
- Package limits (số lượng condotel được phép đăng)
- Payout rules (15 ngày sau khi completed)
- Review rules (chỉ review sau khi completed)
- Voucher validation (expiry, max uses, condotel match)

### Integration Points
- **PayOS**: Payment gateway (create payment, callback, refund)
- **Email Service**: OTP, notifications
- **Cloudinary**: Image upload/delete
- **DeepSeek OCR**: ID Card verification

### Error Handling
- Backend trả về structured errors:
  ```json
  {
    "success": false,
    "message": "Error message",
    "errors": { "field": ["error1", "error2"] }
  }
  ```
- Frontend hiển thị errors bằng react-toastify

---

## MAPPING FRONTEND ROUTES → BACKEND ENDPOINTS

| Frontend Route | Component | Backend Endpoint |
|---------------|-----------|-----------------|
| `/login` | PageLogin | `POST /api/Auth/login` |
| `/signup` | PageSignUp | `POST /api/Auth/register` |
| `/listing-stay` | ListingStayPage | `GET /api/Tenant/condotels` |
| `/listing-stay-detail/:id` | ListingStayDetailPage | `GET /api/Tenant/condotels/{id}` |
| `/checkout` | CheckOutPage | `POST /api/Booking` |
| `/my-bookings` | PageTenantBookings | `GET /api/Booking/my` |
| `/host-dashboard` | HostCondotelDashboard | `GET /api/Host/dashboard` |
| `/add-condotel` | PageAddListingSimple | `POST /api/Host/condotel` |
| `/admin/*` | AdminPage | `GET /api/Admin/*` |
| `/chat` | PageChat | SignalR `/hubs/chat` |

---

## TEST COVERAGE

### Đã có System Tests:
- ✅ SYS-001: Complete Tenant Booking Flow
- ✅ SYS-002: Complete Host Registration Flow
- ✅ SYS-003: Complete Booking with Payment Flow
- ✅ SYS-004: Complete Review Flow
- ✅ SYS-005: Complete Package Purchase Flow
- ✅ SYS-006: Complete Wallet and Payout Flow
- ✅ SYS-007: Complete Admin Management Flow
- ✅ SYS-008: Authorization and Security Flow
- ✅ SYS-009: Complete Search and Filter Flow
- ✅ SYS-010: Complete Voucher Flow

### Cần bổ sung:
- ⏳ SYS-011: Complete Authentication Flow (Register → Verify → Login → Forgot Password)
- ⏳ SYS-012: Complete Refund Request Flow (Tenant request → Admin approve/reject)
- ⏳ SYS-013: Complete Chat Flow (SignalR)
- ⏳ SYS-014: Complete Promotion Flow (Host create → Tenant use)
- ⏳ SYS-015: Complete Package Limit Enforcement Flow





