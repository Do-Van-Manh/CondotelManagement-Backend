# Test Cases - Luồng Nghiệp Vụ Condotel Management

## 1. AUTHENTICATION FLOW

### TC-AUTH-001: Register New User
- **Test Scenario:** User đăng ký tài khoản mới
- **Precondition:** Email chưa tồn tại trong hệ thống
- **Test Steps:**
  1. POST /api/auth/register với email, password, fullName, phone
  2. Verify response status 201
  3. Verify user được tạo với Status = "Pending"
  4. Verify OTP được gửi qua email
- **Expected Result:** User được tạo thành công, Status = "Pending", OTP được gửi

### TC-AUTH-002: Verify Email với OTP
- **Test Scenario:** User xác thực email bằng OTP
- **Precondition:** User đã register và có OTP hợp lệ
- **Test Steps:**
  1. POST /api/auth/verify-email với email và OTP
  2. Verify response status 200
  3. Verify user Status = "Active"
  4. Verify OTP bị xóa
- **Expected Result:** User được kích hoạt, Status = "Active"

### TC-AUTH-003: Login với Credentials hợp lệ
- **Test Scenario:** User đăng nhập với email và password đúng
- **Precondition:** User đã verify email (Status = "Active")
- **Test Steps:**
  1. POST /api/auth/login với email và password
  2. Verify response status 200
  3. Verify response có Token và User info
- **Expected Result:** Nhận được JWT token và thông tin user

### TC-AUTH-004: Login với Credentials không hợp lệ
- **Test Scenario:** User đăng nhập với password sai
- **Precondition:** User tồn tại và đã active
- **Test Steps:**
  1. POST /api/auth/login với email đúng, password sai
  2. Verify response status 401
- **Expected Result:** Trả về 401 Unauthorized

### TC-AUTH-005: Forgot Password Flow
- **Test Scenario:** User quên mật khẩu và reset
- **Precondition:** User tồn tại và active
- **Test Steps:**
  1. POST /api/auth/send-otp với email
  2. Verify OTP được gửi
  3. POST /api/auth/reset-password-with-otp với email, OTP, newPassword
  4. Verify password được đổi
  5. Login với password mới
- **Expected Result:** Password được reset thành công, có thể login với password mới

## 2. TENANT FLOW - XEM VÀ TÌM KIẾM CONDOTEL

### TC-TENANT-001: Xem danh sách Condotel (không filter)
- **Test Scenario:** Tenant xem tất cả condotel
- **Precondition:** Có ít nhất 1 condotel trong hệ thống
- **Test Steps:**
  1. GET /api/tenant/condotels
  2. Verify response status 200
  3. Verify response có danh sách condotel
- **Expected Result:** Trả về danh sách condotel

### TC-TENANT-002: Tìm kiếm Condotel theo tên
- **Test Scenario:** Tenant tìm kiếm condotel theo tên
- **Precondition:** Có condotel với tên "Test Condotel"
- **Test Steps:**
  1. GET /api/tenant/condotels?name=Test
  2. Verify response status 200
  3. Verify tất cả condotel trong response có tên chứa "Test"
- **Expected Result:** Trả về condotel có tên chứa "Test"

### TC-TENANT-003: Filter Condotel theo giá
- **Test Scenario:** Tenant filter condotel theo khoảng giá
- **Precondition:** Có condotel với giá khác nhau
- **Test Steps:**
  1. GET /api/tenant/condotels?minPrice=50000&maxPrice=150000
  2. Verify response status 200
  3. Verify tất cả condotel có PricePerNight trong khoảng 50000-150000
- **Expected Result:** Trả về condotel trong khoảng giá

### TC-TENANT-004: Filter Condotel theo số giường/phòng tắm
- **Test Scenario:** Tenant filter theo beds và bathrooms
- **Precondition:** Có condotel với beds=2, bathrooms=1
- **Test Steps:**
  1. GET /api/tenant/condotels?beds=2&bathrooms=1
  2. Verify response status 200
  3. Verify tất cả condotel có beds=2 và bathrooms=1
