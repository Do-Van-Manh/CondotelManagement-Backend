# 🔍 Phân Tích Lỗi Luồng Booking

## 📋 Luồng Booking Hiện Tại

```
1. CreateBookingAsync
   ↓
2. Payment (PayOS)
   ↓
3. Webhook/Return URL → Status = "Confirmed"
   ↓
4. Background Service → Status = "Completed" (khi qua EndDate)
   ↓
5. Payout (sau 15 ngày)
```

---

## ❌ Các Lỗi Tiềm Ẩn Đã Phát Hiện

### 1. **Race Condition - CheckAvailability không thread-safe**

**Vị trí**: `CheckAvailability()` method

**Vấn đề**:
```csharp
public bool CheckAvailability(int condotelId, DateOnly checkIn, DateOnly checkOut)
{
    var bookings = _context.Bookings
        .Where(b => b.CondotelId == condotelId
            && b.Status != "Cancelled"
            && b.EndDate >= today
            && b.StartDate <= checkOut)
        .ToList();
    
    return !bookings.Any(b =>
        checkIn < b.EndDate &&
        checkOut > b.StartDate
    );
}
```

**Lỗi**:
- Không có transaction/lock
- Nhiều user cùng lúc có thể check và tạo booking cho cùng 1 phòng
- Có thể dẫn đến double booking

**Giải pháp**:
- Sử dụng database transaction với isolation level `Serializable`
- Hoặc sử dụng row-level lock khi check availability

---

### 2. **Voucher UsedCount tăng ngay khi tạo booking, không phải khi payment thành công**

**Vị trí**: `CreateBookingAsync()` - Line 288-292

**Vấn đề**:
```csharp
// Cập nhật UsedCount của voucher sau khi booking được tạo thành công
if (appliedVoucherId.HasValue)
{
    await _voucherService.ApplyVoucherToBookingAsync(appliedVoucherId.Value);
}
```

**Lỗi**:
- Voucher được đánh dấu "đã dùng" ngay khi tạo booking
- Nếu booking bị cancel hoặc payment fail → Voucher đã bị dùng nhưng booking không thành công
- User mất voucher mà không được sử dụng

**Giải pháp**:
- Chỉ tăng UsedCount khi payment thành công (trong webhook)
- Hoặc rollback UsedCount nếu booking bị cancel

---

### 3. **Service Package Validation - Query Condotel 2 lần**

**Vị trí**: `CreateBookingAsync()` - Line 237-241

**Vấn đề**:
```csharp
// Validate service package thuộc về host của condotel này
var condotelForValidation = _condotelRepo.GetCondotelById(dto.CondotelId);
if (condotelForValidation == null)
{
    return ServiceResultDTO.Fail("Condotel not found.");
}
```

**Lỗi**:
- Condotel đã được query ở line 132: `var condotel = _condotelRepo.GetCondotelById(dto.CondotelId);`
- Query lại lần 2 trong loop service packages → không cần thiết, tốn performance

**Giải pháp**:
- Sử dụng lại biến `condotel` đã query trước đó

---

### 4. **CheckAvailability không check Status = "Pending"**

**Vị trí**: `CheckAvailability()` method

**Vấn đề**:
```csharp
var bookings = _context.Bookings
    .Where(b => b.CondotelId == condotelId
        && b.Status != "Cancelled"  // ❌ Chỉ loại bỏ "Cancelled"
        && b.EndDate >= today
        && b.StartDate <= checkOut)
```

**Lỗi**:
- Booking có status "Pending" vẫn được tính là đã booked
- Nhưng "Pending" có thể bị cancel hoặc payment fail
- Có thể dẫn đến false negative (nghĩ là đã booked nhưng thực tế chưa)

**Giải pháp**:
- Chỉ tính bookings có status "Confirmed" hoặc "Completed"
- Hoặc check thêm điều kiện: `b.Status == "Confirmed" || b.Status == "Completed"`

---

### 5. **UpdateBooking không có validation**

**Vị trí**: `UpdateBooking()` method - Line 301-315

**Vấn đề**:
```csharp
public BookingDTO UpdateBooking(BookingDTO dto)
{
    var booking = _bookingRepo.GetBookingById(dto.BookingId);
    if (booking == null) return null;

    booking.StartDate = dto.StartDate;
    booking.EndDate = dto.EndDate;
    booking.Status = dto.Status;  // ❌ Có thể update status bất kỳ
    booking.TotalPrice = dto.TotalPrice;

    _bookingRepo.UpdateBooking(booking);
    _bookingRepo.SaveChanges();

    return ToDTO(booking);
}
```

**Lỗi**:
- Không validate date range
- Không validate status transition (có thể chuyển từ "Completed" → "Pending")
- Không check availability khi update dates
- Không validate business rules

**Giải pháp**:
- Thêm validation cho date range
- Validate status transition hợp lệ
- Check availability nếu update dates
- Chỉ cho phép update một số fields nhất định

