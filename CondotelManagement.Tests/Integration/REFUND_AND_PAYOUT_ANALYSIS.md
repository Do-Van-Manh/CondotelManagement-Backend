# Phân Tích Code: Refund và Payout

## 1. REFUND (Hoàn Tiền Khi Hủy Phòng)

### 1.1. Tổng Quan
Chức năng hoàn tiền cho khách hàng khi họ hủy phòng đã đặt. Hệ thống tự động tạo refund request khi booking bị hủy (nếu đã thanh toán) hoặc khách hàng có thể yêu cầu hoàn tiền thủ công.

### 1.2. Luồng Xử Lý

#### A. Tenant Hủy Booking (DELETE /api/booking/{id})
- **Controller**: `BookingController.CancelBooking()`
- **Service**: `BookingService.CancelBooking()`
- **Logic**:
  1. Kiểm tra booking thuộc về customer
  2. Nếu booking status = "Confirmed" hoặc "Completed":
     - Tự động gọi `RefundBooking()` để tạo refund request
     - Set booking status = "Cancelled"
  3. Nếu booking status = "Pending":
     - Chỉ set status = "Cancelled" (không tạo refund vì chưa thanh toán)

#### B. Tenant Yêu Cầu Hoàn Tiền (POST /api/booking/{id}/refund)
- **Controller**: `BookingController.RefundBooking()`
- **Service**: `BookingService.RefundBooking()`
- **Request Body**: `RefundBookingRequestDTO` (BankCode, AccountNumber, AccountHolder - optional)
- **Điều Kiện**:
  - Booking phải thuộc về customer
  - Booking status phải là: "Cancelled", "Confirmed", "Completed", hoặc "Refunded"
  - Phải hủy trước **ít nhất 2 ngày** so với check-in date
  - Booking chưa được refund (chưa có RefundRequest với status = "Completed" hoặc "Refunded")
- **Logic**:
  1. Kiểm tra điều kiện refund
  2. Gọi `ProcessRefund()` để xử lý:
     - Lấy thông tin customer và wallet
     - Tạo RefundRequest với status = "Pending"
     - Tạo PayOS refund payment link (nếu chưa có TransactionId)
     - Gửi email thông báo cho customer

#### C. Admin Xem Danh Sách Refund Requests (GET /api/admin/refund-requests)
- **Controller**: `AdminRefundController.GetRefundRequests()`
- **Service**: `BookingService.GetRefundRequestsAsync()`
- **Query Parameters**:
  - `searchTerm`: Tìm kiếm theo booking ID hoặc customer name
  - `status`: Filter theo status ("all", "Pending", "Completed", "Refunded", "Rejected")
  - `startDate`, `endDate`: Filter theo ngày tạo
- **Response**: Danh sách `RefundRequestDTO`

#### D. Admin Xác Nhận Hoàn Tiền (POST /api/admin/refund-requests/{id}/confirm)
- **Controller**: `AdminRefundController.ConfirmRefund()`
- **Service**: `BookingService.ConfirmRefundManually()`
- **Logic**:
  1. Tìm RefundRequest theo ID (hoặc BookingId nếu không tìm thấy)
  2. Nếu không tìm thấy và booking đã cancelled, tạo mới RefundRequest
  3. Set status = "Completed"
  4. Set ProcessedAt = DateTime.UtcNow
  5. Gửi email xác nhận hoàn tiền cho customer

### 1.3. Model: RefundRequest
```csharp
- Id: int (PK)
- BookingId: int (FK)
- CustomerId: int (FK)
- CustomerName: string
- CustomerEmail: string?
- RefundAmount: decimal
- Status: string ("Pending", "Completed", "Refunded", "Rejected")
- BankCode: string? (Mã ngân hàng)
- AccountNumber: string?
- AccountHolder: string?
- Reason: string? (Lý do hủy)
- TransactionId: string? (PayOS transaction ID)
- PaymentMethod: string? ("Auto" hoặc "Manual")
- ProcessedBy: int? (Admin user ID)
- ProcessedAt: DateTime?
- CreatedAt: DateTime
- UpdatedAt: DateTime?
```

