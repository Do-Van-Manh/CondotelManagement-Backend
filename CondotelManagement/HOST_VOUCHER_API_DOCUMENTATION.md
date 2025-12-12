# 📋 TÀI LIỆU API VOUCHER CỦA HOST

## 🎯 TỔNG QUAN

Host có thể quản lý voucher cho các condotel của mình thông qua 2 nhóm API:
1. **Quản lý Voucher** (`/api/host/vouchers`) - CRUD voucher
2. **Cài đặt Auto Generate** (`/api/host/settings/voucher`) - Cấu hình tự động phát voucher

---

## 📌 NHÓM 1: QUẢN LÝ VOUCHER

### Base URL: `/api/host/vouchers`
**Authorization:** `[Authorize(Roles = "Host")]`

---

### 1. **GET `/api/host/vouchers`** - Lấy danh sách voucher của host

**Mô tả:** Lấy tất cả voucher thuộc về các condotel của host hiện tại

**Request:**
```
GET /api/host/vouchers
Headers: Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "voucherID": 1,
      "condotelID": 5,
      "condotelName": "Căn hộ cao cấp",
      "userID": 10,
      "fullName": "Nguyễn Văn A",
      "code": "SUMMER2024",
      "discountAmount": 50000,
      "discountPercentage": null,
      "startDate": "2024-01-01",
      "endDate": "2024-12-31",
      "status": "Active"
    }
  ]
}
```

**Logic:**
- Lấy `hostId` từ user đang đăng nhập
- Query tất cả voucher có `Condotel.HostId == hostId` và `Status == "Active"`
- Trả về danh sách voucher với thông tin condotel và user

---

### 2. **POST `/api/host/vouchers`** - Tạo voucher mới

**Mô tả:** Host tạo voucher thủ công cho một condotel cụ thể

**Request:**
```json
POST /api/host/vouchers
Headers: Authorization: Bearer {token}
Content-Type: application/json

{
  "condotelID": 5,                    // BẮT BUỘC: Phải có CondotelID
  "userID": 10,                       // Optional: Nếu có = voucher cá nhân cho user đó
  "code": "SUMMER2024",              // BẮT BUỘC: Mã voucher (unique)
  "discountAmount": 50000,            // Optional: Giảm giá theo số tiền (VNĐ)
  "discountPercentage": null,         // Optional: Giảm giá theo % (0-100)
  "startDate": "2024-01-01",          // BẮT BUỘC: Ngày bắt đầu
  "endDate": "2024-12-31",            // BẮT BUỘC: Ngày kết thúc
  "usageLimit": 100                   // Optional: Giới hạn số lần sử dụng
}
```

**Validation:**
- ✅ `StartDate < EndDate` (Ngày bắt đầu phải nhỏ hơn ngày kết thúc)
- ✅ `CondotelID` phải > 0 và thuộc về host hiện tại
- ✅ `Code` phải unique trong hệ thống
- ✅ Phải có ít nhất một trong hai: `DiscountAmount` hoặc `DiscountPercentage`

**Response Success:**
```json
{
  "success": true,
  "message": "Đã tạo thành công",
  "data": {
    "voucherID": 1,
    "condotelID": 5,
    "condotelName": "Căn hộ cao cấp",
    "userID": 10,
    "fullName": "Nguyễn Văn A",
    "code": "SUMMER2024",
    "discountAmount": 50000,
    "discountPercentage": null,
    "startDate": "2024-01-01",
    "endDate": "2024-12-31",
    "status": "Active"
  }
}
```

**Response Error:**
```json
{
  "success": false,
  "errors": {
    "StartDate": ["Ngày bắt đầu phải nhỏ hơn ngày kết thúc."],
    "EndDate": ["Ngày kết thúc phải lớn hơn ngày bắt đầu."]
  }
}
```

**Logic:**
1. Validate input (StartDate < EndDate, CondotelID required)
2. Kiểm tra CondotelID có thuộc về host không
3. Tạo voucher với `Status = "Active"`
4. Lưu vào database
5. Trả về voucher đã tạo

---

### 3. **PUT `/api/host/vouchers/{id}`** - Cập nhật voucher

**Mô tả:** Cập nhật thông tin voucher (chỉ voucher thuộc về host)

