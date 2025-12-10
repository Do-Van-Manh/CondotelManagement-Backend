# Host Use Cases - Tài liệu Use Cases cho Host

## Tổng quan
Host là người sở hữu và quản lý các condotel trong hệ thống. Chỉ những user có `roleName === "Host"` mới có thể truy cập các chức năng này.

## Routes và Bảo mật

### Protected Routes (Chỉ Host mới truy cập được)
- `/host-dashboard` - Dashboard chính của Host
- `/add-condotel` - Trang thêm condotel mới
- `/add-listing-1` - Alias cho `/add-condotel`

Tất cả các routes này được bảo vệ bởi `ProtectedRoute` với `requireHost={true}`.

---

## Use Cases

### 1. Quản lý Condotel (Condotel Management)

#### 1.1. Xem danh sách Condotel
**Route:** `/host-dashboard?tab=condotels`  
**Component:** `HostCondotelDashboard` - Tab "Condotels"

**Chức năng:**
- Hiển thị tất cả condotel của host đã tạo
- Hiển thị dạng grid với `CondotelCard`
- Hiển thị loading state khi đang tải
- Hiển thị empty state nếu chưa có condotel

**API:**
- `GET /api/host/condotel` - Lấy tất cả condotels của host

**Use Case Flow:**
1. Host đăng nhập vào hệ thống
2. Truy cập `/host-dashboard`
3. Tab "Condotels" được chọn mặc định
4. Hệ thống tự động gọi API lấy danh sách condotel
5. Hiển thị danh sách condotel dạng card

---

#### 1.2. Thêm Condotel mới
**Route:** `/add-condotel`  
**Component:** `PageAddListingSimple`

**Chức năng:**
- Form đơn giản, tất cả fields trong 1 trang
- Nhập thông tin cơ bản:
  - Tên condotel (required)
  - Loại property (Hotel, Villa, Cottage, etc.)
  - Hình thức cho thuê (Entire place, Private room, Share room)
  - Mô tả
  - Trạng thái (Pending, Available, Unavailable)
- Nhập địa chỉ:
  - Quốc gia
  - Địa chỉ đường/phố (required)
  - Thành phố (required)
  - Mã bưu điện
  - Tên địa điểm (optional)
- Nhập chi tiết:
  - Số giường (required, min: 1)
  - Số phòng tắm (required, min: 1)
  - Giá mỗi đêm (required, min: 0)
- Chọn tiện ích & tiện nghi:
  - Checkboxes cho amenities (Pool, Wifi, Breakfast)
  - Checkboxes cho utilities (Parking, Gym)
- Thêm hình ảnh:
  - Nhập URL hình ảnh
  - Thêm/xóa hình ảnh
  - Xem preview
- Thêm chi tiết phòng:
  - Tên tòa nhà
  - Số phòng
  - Thêm/xóa chi tiết

**Validation:**
- Tên condotel: Required
- Địa chỉ & Thành phố: Required
- Số giường, phòng tắm: Required, > 0
- Giá mỗi đêm: Required, > 0

**API Flow:**
1. Tạo Location trước:
   - `POST /api/host/location` với thông tin địa chỉ
   - Nhận về `locationId`
2. Tạo Condotel:
   - `POST /api/host/condotel` với:
     - `hostId` (từ user context)
     - `name`, `pricePerNight`, `beds`, `bathrooms`, `status`
     - `description` (optional)
     - `images[]` (optional)
     - `details[]` (optional)
     - `amenityIds[]` (optional)
     - `utilityIds[]` (optional)

**Use Case Flow:**
1. Host click "Thêm căn hộ" ở `/host-dashboard`
2. Navigate đến `/add-condotel`
3. Điền form đầy đủ thông tin
4. Click "Tạo Condotel"
5. Hệ thống validate form
6. Tạo location trước
7. Tạo condotel với locationId
8. Hiển thị success message
9. Redirect về `/host-dashboard`

---

### 2. Quản lý Promotion (Promotion Management)

#### 2.1. Xem danh sách Promotion
**Route:** `/host-dashboard?tab=promotions`  
**Component:** `HostPromotionContent`

**Chức năng:**
- Hiển thị tất cả promotions của host
- Hiển thị dạng grid với card
- Mỗi card hiển thị:
  - Tiêu đề promotion
  - Tên condotel
  - Trạng thái (Đang hoạt động / Đã tắt)
  - Mô tả
  - Giảm giá (% hoặc VNĐ)
  - Ngày bắt đầu và kết thúc
  - Nút Sửa và Xóa

**API:**
- `GET /api/host/promotion` - Lấy tất cả promotions của host
- `GET /api/host/condotel` - Lấy danh sách condotels để chọn khi tạo promotion