- **Expected Result:** Trả về condotel đúng tiêu chí

### TC-TENANT-005: Xem chi tiết Condotel
- **Test Scenario:** Tenant xem chi tiết một condotel
- **Precondition:** Condotel ID = 1 tồn tại
- **Test Steps:**
  1. GET /api/tenant/condotels/1
  2. Verify response status 200
  3. Verify response có đầy đủ thông tin: images, prices, amenities, utilities
- **Expected Result:** Trả về chi tiết condotel đầy đủ

## 3. TENANT FLOW - BOOKING

### TC-BOOKING-001: Check Availability - Condotel trống
- **Test Scenario:** Tenant kiểm tra condotel có trống không
- **Precondition:** Condotel ID = 1 không có booking trong khoảng thời gian
- **Test Steps:**
  1. GET /api/booking/check-availability?condotelId=1&checkIn=2025-12-01&checkOut=2025-12-05
  2. Verify response status 200
  3. Verify available = true
- **Expected Result:** Condotel available trong khoảng thời gian

### TC-BOOKING-002: Check Availability - Condotel đã được đặt
- **Test Scenario:** Tenant kiểm tra condotel đã có booking
- **Precondition:** Condotel ID = 1 đã có booking từ 2025-12-01 đến 2025-12-05
- **Test Steps:**
  1. GET /api/booking/check-availability?condotelId=1&checkIn=2025-12-03&checkOut=2025-12-07
  2. Verify response status 200
  3. Verify available = false
- **Expected Result:** Condotel không available (overlap)

### TC-BOOKING-003: Tạo Booking hợp lệ
- **Test Scenario:** Tenant tạo booking mới
- **Precondition:** 
  - User đã login (Tenant role)
  - Condotel ID = 1 available trong khoảng thời gian
- **Test Steps:**
  1. POST /api/booking với condotelId, startDate, endDate
  2. Verify response status 201
  3. Verify booking được tạo với Status = "Pending"
  4. Verify TotalPrice được tính đúng (PricePerNight * số đêm)
- **Expected Result:** Booking được tạo thành công

### TC-BOOKING-004: Tạo Booking với ngày quá khứ
- **Test Scenario:** Tenant cố tạo booking với ngày trong quá khứ
- **Precondition:** User đã login
- **Test Steps:**
  1. POST /api/booking với startDate = yesterday
  2. Verify response status 400
  3. Verify message lỗi
- **Expected Result:** Trả về lỗi "Start date cannot be in the past"

### TC-BOOKING-005: Tạo Booking với ngày trùng lặp
- **Test Scenario:** Tenant tạo booking với ngày đã có booking khác
- **Precondition:** 
  - User đã login
  - Condotel đã có booking trong khoảng thời gian
- **Test Steps:**
  1. POST /api/booking với ngày overlap với booking hiện có
  2. Verify response status 400
  3. Verify message "not available"
- **Expected Result:** Trả về lỗi không available

### TC-BOOKING-006: Tạo Booking với Promotion
- **Test Scenario:** Tenant tạo booking và áp dụng promotion
- **Precondition:**
  - User đã login
  - Condotel có promotion 20% discount
- **Test Steps:**
  1. POST /api/booking với promotionId
  2. Verify response status 201
  3. Verify TotalPrice = basePrice * 0.8 (20% discount)
- **Expected Result:** Booking được tạo với giá đã giảm

### TC-BOOKING-007: Tạo Booking với Voucher
- **Test Scenario:** Tenant tạo booking và áp dụng voucher
- **Precondition:**
  - User đã login
  - Có voucher hợp lệ
- **Test Steps:**
  1. POST /api/booking với voucherId
  2. Verify response status 201
  3. Verify TotalPrice đã trừ discount
- **Expected Result:** Booking được tạo với voucher discount