### 1.4. Test Cases Đã Tạo
- ✅ TC-REFUND-001: Tenant hủy booking đã thanh toán → Tự động tạo refund request
- ✅ TC-REFUND-002: Tenant yêu cầu hoàn tiền với thông tin ngân hàng
- ✅ TC-REFUND-003: Tenant hủy booking < 2 ngày trước check-in → Không được refund
- ✅ TC-REFUND-004: Admin xem danh sách refund requests
- ✅ TC-REFUND-005: Admin xác nhận hoàn tiền → Update status
- ✅ TC-REFUND-006: Tenant yêu cầu refund booking chưa thanh toán → BadRequest

---

## 2. PAYOUT (Admin Thanh Toán Cho Host)

### 2.1. Tổng Quan
Chức năng admin thanh toán tiền phòng hàng tháng cho host. Hệ thống tự động tìm các booking đã completed >= 15 ngày và chưa được trả tiền, sau đó admin có thể xử lý thanh toán hàng loạt hoặc từng booking.

### 2.2. Luồng Xử Lý

#### A. Admin Xem Danh Sách Booking Chờ Thanh Toán (GET /api/admin/payouts/pending)
- **Controller**: `AdminPayoutController.GetPendingPayouts()`
- **Service**: `HostPayoutService.GetPendingPayoutsAsync()`
- **Query Parameters**: `hostId` (optional - filter theo host)
- **Điều Kiện Booking Đủ Để Thanh Toán**:
  - Status = "Completed" (hoặc "Hoàn thành")
  - EndDate <= (Today - 15 days)
  - IsPaidToHost = false hoặc null
  - TotalPrice > 0
  - **KHÔNG có** RefundRequest với status = "Pending" hoặc "Approved"
- **Response**: Danh sách `HostPayoutItemDTO` với thông tin:
  - Booking details (BookingId, CondotelId, CondotelName, etc.)
  - Host details (HostId, HostName)
  - Amount, EndDate, DaysSinceCompleted
  - Customer info
  - Bank info từ Wallet của host (để thực hiện thanh toán)

#### B. Admin Xử Lý Tất Cả Booking Đủ Điều Kiện (POST /api/admin/payouts/process-all)
- **Controller**: `AdminPayoutController.ProcessAllPayouts()`
- **Service**: `HostPayoutService.ProcessPayoutsAsync()`
- **Logic**:
  1. Tìm tất cả booking đủ điều kiện (theo điều kiện ở trên)
  2. Với mỗi booking:
     - Lấy thông tin Wallet của host (ưu tiên default, active)
     - Set `IsPaidToHost = true`
     - Set `PaidToHostAt = DateTime.UtcNow`
     - Gửi email thông báo cho host
  3. Save changes
  4. Return `HostPayoutResponseDTO` với:
     - ProcessedCount: Số booking đã xử lý
     - TotalAmount: Tổng số tiền đã trả
     - ProcessedItems: Danh sách booking đã xử lý

#### C. Admin Xác Nhận Thanh Toán Cho Một Booking (POST /api/admin/payouts/{bookingId}/confirm)
- **Controller**: `AdminPayoutController.ConfirmPayout()`
- **Service**: `HostPayoutService.ProcessPayoutForBookingAsync()`
- **Logic**:
  1. Kiểm tra booking tồn tại
  2. Kiểm tra status = "Completed"
  3. Kiểm tra chưa được trả tiền (IsPaidToHost != true)
  4. Kiểm tra đã kết thúc >= 15 ngày
  5. Kiểm tra không có refund request pending/approved
  6. Lấy thông tin Wallet của host
  7. Set `IsPaidToHost = true`, `PaidToHostAt = DateTime.UtcNow`
  8. Gửi email thông báo cho host
  9. Return `HostPayoutResponseDTO`

#### D. Admin Xem Danh Sách Booking Đã Thanh Toán (GET /api/admin/payouts/paid)
- **Controller**: `AdminPayoutController.GetPaidPayouts()`
- **Service**: `HostPayoutService.GetPaidPayoutsAsync()`
- **Query Parameters**:
  - `hostId`: Filter theo host (optional)
  - `fromDate`, `toDate`: Filter theo ngày thanh toán (optional)
- **Response**: Danh sách `HostPayoutItemDTO` với `IsPaid = true`