**Use Case Flow:**
1. Host truy cập `/host-dashboard?tab=promotions`
2. Hệ thống gọi API lấy danh sách promotions và condotels
3. Hiển thị danh sách promotions dạng card
4. Nếu chưa có promotion, hiển thị empty state với nút "Thêm khuyến mãi đầu tiên"

---

#### 2.2. Tạo Promotion mới
**Route:** `/host-dashboard?tab=promotions`  
**Component:** `HostPromotionContent` - Modal "Thêm Khuyến mãi mới"

**Chức năng:**
- Form trong modal để tạo promotion:
  - Chọn Condotel (required) - Dropdown danh sách condotel của host
  - Tiêu đề (required)
  - Mô tả (optional)
  - Giảm giá:
    - Phần trăm giảm giá (0-100%) HOẶC
    - Số tiền giảm giá (VNĐ)
    - Chỉ nhập một trong hai
  - Ngày bắt đầu (required)
  - Ngày kết thúc (required, phải sau ngày bắt đầu)
  - Checkbox "Kích hoạt ngay" (default: true)

**Validation:**
- Condotel: Required
- Tiêu đề: Required, không được để trống
- Ngày bắt đầu & kết thúc: Required
- Ngày kết thúc phải sau ngày bắt đầu
- Phải có % hoặc số tiền giảm giá (ít nhất một trong hai)

**API:**
- `POST /api/host/promotion` với:
  ```typescript
  {
    condotelId: number;
    title: string;
    description?: string;
    discountPercentage?: number;
    discountAmount?: number;
    startDate: string; // ISO date string
    endDate: string; // ISO date string
    isActive?: boolean;
  }
  ```

**Use Case Flow:**
1. Host click "Thêm khuyến mãi" ở tab Promotions
2. Modal hiện lên với form
3. Chọn condotel từ dropdown
4. Nhập tiêu đề và các thông tin khác
5. Chọn ngày bắt đầu và kết thúc
6. Nhập % hoặc số tiền giảm giá
7. Click "Tạo mới"
8. Hệ thống validate form
9. Gọi API tạo promotion
10. Hiển thị success message
11. Đóng modal và refresh danh sách

---

#### 2.3. Sửa Promotion
**Route:** `/host-dashboard?tab=promotions`  
**Component:** `HostPromotionContent` - Modal "Sửa Khuyến mãi"

**Chức năng:**
- Tương tự form tạo, nhưng:
  - Condotel bị disabled (không thể đổi condotel)
  - Form được điền sẵn với dữ liệu promotion hiện tại
  - Tiêu đề modal là "Sửa Khuyến mãi"

**API:**
- `PUT /api/host/promotion/{id}` với:
  ```typescript
  {
    title?: string;
    description?: string;
    discountPercentage?: number;
    discountAmount?: number;
    startDate?: string;
    endDate?: string;
    isActive?: boolean;
  }
  ```

**Use Case Flow:**
1. Host click "Sửa" trên promotion card
2. Modal hiện lên với form đã điền sẵn
3. Chỉnh sửa các thông tin cần thiết
4. Click "Cập nhật"
5. Hệ thống validate và gọi API update
6. Hiển thị success message
7. Đóng modal và refresh danh sách

---

#### 2.4. Xóa Promotion
**Route:** `/host-dashboard?tab=promotions`  
**Component:** `HostPromotionContent`

**Chức năng:**
- Click "Xóa" trên promotion card
- Hiển thị confirmation dialog
- Nếu confirm, gọi API xóa promotion
- Hiển thị loading state trong lúc xóa
- Refresh danh sách sau khi xóa thành công

**API:**
- `DELETE /api/host/promotion/{id}`

**Use Case Flow:**
1. Host click "Xóa" trên promotion card
2. Hiển thị confirm dialog: "Bạn có chắc chắn muốn xóa promotion "{title}"?"
3. Nếu click "OK":
   - Hiển thị loading state
   - Gọi API xóa promotion
   - Hiển thị success message
   - Refresh danh sách
4. Nếu click "Cancel", đóng dialog

---

## Authentication & Authorization

### Login Flow cho Host
1. Host đăng nhập qua `/login`
2. Backend trả về token và user info với `roleName === "Host"`
3. Frontend lưu token và user vào context/localStorage
4. Redirect đến `/host-dashboard`

### Route Protection
- Tất cả host routes sử dụng `ProtectedRoute` với:
  - `requireAuth={true}` - Phải đăng nhập
  - `requireHost={true}` - Phải là Host role
- Nếu không phải Host → Redirect về `/`
- Nếu chưa đăng nhập → Redirect về `/login`

### Component Level Protection
- Các component cũng có `useEffect` check role:
  ```typescript
  useEffect(() => {
    if (!isAuthenticated || user?.roleName !== "Host") {
      navigate("/");
      return;
    }
  }, [isAuthenticated, user, navigate]);
  ```