**Request:**
```json
PUT /api/host/vouchers/1
Headers: Authorization: Bearer {token}
Content-Type: application/json

{
  "condotelID": 5,
  "userID": 10,
  "code": "SUMMER2024_UPDATED",
  "discountAmount": 75000,
  "discountPercentage": null,
  "startDate": "2024-02-01",
  "endDate": "2024-12-31",
  "usageLimit": 150
}
```

**Validation:** Tương tự như Create

**Response Success:**
```json
{
  "success": true,
  "message": "Đã sửa thành công",
  "data": { ... }
}
```

**Response Error:**
- `404 Not Found` - Voucher không tồn tại hoặc không thuộc về host

**Logic:**
1. Tìm voucher theo ID
2. Kiểm tra voucher có thuộc về host không (qua Condotel.HostId)
3. Cập nhật thông tin
4. Lưu vào database

---

### 4. **DELETE `/api/host/vouchers/{id}`** - Xóa voucher

**Mô tả:** Xóa voucher (soft delete - chuyển Status = "Inactive")

**Request:**
```
DELETE /api/host/vouchers/1
Headers: Authorization: Bearer {token}
```

**Response Success:**
```json
{
  "success": true,
  "message": "Đã xóa thành công"
}
```

**Response Error:**
- `404 Not Found` - Voucher không tồn tại

**Logic:**
- Soft delete: Chuyển `Status = "Inactive"` (không xóa khỏi database)
- Voucher vẫn tồn tại nhưng không còn hiển thị trong danh sách Active

---

## 📌 NHÓM 2: CÀI ĐẶT AUTO GENERATE

### Base URL: `/api/host/settings/voucher`
**Authorization:** `[Authorize(Roles = "Host")]`

---

### 5. **GET `/api/host/settings/voucher`** - Lấy cài đặt auto generate

**Mô tả:** Lấy cấu hình tự động phát voucher của host

**Request:**
```
GET /api/host/settings/voucher
Headers: Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "settingID": 1,
    "hostID": 3,
    "discountAmount": 50000,
    "discountPercentage": null,
    "autoGenerate": true,              // Bật/tắt tự động phát voucher
    "validMonths": 3,                  // Thời hạn voucher (tháng)
    "usageLimit": 1                    // Số lần sử dụng tối đa
  }
}
```

**Response null:**
```json
{
  "success": true,
  "data": null  // Chưa có cài đặt
}
```

**Logic:**
- Lấy `hostId` từ user đang đăng nhập
- Query `HostVoucherSetting` theo `hostId`
- Trả về setting hoặc null nếu chưa có

---

### 6. **POST `/api/host/settings/voucher`** - Lưu cài đặt auto generate

**Mô tả:** Cấu hình tự động phát voucher khi booking completed

**Request:**
```json
POST /api/host/settings/voucher
Headers: Authorization: Bearer {token}
Content-Type: application/json

{
  "discountAmount": 50000,            // Optional: Giảm giá theo số tiền
  "discountPercentage": 10,           // Optional: Giảm giá theo %
  "autoGenerate": true,               // BẮT BUỘC: Bật/tắt tự động phát
  "validMonths": 3,                   // BẮT BUỘC: Thời hạn voucher (tháng)
  "usageLimit": 1                     // Optional: Số lần sử dụng tối đa
}
```

**Response Success:**
```json
{
  "success": true,
  "message": "Lưu setting thành công",
  "data": {
    "settingID": 1,
    "hostID": 3,
    "discountAmount": 50000,
    "discountPercentage": null,
    "autoGenerate": true,
    "validMonths": 3,
    "usageLimit": 1
  }
}
```

**Logic:**
1. Lấy `hostId` từ user đang đăng nhập
2. Tìm hoặc tạo `HostVoucherSetting` cho host
3. Cập nhật/thêm setting
4. Lưu vào database

**Cách hoạt động Auto Generate:**
- Khi `autoGenerate = true`:
  - Khi booking chuyển sang `Status = "Completed"`
  - Hệ thống tự động tạo voucher cho **TẤT CẢ condotel** của host
  - Mỗi condotel = 1 voucher
  - Voucher được gửi cho customer qua email
- Khi `autoGenerate = false`:
  - Không tự động tạo voucher
  - Host phải tạo voucher thủ công

---

