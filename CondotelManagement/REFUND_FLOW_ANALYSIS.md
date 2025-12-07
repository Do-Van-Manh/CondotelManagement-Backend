# 🔄 Phân Tích Luồng Hoàn Tiền Khi Hủy Booking

## 📋 Luồng Hoàn Tiền Hiện Tại

```
1. User hủy booking (CancelBooking)
   ↓
2. Nếu status = "Confirmed"/"Completed" → Gọi RefundBooking
   ↓
3. RefundBooking → Validate → ProcessRefund
   ↓
4. ProcessRefund → Tạo RefundRequest + PayOS refund link
   ↓
5. Customer nhận tiền qua PayOS link
   ↓
6. Webhook/Return URL → Update RefundRequest.Status = "Refunded"
```

---

## 🔍 Chi Tiết Luồng

### 1. **CancelBooking** (`CancelBooking()` - Line 318)

**Flow:**
```csharp
if (booking.Status == "Confirmed" || booking.Status == "Completed")
{
    // Tự động gọi RefundBooking
    var refundResult = await RefundBooking(bookingId, customerId);
    
    // Set status = "Cancelled" (dù refund thành công hay không)
    booking.Status = "Cancelled";
    _bookingRepo.UpdateBooking(booking);
    return _bookingRepo.SaveChanges();
}
else if (booking.Status == "Pending")
{
    // Chưa thanh toán, chỉ cần hủy
    booking.Status = "Cancelled";
    _bookingRepo.UpdateBooking(booking);
    return _bookingRepo.SaveChanges();
}
```

**Vấn đề:**
- ❌ Status được set "Cancelled" ngay cả khi refund fail
- ❌ Không rollback voucher UsedCount
- ❌ Không có transaction

---

### 2. **RefundBooking** (`RefundBooking()` - Line 353)

**Validation:**
```csharp
// 1. Check booking tồn tại và thuộc về customer
if (booking == null || booking.CustomerId != customerId)
    return Fail("Booking not found");

// 2. Check đã refund chưa
if (existingRefundRequest != null && 
    (existingRefundRequest.Status == "Completed" || existingRefundRequest.Status == "Refunded"))
    return Fail("Already refunded");

// 3. Check status hợp lệ
if (booking.Status != "Cancelled" && booking.Status != "Confirmed" && 
    booking.Status != "Completed" && booking.Status != "Refunded")
    return Fail("Only cancelled, confirmed, or completed bookings can be refunded");

// 4. Check thời gian (phải hủy trước 2 ngày)
var daysBeforeCheckIn = (startDateTime - now).TotalDays;
if (daysBeforeCheckIn < 2)
    return Fail("Refund is only available when cancelling at least 2 days before check-in");
```

**Vấn đề:**
- ✅ Validation khá đầy đủ
- ❌ Không check booking đã được payout cho host chưa
- ❌ Không rollback voucher

---

### 3. **ProcessRefund** (`ProcessRefund()` - Line 414)

**Flow:**
```csharp
// 1. Lấy thông tin customer và wallet
var customer = await _context.Users.Include(u => u.Wallets).FirstOrDefaultAsync(...);

// 2. Lấy bank info (từ request hoặc wallet)
string? bankCode = requestBankCode;
string? accountNumber = requestAccountNumber;
string? accountHolder = requestAccountHolder;

if (string.IsNullOrEmpty(bankCode) || string.IsNullOrEmpty(accountNumber))
{
    // Lấy từ Wallet
    var customerWallet = customer.Wallets.FirstOrDefault();
    accountNumber = customerWallet?.AccountNumber;
    accountHolder = customerWallet?.AccountHolderName;
    bankCode = // Map từ BankName
}

// 3. Tạo hoặc update RefundRequest
var existingRefundRequest = await _context.RefundRequests
    .FirstOrDefaultAsync(r => r.BookingId == booking.BookingId);

if (existingRefundRequest != null)
{
    // Update bank info
    refundRequest = existingRefundRequest;
    refundRequest.BankCode = bankCode;
    refundRequest.AccountNumber = accountNumber;
    refundRequest.AccountHolder = accountHolder;
}
else
{
    // Tạo mới
    refundRequest = new RefundRequest { ... };
    _context.RefundRequests.Add(refundRequest);
}

// 4. Tạo PayOS refund link
if (shouldCreatePayOSLink)
{
    refundResponse = await _payOSService.CreateRefundPaymentLinkAsync(...);
}

// 5. Update status
if (refundResponse != null && refundResponse.Code == "00")
{
    booking.Status = "Cancelled";
    refundRequest.Status = "Pending";
    refundRequest.TransactionId = refundResponse.Data.PaymentLinkId?.ToString();
    await _context.SaveChangesAsync();
}
```