---

### 6. **Không sử dụng Transaction trong CreateBookingAsync**

**Vị trí**: `CreateBookingAsync()` method

**Vấn đề**:
- Tạo booking → Save
- Tạo BookingDetails → Save
- Update Voucher UsedCount → Save

**Lỗi**:
- Nếu bước 2 hoặc 3 fail → Booking đã được tạo nhưng không có details/voucher
- Data inconsistency

**Giải pháp**:
- Wrap toàn bộ trong 1 transaction
- Rollback nếu có lỗi

---

### 7. **Voucher validation không check UsedCount trước khi apply**

**Vị trí**: `CreateBookingAsync()` - Line 192-201

**Vấn đề**:
- Validate voucher nhưng không check UsedCount hiện tại
- Có thể validate pass nhưng khi apply thì đã hết lượt (nếu có booking khác dùng trước)

**Giải pháp**:
- Check UsedCount trong transaction
- Hoặc lock voucher khi validate

---

### 8. **Service Package Price có thể thay đổi sau khi booking được tạo**

**Vị trí**: `CreateBookingAsync()` - Line 250-260

**Vấn đề**:
```csharp
// Tính tiền: Price * Quantity
decimal serviceTotal = servicePackage.Price * serviceSelection.Quantity;
servicePackagesTotal += serviceTotal;

// Tạo BookingDetail
bookingDetails.Add(new BookingDetail
{
    ServiceId = serviceSelection.ServiceId,
    Quantity = serviceSelection.Quantity,
    Price = servicePackage.Price  // ✅ Đã lưu price vào BookingDetail
});
```

**Tốt**: Đã lưu Price vào BookingDetail → OK

**Nhưng**: Nếu service package bị xóa hoặc inactive sau khi booking được tạo → có thể gây confusion

---

### 9. **CheckAvailability không check StartDate**

**Vị trí**: `CheckAvailability()` method

**Vấn đề**:
```csharp
var bookings = _context.Bookings
    .Where(b => b.CondotelId == condotelId
        && b.Status != "Cancelled"
        && b.EndDate >= today  // ✅ Check EndDate
        && b.StartDate <= checkOut)  // ✅ Check StartDate
```

**Tốt**: Đã check đúng

---

### 10. **CancelBooking - Voucher không được rollback**

**Vị trí**: `CancelBooking()` method - Line 318-351

**Vấn đề**:
- Khi cancel booking, voucher UsedCount không được giảm lại
- User mất voucher mà không được hoàn lại

**Giải pháp**:
- Rollback voucher UsedCount khi cancel booking
- Hoặc tạo voucher mới cho user

---

### 11. **CreateBookingAsync - DateTime.Now vs DateTime.UtcNow**

**Vị trí**: Line 113, 271

**Vấn đề**:
```csharp
var today = DateOnly.FromDateTime(DateTime.Now);  // ❌ Local time
dto.CreatedAt = DateTime.Now;  // ❌ Local time
```

**Lỗi**:
- Sử dụng `DateTime.Now` (local time) thay vì `DateTime.UtcNow`
- Có thể gây vấn đề khi deploy lên server ở timezone khác

**Giải pháp**:
- Sử dụng `DateTime.UtcNow` cho consistency

---

### 12. **CheckAvailability - Không check booking đang trong quá trình thanh toán**

**Vị trí**: `CheckAvailability()` method

**Vấn đề**:
- Booking "Pending" (đang chờ thanh toán) vẫn được tính là available
- Có thể dẫn đến overbooking nếu nhiều user cùng tạo booking "Pending"

**Giải pháp**:
- Có thể thêm timeout cho booking "Pending" (ví dụ: 15 phút)
- Hoặc chỉ tính "Confirmed" và "Completed" là booked

---

## ✅ Điểm Tốt

1. ✅ **Service Package Price được lưu vào BookingDetail** - Đảm bảo giá không thay đổi
2. ✅ **Validation đầy đủ** - Date range, promotion, voucher, service packages
3. ✅ **Check host không được book chính mình** - Business rule đúng
4. ✅ **Background service tự động update status** - Tự động hóa tốt
5. ✅ **Voucher validation đầy đủ** - Check status, date, condotel, user, usage limit

---

## 🔧 Khuyến Nghị Sửa Lỗi

### Ưu tiên cao:
1. **Race Condition** - Thêm transaction/lock
2. **Voucher UsedCount** - Chỉ tăng khi payment thành công
3. **Transaction** - Wrap CreateBookingAsync trong transaction
4. **DateTime** - Sử dụng UtcNow

### Ưu tiên trung bình:
5. **UpdateBooking validation** - Thêm business rules
6. **CheckAvailability** - Chỉ tính "Confirmed"/"Completed"
7. **Service Package query** - Tối ưu query

### Ưu tiên thấp:
8. **CancelBooking voucher rollback** - Có thể làm sau