## 🔄 LUỒNG HOẠT ĐỘNG TỰ ĐỘNG PHÁT VOUCHER

```
1. Customer đặt phòng → Booking Status = "Pending"
2. Customer thanh toán → Booking Status = "Confirmed"
3. Qua EndDate → Background Service chuyển Status = "Completed"
4. BookingStatusUpdateService kiểm tra:
   ├─ Lấy Condotel → Lấy HostId
   ├─ Lấy HostVoucherSetting
   └─ Nếu AutoGenerate = true:
      ├─ Tạo voucher cho TẤT CẢ condotel của host
      ├─ Mỗi voucher có:
      │  ├─ Code: BOOK{userId}{random}
      │  ├─ DiscountAmount/Percentage từ setting
      │  ├─ StartDate: Hôm nay
      │  ├─ EndDate: Hôm nay + ValidMonths
      │  └─ UsageLimit từ setting
      └─ Gửi email thông báo voucher cho customer
```

---

## 📊 CÁC TRẠNG THÁI VOUCHER

- **Active** - Voucher đang hoạt động, có thể sử dụng
- **Inactive** - Voucher đã bị xóa (soft delete)
- **Expired** - Voucher đã hết hạn (tự động cập nhật bởi VoucherStatusUpdateService)
- **Used** - Voucher đã được sử dụng hết (UsedCount >= UsageLimit)

---

## ⚠️ LƯU Ý QUAN TRỌNG

### 1. **Voucher phải gắn với Condotel**
- Mỗi voucher **BẮT BUỘC** phải có `CondotelID`
- Voucher chỉ áp dụng cho condotel cụ thể
- Không có voucher "dùng cho tất cả condotel"

### 2. **Voucher cá nhân vs Voucher công khai**
- Nếu có `UserID` → Voucher cá nhân (chỉ user đó dùng được)
- Nếu `UserID = null` → Voucher công khai (ai cũng dùng được)

### 3. **Giảm giá**
- Phải có ít nhất một trong hai: `DiscountAmount` HOẶC `DiscountPercentage`
- Không thể có cả hai cùng lúc (logic nghiệp vụ)

### 4. **Auto Generate**
- Chỉ tạo voucher khi booking **Completed** (không phải Confirmed)
- Tạo voucher cho **TẤT CẢ** condotel của host
- Customer nhận email thông báo tự động

### 5. **Validation**
- `StartDate < EndDate` (bắt buộc)
- `Code` phải unique
- `CondotelID` phải thuộc về host hiện tại

---

## 🧪 VÍ DỤ SỬ DỤNG

### Tạo voucher thủ công:
```bash
POST /api/host/vouchers
{
  "condotelID": 5,
  "code": "WELCOME2024",
  "discountAmount": 100000,
  "startDate": "2024-01-01",
  "endDate": "2024-12-31",
  "usageLimit": 50
}
```

### Bật auto generate:
```bash
POST /api/host/settings/voucher
{
  "autoGenerate": true,
  "discountAmount": 50000,
  "validMonths": 3,
  "usageLimit": 1
}
```

### Lấy danh sách voucher:
```bash
GET /api/host/vouchers
→ Trả về tất cả voucher của host (chỉ Status = "Active")
```

---

## 🔍 CÁC API LIÊN QUAN

### Tenant API (Customer):
- `GET /api/vouchers/my` - Lấy voucher của user
- `GET /api/vouchers/condotel/{condotelId}` - Lấy voucher theo condotel

### Validation khi booking:
- Khi customer nhập voucher code → `ValidateVoucherByCodeAsync()`
- Kiểm tra: Status, thời hạn, condotel, user, usage limit

---

## 📝 TÓM TẮT

**Host có thể:**
1. ✅ Xem danh sách voucher của mình
2. ✅ Tạo voucher thủ công cho từng condotel
3. ✅ Cập nhật voucher
4. ✅ Xóa voucher (soft delete)
5. ✅ Cấu hình auto generate voucher
6. ✅ Bật/tắt tự động phát voucher khi booking completed

**Hệ thống tự động:**
- ✅ Tạo voucher khi booking completed (nếu AutoGenerate = true)
- ✅ Gửi email thông báo voucher cho customer
- ✅ Cập nhật Status = "Expired" khi voucher hết hạn