**Vấn đề:**
- ❌ Không có transaction → có thể tạo RefundRequest nhưng không tạo PayOS link
- ❌ Không rollback voucher
- ❌ Không check booking đã được payout cho host
- ❌ DateTime.Now thay vì UtcNow (line 492, 512, 943)

---

### 4. **Webhook/Return URL** (`PaymentController` - Line 366-398)

**Flow:**
```csharp
// Kiểm tra có phải refund payment không
var orderCodeSuffix = orderCode % 1000000;
var isRefundPayment = orderCodeSuffix == 999999;

if (isRefundPayment)
{
    var refundRequest = await _context.RefundRequests
        .Include(r => r.Booking)
        .FirstOrDefaultAsync(r => r.BookingId == bookingId && r.Status == "Pending");

    if (status == "PAID" && cancel != "true")
    {
        // Customer đã nhận tiền
        refundRequest.Status = "Refunded";
        refundRequest.ProcessedAt = DateTime.Now;
        refundRequest.UpdatedAt = DateTime.Now;
        // Booking status giữ nguyên "Cancelled"
        await _context.SaveChangesAsync();
    }
}
```

**Vấn đề:**
- ✅ Logic đúng
- ❌ DateTime.Now thay vì UtcNow
- ❌ Không rollback voucher

---

### 5. **AdminRefundBooking** (`AdminRefundBooking()` - Line 391)

**Flow:**
```csharp
// Admin có thể refund bất kỳ booking nào (trừ Pending)
if (booking.Status == "Pending")
    return Fail("Cannot refund a booking that has not been paid");

return await ProcessRefund(booking, "Admin", reason);
```

**Vấn đề:**
- ❌ Admin có thể refund booking đã được payout cho host
- ❌ Không check IsPaidToHost

---

## ❌ Các Lỗi Tiềm Ẩn

### 1. **Voucher không được rollback khi refund**

**Vấn đề:**
- Khi tạo booking, voucher UsedCount được tăng
- Khi refund, voucher không được rollback
- User mất voucher mà không được hoàn lại

**Vị trí**: Không có logic rollback voucher trong `ProcessRefund()`

**Giải pháp:**
```csharp
// Trong ProcessRefund, sau khi tạo RefundRequest
if (booking.VoucherId.HasValue)
{
    await _voucherService.RollbackVoucherUsageAsync(booking.VoucherId.Value);
}
```

---

### 2. **Không check booking đã được payout cho host**

**Vấn đề:**
- Booking có thể đã được payout cho host (IsPaidToHost = true)
- Nhưng vẫn có thể refund → mất tiền

**Vị trí**: `RefundBooking()` và `ProcessRefund()`

**Giải pháp:**
```csharp
// Trong RefundBooking
if (booking.IsPaidToHost == true)
{
    return ServiceResultDTO.Fail("Cannot refund booking that has already been paid to host.");
}
```

---

### 3. **Không có Transaction trong ProcessRefund**

**Vấn đề:**
- Tạo RefundRequest → Save
- Tạo PayOS link → Có thể fail
- Nếu PayOS fail → RefundRequest đã được tạo nhưng không có payment link
- Data inconsistency

**Vị trí**: `ProcessRefund()` method

**Giải pháp:**
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Tạo RefundRequest
    // Tạo PayOS link
    // Save changes
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

### 4. **DateTime.Now thay vì DateTime.UtcNow**

**Vấn đề:**
- Sử dụng `DateTime.Now` (local time) ở nhiều nơi
- Có thể gây vấn đề khi deploy ở timezone khác

**Vị trí**: 
- Line 492: `refundRequest.UpdatedAt = DateTime.Now;`
- Line 512: `CancelDate = DateTime.Now;`
- Line 943: `ProcessedAt = DateTime.Now;`