### TC-BOOKING-008: Xem danh sách Bookings của mình
- **Test Scenario:** Tenant xem tất cả bookings của mình
- **Precondition:** User đã login và có ít nhất 1 booking
- **Test Steps:**
  1. GET /api/booking/my
  2. Verify response status 200
  3. Verify response chỉ có bookings của user hiện tại
  4. Verify có thông tin CanReview và HasReviewed
- **Expected Result:** Trả về danh sách bookings của user

### TC-BOOKING-009: Xem chi tiết Booking
- **Test Scenario:** Tenant xem chi tiết một booking
- **Precondition:** User đã login và có booking ID = 1
- **Test Steps:**
  1. GET /api/booking/1
  2. Verify response status 200
  3. Verify response có đầy đủ thông tin booking
- **Expected Result:** Trả về chi tiết booking

### TC-BOOKING-010: Cancel Booking
- **Test Scenario:** Tenant hủy booking của mình
- **Precondition:** User đã login và có booking với Status = "Pending" hoặc "Confirmed"
- **Test Steps:**
  1. DELETE /api/booking/{bookingId}
  2. Verify response status 200/204
  3. Verify booking Status = "Cancelled"
- **Expected Result:** Booking được cancel thành công

### TC-BOOKING-011: Cancel Booking của user khác
- **Test Scenario:** Tenant cố cancel booking không phải của mình
- **Precondition:** User đã login, booking ID = 1 thuộc user khác
- **Test Steps:**
  1. DELETE /api/booking/1
  2. Verify response status 403/404
- **Expected Result:** Trả về Forbidden hoặc NotFound

## 4. PAYMENT FLOW

### TC-PAYMENT-001: Tạo Payment Link
- **Test Scenario:** Tenant tạo payment link cho booking
- **Precondition:**
  - User đã login (Tenant)
  - Có booking với Status = "Pending"
  - Booking.TotalPrice >= 10000 VND
- **Test Steps:**
  1. POST /api/payment/payos/create với bookingId
  2. Verify response status 200
  3. Verify response có checkoutUrl, paymentLinkId, qrCode
- **Expected Result:** Payment link được tạo thành công

### TC-PAYMENT-002: Tạo Payment với Booking không tồn tại
- **Test Scenario:** Tenant tạo payment với bookingId không hợp lệ
- **Precondition:** User đã login
- **Test Steps:**
  1. POST /api/payment/payos/create với bookingId = 99999
  2. Verify response status 404
- **Expected Result:** Trả về NotFound

### TC-PAYMENT-003: Tạo Payment với Booking không phải của mình
- **Test Scenario:** Tenant cố tạo payment cho booking của user khác
- **Precondition:** User đã login, booking ID = 1 thuộc user khác
- **Test Steps:**
  1. POST /api/payment/payos/create với bookingId = 1
  2. Verify response status 403
- **Expected Result:** Trả về Forbidden

### TC-PAYMENT-004: Tạo Payment với Booking đã thanh toán
- **Test Scenario:** Tenant tạo payment cho booking đã Confirmed
- **Precondition:** Booking Status = "Confirmed"
- **Test Steps:**
  1. POST /api/payment/payos/create
  2. Verify response status 400
  3. Verify message "not in a payable state"
- **Expected Result:** Trả về lỗi booking không thể thanh toán

### TC-PAYMENT-005: Check Payment Status
- **Test Scenario:** Tenant kiểm tra trạng thái thanh toán
- **Precondition:** Đã tạo payment link
- **Test Steps:**
  1. GET /api/payment/payos/status/{paymentLinkId}
  2. Verify response status 200
  3. Verify response có status, amount, orderCode
- **Expected Result:** Trả về trạng thái payment

## 5. REVIEW FLOW

### TC-REVIEW-001: Tạo Review cho Booking đã Completed
- **Test Scenario:** Tenant tạo review sau khi booking completed
- **Precondition:**
  - User đã login
  - Có booking với Status = "Completed", EndDate < Today
  - Chưa có review cho booking này