### 2.3. Model: Booking (Các Field Liên Quan)
```csharp
- BookingId: int (PK)
- CustomerId: int (FK)
- CondotelId: int (FK)
- StartDate: DateOnly
- EndDate: DateOnly
- TotalPrice: decimal?
- Status: string ("Pending", "Confirmed", "Completed", "Cancelled")
- IsPaidToHost: bool? (Đã trả tiền cho host chưa)
- PaidToHostAt: DateTime? (Thời điểm trả tiền)
```

### 2.4. DTO: HostPayoutItemDTO
```csharp
- BookingId: int
- CondotelId: int
- CondotelName: string
- HostId: int
- HostName: string
- Amount: decimal
- EndDate: DateOnly
- PaidAt: DateTime? (null nếu chưa trả)
- IsPaid: bool
- DaysSinceCompleted: int
- CustomerId: int
- CustomerName: string
- CustomerEmail: string?
- BankName: string? (Từ Wallet của host)
- AccountNumber: string?
- AccountHolderName: string?
```

### 2.5. Test Cases Đã Tạo
- ✅ TC-PAYOUT-001: Admin xem danh sách booking chờ thanh toán
- ✅ TC-PAYOUT-002: Admin xử lý tất cả booking đủ điều kiện → Đánh dấu đã trả tiền
- ✅ TC-PAYOUT-003: Admin xác nhận thanh toán cho một booking cụ thể
- ✅ TC-PAYOUT-004: Admin xử lý booking chưa đủ 15 ngày → BadRequest
- ✅ TC-PAYOUT-005: Admin xem danh sách booking đã thanh toán
- ✅ TC-PAYOUT-006: Admin xử lý booking có refund request → Không được xử lý
- ✅ TC-PAYOUT-007: Admin xử lý booking đã trả tiền → BadRequest

---

## 3. Điểm Quan Trọng

### 3.1. Refund
- ⚠️ **Điều kiện refund**: Phải hủy trước **ít nhất 2 ngày** so với check-in
- ⚠️ **Trạng thái booking**: Chỉ refund được booking đã thanh toán (Confirmed/Completed)
- ⚠️ **RefundRequest status**: "Pending" → "Completed" (khi admin xác nhận)
- ⚠️ **PayOS integration**: Tự động tạo refund payment link qua PayOS

### 3.2. Payout
- ⚠️ **Thời gian chờ**: Booking phải đã kết thúc **ít nhất 15 ngày** mới được thanh toán
- ⚠️ **Điều kiện loại trừ**: Booking có refund request pending/approved sẽ không được thanh toán
- ⚠️ **Wallet của host**: Lấy từ bảng Wallet, ưu tiên default và active
- ⚠️ **Email notification**: Tự động gửi email thông báo cho host khi được thanh toán

### 3.3. Tương Tác Giữa Refund và Payout
- Nếu booking có refund request pending/approved → **KHÔNG** được thanh toán cho host
- Đảm bảo không có xung đột: refund và payout không thể xảy ra đồng thời cho cùng một booking

---

## 4. Endpoints Summary

### Refund Endpoints
- `DELETE /api/booking/{id}` - Tenant hủy booking (tự động tạo refund nếu đã thanh toán)
- `POST /api/booking/{id}/refund` - Tenant yêu cầu hoàn tiền
- `GET /api/admin/refund-requests` - Admin xem danh sách refund requests
- `POST /api/admin/refund-requests/{id}/confirm` - Admin xác nhận hoàn tiền

### Payout Endpoints
- `GET /api/admin/payouts/pending` - Admin xem danh sách booking chờ thanh toán
- `POST /api/admin/payouts/process-all` - Admin xử lý tất cả booking đủ điều kiện
- `POST /api/admin/payouts/{bookingId}/confirm` - Admin xác nhận thanh toán cho một booking
- `GET /api/admin/payouts/paid` - Admin xem danh sách booking đã thanh toán

---

## 5. File Test Đã Tạo

**File**: `CondotelManagement.Tests/Integration/RefundAndPayoutTests.cs`

**Bao gồm**:
- 6 test cases cho Refund (TC-REFUND-001 đến TC-REFUND-006)
- 7 test cases cho Payout (TC-PAYOUT-001 đến TC-PAYOUT-007)

**Tất cả test cases đều**:
- ✅ Kiểm tra status code
- ✅ Kiểm tra response data
- ✅ Kiểm tra database state
- ✅ Sử dụng tiếng Việt trong assertions (nếu có)
- ✅ Mock external services (EmailService)