**Giải pháp**: Thay tất cả bằng `DateTime.UtcNow`

---

### 5. **CancelBooking set status "Cancelled" ngay cả khi refund fail**

**Vấn đề:**
```csharp
var refundResult = await RefundBooking(bookingId, customerId);
if (!refundResult.Success)
{
    // Refund fail nhưng vẫn set Cancelled
    booking.Status = "Cancelled";
    _bookingRepo.UpdateBooking(booking);
    return _bookingRepo.SaveChanges();
}
```

**Lỗi:**
- Booking bị cancel nhưng không có refund request
- User mất booking nhưng không được hoàn tiền

**Giải pháp:**
- Chỉ set "Cancelled" nếu refund thành công
- Hoặc tạo RefundRequest với status "Pending" ngay cả khi PayOS fail

---

### 6. **RefundRequest có thể bị duplicate**

**Vấn đề:**
- `ProcessRefund()` check existing request nhưng không có lock
- Nhiều request cùng lúc có thể tạo nhiều RefundRequest

**Vị trí**: `ProcessRefund()` - Line 478

**Giải pháp:**
- Sử dụng transaction với isolation level
- Hoặc check và create trong 1 query

---

### 7. **AdminRefundBooking không check IsPaidToHost**

**Vấn đề:**
- Admin có thể refund booking đã được payout
- Dẫn đến mất tiền

**Vị trí**: `AdminRefundBooking()` - Line 391

**Giải pháp:**
```csharp
if (booking.IsPaidToHost == true)
{
    return ServiceResultDTO.Fail("Cannot refund booking that has already been paid to host.");
}
```

---

### 8. **Wallet.FirstOrDefault() không ưu tiên IsDefault**

**Vấn đề:**
```csharp
var customerWallet = customer.Wallets.FirstOrDefault();
```

**Lỗi:**
- Lấy wallet đầu tiên, không ưu tiên default wallet
- Có thể lấy sai wallet

**Giải pháp:**
```csharp
var customerWallet = customer.Wallets
    .Where(w => w.Status == "Active")
    .OrderByDescending(w => w.IsDefault)
    .FirstOrDefault();
```

---

### 9. **RefundRequest Status không consistent**

**Vấn đề:**
- Status có thể là "Pending", "Completed", "Refunded"
- "Completed" và "Refunded" đều có nghĩa là đã hoàn tiền
- Gây confusion

**Giải pháp:**
- Thống nhất: "Pending" → "Refunded" (bỏ "Completed")
- Hoặc: "Pending" → "Completed" (bỏ "Refunded")

---

### 10. **Không validate bank info đầy đủ**

**Vấn đề:**
- Có thể tạo RefundRequest với bank info không đầy đủ
- PayOS link có thể fail nhưng RefundRequest vẫn được tạo

**Giải pháp:**
- Validate bank info trước khi tạo RefundRequest
- Hoặc yêu cầu bank info bắt buộc

---

## ✅ Điểm Tốt

1. ✅ **Validation đầy đủ** - Check status, thời gian, duplicate
2. ✅ **Lấy bank info từ Wallet** - Fallback tốt
3. ✅ **Update existing RefundRequest** - Tránh duplicate
4. ✅ **Webhook xử lý đúng** - Update status khi customer nhận tiền
5. ✅ **Admin có thể refund manual** - Flexible

---

## 🔧 Khuyến Nghị Sửa Lỗi

### Ưu tiên cao:
1. **Rollback Voucher** - Giảm UsedCount khi refund
2. **Check IsPaidToHost** - Không cho refund booking đã payout
3. **Transaction** - Wrap ProcessRefund trong transaction
4. **DateTime.UtcNow** - Thay tất cả DateTime.Now

### Ưu tiên trung bình:
5. **CancelBooking logic** - Chỉ cancel nếu refund thành công
6. **Wallet selection** - Ưu tiên default wallet
7. **Bank info validation** - Validate trước khi tạo RefundRequest

### Ưu tiên thấp:
8. **Status consistency** - Thống nhất "Completed" vs "Refunded"
9. **Duplicate prevention** - Thêm lock cho RefundRequest creation