- **Test Steps:**
  1. POST /api/tenant/reviews với bookingId, condotelId, rating, comment
  2. Verify response status 200
  3. Verify review được tạo trong database
- **Expected Result:** Review được tạo thành công

### TC-REVIEW-002: Tạo Review cho Booking chưa Completed
- **Test Scenario:** Tenant cố tạo review cho booking pending
- **Precondition:** Booking Status = "Pending"
- **Test Steps:**
  1. POST /api/tenant/reviews
  2. Verify response status 400
- **Expected Result:** Trả về lỗi "Booking not completed"

### TC-REVIEW-003: Tạo Review trùng lặp
- **Test Scenario:** Tenant cố tạo review lần 2 cho cùng booking
- **Precondition:** Đã có review cho booking này
- **Test Steps:**
  1. POST /api/tenant/reviews với bookingId đã có review
  2. Verify response status 400
- **Expected Result:** Trả về lỗi "already reviewed"

### TC-REVIEW-004: Xem danh sách Reviews của mình
- **Test Scenario:** Tenant xem tất cả reviews mình đã tạo
- **Precondition:** User đã login và có ít nhất 1 review
- **Test Steps:**
  1. GET /api/tenant/reviews
  2. Verify response status 200
  3. Verify response chỉ có reviews của user hiện tại
- **Expected Result:** Trả về danh sách reviews của user

### TC-REVIEW-005: Update Review (trong 7 ngày)
- **Test Scenario:** Tenant cập nhật review trong vòng 7 ngày
- **Precondition:** 
  - User đã login
  - Có review được tạo < 7 ngày trước
- **Test Steps:**
  1. PUT /api/tenant/reviews/{reviewId} với rating và comment mới
  2. Verify response status 200
  3. Verify review được update
- **Expected Result:** Review được update thành công

### TC-REVIEW-006: Update Review sau 7 ngày
- **Test Scenario:** Tenant cố update review sau 7 ngày
- **Precondition:** Review được tạo > 7 ngày trước
- **Test Steps:**
  1. PUT /api/tenant/reviews/{reviewId}
  2. Verify response status 400
- **Expected Result:** Trả về lỗi "cannot update after 7 days"

### TC-REVIEW-007: Host Reply Review
- **Test Scenario:** Host trả lời review của tenant
- **Precondition:**
  - Host đã login
  - Có review cho condotel của host
- **Test Steps:**
  1. POST /api/host/review/reply với reviewId và reply text
  2. Verify response status 200
  3. Verify review.Reply được update
- **Expected Result:** Reply được thêm vào review

## 6. HOST FLOW - QUẢN LÝ CONDOTEL

### TC-HOST-001: Tạo Condotel
- **Test Scenario:** Host tạo condotel mới
- **Precondition:** Host đã login
- **Test Steps:**
  1. POST /api/host/condotel với thông tin condotel
  2. Verify response status 201
  3. Verify condotel được tạo với HostId = current host
- **Expected Result:** Condotel được tạo thành công

### TC-HOST-002: Update Condotel
- **Test Scenario:** Host cập nhật thông tin condotel
- **Precondition:** Host đã login, có condotel ID = 1
- **Test Steps:**
  1. PUT /api/host/condotel/1 với thông tin mới
  2. Verify response status 200
  3. Verify condotel được update
- **Expected Result:** Condotel được update thành công

### TC-HOST-003: Delete Condotel
- **Test Scenario:** Host xóa condotel
- **Precondition:** Host đã login, có condotel ID = 1
- **Test Steps:**
  1. DELETE /api/host/condotel/1
  2. Verify response status 200
  3. Verify condotel bị xóa hoặc Status = "Inactive"
- **Expected Result:** Condotel được xóa thành công

### TC-HOST-004: Xem danh sách Condotel của mình
- **Test Scenario:** Host xem tất cả condotel của mình
- **Precondition:** Host đã login, có ít nhất 1 condotel
- **Test Steps:**
  1. GET /api/host/condotel
  2. Verify response status 200
  3. Verify response chỉ có condotel của host hiện tại