---

## API Endpoints

### Condotel APIs
- `GET /api/host/condotel` - Lấy tất cả condotels
- `GET /api/host/condotel/{id}` - Lấy condotel theo ID
- `POST /api/host/condotel` - Tạo condotel mới
- `PUT /api/host/condotel/{id}` - Cập nhật condotel
- `DELETE /api/host/condotel/{id}` - Xóa condotel

### Location APIs
- `POST /api/host/location` - Tạo location mới
- `GET /api/host/location` - Lấy tất cả locations
- `GET /api/host/location/{id}` - Lấy location theo ID

### Promotion APIs
- `GET /api/host/promotion` - Lấy tất cả promotions
- `GET /api/host/promotion/{id}` - Lấy promotion theo ID
- `POST /api/host/promotion` - Tạo promotion mới
- `PUT /api/host/promotion/{id}` - Cập nhật promotion
- `DELETE /api/host/promotion/{id}` - Xóa promotion

**Tất cả APIs yêu cầu:**
- JWT Token trong header: `Authorization: Bearer {token}`
- Token phải có role "Host"

---

## Components Structure

```
src/
├── api/
│   ├── condotel.ts          # Condotel API calls
│   ├── promotion.ts         # Promotion API calls
│   └── location.ts          # Location API calls
├── containers/
│   ├── HostCondotelDashboard.tsx      # Main dashboard với tabs
│   ├── HostPromotionPage/
│   │   ├── HostPromotionContent.tsx   # Promotion management
│   │   └── HostPromotionPage.tsx      # Standalone page (optional)
│   └── PageAddListing1/
│       ├── PageAddListingSimple.tsx   # Single page form để thêm condotel
│       ├── _context.tsx              # AddCondotelContext
│       └── AddListingLayout.tsx      # Layout wrapper với Provider
└── components/
    └── ProtectedRoute/
        └── ProtectedRoute.tsx        # Route protection component
```

---

## Error Handling

### Validation Errors
- Client-side validation: Hiển thị error message ngay trong form
- Server-side validation: Hiển thị error từ backend response
- Format: Hiển thị từng field và message lỗi

### API Errors
- Network errors: "Không thể kết nối đến server"
- 401 Unauthorized: Redirect về `/login`
- 403 Forbidden: "Bạn không có quyền truy cập"
- 404 Not Found: "Không tìm thấy dữ liệu"
- 500 Server Error: "Lỗi máy chủ. Vui lòng thử lại sau"

---

## State Management

### Context API
- `AddCondotelContext`: Quản lý form data khi thêm condotel
- `AuthContext`: Quản lý authentication state và user info

### Local State
- Mỗi component sử dụng `useState` cho local state
- `useEffect` để sync state với context khi cần

---

## Summary - Tóm tắt Use Cases

### Host có thể:
1. ✅ Xem danh sách condotel của mình
2. ✅ Thêm condotel mới (1 trang form đơn giản)
3. ✅ Xem danh sách promotions
4. ✅ Tạo promotion mới cho condotel
5. ✅ Sửa promotion
6. ✅ Xóa promotion

### Host KHÔNG thể:
- ❌ Truy cập Admin pages (`/admin/*`)
- ❌ Xem condotel của host khác (API tự động filter theo hostId từ JWT)
- ❌ Tạo promotion cho condotel không thuộc về mình

---

## Testing Scenarios

### Test Cases cho Host
1. **Login as Host**
   - Login với account có roleName = "Host"
   - Verify redirect đến `/host-dashboard`

2. **View Condotels**
   - Access `/host-dashboard`
   - Verify tab "Condotels" được chọn
   - Verify danh sách condotel hiển thị

3. **Add Condotel**
   - Click "Thêm căn hộ"
   - Fill form đầy đủ
   - Submit
   - Verify condotel được tạo và hiển thị trong danh sách

4. **View Promotions**
   - Click tab "Khuyến mãi"
   - Verify danh sách promotions hiển thị

5. **Create Promotion**
   - Click "Thêm khuyến mãi"
   - Fill form và submit
   - Verify promotion được tạo

6. **Edit Promotion**
   - Click "Sửa" trên promotion card
   - Update thông tin
   - Submit
   - Verify promotion được cập nhật

7. **Delete Promotion**
   - Click "Xóa" trên promotion card
   - Confirm
   - Verify promotion bị xóa

8. **Access Control**
   - Login với account không phải Host
   - Try access `/host-dashboard`
   - Verify redirect về `/`

---

## Notes

- Tất cả APIs sử dụng `/api/host/*` prefix
- JWT token được tự động gửi trong headers bởi `axiosClient`
- Token được lưu trong localStorage và tự động refresh
- Error handling được implement ở cả client và server side