- **Expected Result:** Trả về danh sách condotel của host

## 7. HOST FLOW - QUẢN LÝ BOOKING

### TC-HOST-005: Xem Bookings của Condotel
- **Test Scenario:** Host xem tất cả bookings của condotel
- **Precondition:** Host đã login, có condotel với bookings
- **Test Steps:**
  1. GET /api/host/booking?condotelId=1
  2. Verify response status 200
  3. Verify response có bookings của condotel
- **Expected Result:** Trả về danh sách bookings

### TC-HOST-006: Update Booking Status
- **Test Scenario:** Host cập nhật status booking
- **Precondition:** Host đã login, có booking của condotel mình
- **Test Steps:**
  1. PUT /api/host/booking/{bookingId} với status mới
  2. Verify response status 200
  3. Verify booking status được update
- **Expected Result:** Booking status được update

## 8. VOUCHER FLOW

### TC-VOUCHER-001: Host tạo Voucher
- **Test Scenario:** Host tạo voucher cho condotel
- **Precondition:** Host đã login
- **Test Steps:**
  1. POST /api/host/voucher với code, discountPercentage, expiryDate
  2. Verify response status 200
  3. Verify voucher được tạo
- **Expected Result:** Voucher được tạo thành công

### TC-VOUCHER-002: Tạo Voucher với Code trùng
- **Test Scenario:** Host cố tạo voucher với code đã tồn tại
- **Precondition:** Đã có voucher với code "TEST2024"
- **Test Steps:**
  1. POST /api/host/voucher với code = "TEST2024"
  2. Verify response status 400
- **Expected Result:** Trả về lỗi "Code already exists"

### TC-VOUCHER-003: Tenant xem Vouchers của Condotel
- **Test Scenario:** Tenant xem vouchers có thể dùng cho condotel
- **Precondition:** Condotel có vouchers
- **Test Steps:**
  1. GET /api/tenant/voucher/condotel/1
  2. Verify response status 200
  3. Verify response có vouchers hợp lệ (chưa hết hạn, chưa hết số lần dùng)
- **Expected Result:** Trả về danh sách vouchers hợp lệ

## 9. ADMIN FLOW

### TC-ADMIN-001: Xem Dashboard
- **Test Scenario:** Admin xem dashboard tổng quan
- **Precondition:** Admin đã login
- **Test Steps:**
  1. GET /api/admin/dashboard
  2. Verify response status 200
  3. Verify response có thống kê: totalUsers, totalBookings, totalRevenue
- **Expected Result:** Trả về dashboard data

### TC-ADMIN-002: Quản lý Users
- **Test Scenario:** Admin xem danh sách users
- **Precondition:** Admin đã login
- **Test Steps:**
  1. GET /api/admin/users
  2. Verify response status 200
  3. Verify response có danh sách users
- **Expected Result:** Trả về danh sách users

### TC-ADMIN-003: Update User Status
- **Test Scenario:** Admin cập nhật status user
- **Precondition:** Admin đã login, có user ID = 1
- **Test Steps:**
  1. PUT /api/admin/users/1/status với status mới
  2. Verify response status 200
  3. Verify user status được update
- **Expected Result:** User status được update

## 10. AUTHORIZATION TESTS

### TC-AUTHZ-001: Access với Role không đúng
- **Test Scenario:** Tenant cố truy cập endpoint của Host
- **Precondition:** User đã login với role Tenant
- **Test Steps:**
  1. POST /api/host/condotel
  2. Verify response status 403
- **Expected Result:** Trả về Forbidden

### TC-AUTHZ-002: Access không có Token
- **Test Scenario:** Request không có JWT token
- **Precondition:** Không có authentication
- **Test Steps:**
  1. GET /api/booking/my (không có Authorization header)
  2. Verify response status 401
- **Expected Result:** Trả về Unauthorized














