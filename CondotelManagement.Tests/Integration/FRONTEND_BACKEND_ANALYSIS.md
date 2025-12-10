# Phân Tích Front-end và Back-end để Tạo Test Cases Đầy Đủ

## Tổng Quan

Sau khi phân tích cả Front-end (chisfis-booking-main) và Back-end (CondotelManagement), tôi đã xác định được các màn hình, flows, và interactions cần test.

## Cấu Trúc Front-end

### 1. Routing và Pages
- **Public Pages**: Home, Listing, Detail, About, Contact, Blog
- **Auth Pages**: Login, SignUp, ForgotPassword
- **Tenant Pages**: Booking History, Request Refund, Write Review, My Reviews, My Vouchers
- **Host Pages**: Host Dashboard, Add/Edit Condotel, Manage Vouchers, Manage Promotions, Service Package, Wallet, Payout, Reviews, Customers, Reports
- **Admin Pages**: Admin Dashboard, User Management, Refund Management, Payout Management, Package Management, Location/Resort Management, Blog Management

### 2. Main User Flows

#### A. Booking Flow (Tenant)
1. **Browse Condotels** (`/listing-stay`)
   - View list of condotels
   - Filter by price, location, beds, bathrooms
   - Search by name
   - View on map

2. **View Condotel Detail** (`/listing-stay-detail/:id`)
   - View condotel information
   - Check availability (date picker)
   - Select dates
   - View promotions
   - View reviews
   - Click "Đặt phòng" button

3. **Checkout** (`/checkout`)
   - Review booking summary
   - Select promotion (if available)
   - Enter voucher code
   - Select service packages
   - Choose to use reward points
   - Click "Xác nhận đặt phòng"

4. **Payment** (`/pay-done` or PayOS redirect)
   - View payment link/QR code
   - Complete payment on PayOS
   - Redirect to success/cancel page

5. **Booking History** (`/booking-history`)
   - View list of bookings
   - Click to view detail
   - Cancel booking
   - Request refund
   - Write review

#### B. Refund Flow (Tenant)
1. **Request Refund** (`/request-refund/:id`)
   - View booking information
   - Fill bank information form:
     - Select bank (VCB, MB, TCB, etc.)
     - Enter account number
     - Enter account holder name
   - Submit refund request
   - View confirmation message

#### C. Review Flow (Tenant)
1. **Write Review** (`/write-review/:id`)
   - View booking information
   - Select rating (1-5 stars)
   - Enter review title
   - Enter review comment
   - Submit review
   - View confirmation

2. **My Reviews** (`/my-reviews`)
   - View list of reviews
   - Edit review (within 7 days)
   - Delete review

#### D. Host Management Flow
1. **Host Dashboard** (`/host-dashboard`)
   - View condotels list
   - View bookings list
   - Manage condotels (add/edit/delete)
   - Manage bookings (update status)
   - View tabs: Condotels, Bookings, Promotions, Vouchers, Service Packages, Wallet, Payout, Reviews, Customers, Reports

2. **Add/Edit Condotel** (`/add-condotel`, `/edit-condotel/:id`)
   - Fill condotel form
   - Upload images
   - Select amenities
   - Set price
   - Save condotel

3. **Service Package** (`/subscription`)
   - View available packages
   - Purchase package
   - View my package
   - Check package limits

#### E. Admin Management Flow
1. **Admin Dashboard** (`/admin/*`)
   - View refund requests
   - Filter by status
   - Confirm refund
   - View pending payouts
   - Process payouts
   - Manage packages
   - Manage locations/resorts

## UI Components và Interactions

### 1. Date Picker
- Select check-in date
- Select check-out date
- Validation: cannot select past dates
- Validation: check-out must be after check-in

### 2. Promotion Selection
- View available promotions
- Select promotion
- View discount applied
- Validation: dates must be within promotion period

### 3. Voucher Input
- Enter voucher code
- Validate voucher
- Apply voucher discount
- Error handling: invalid voucher, expired voucher

### 4. Service Package Selection
- View available service packages
- Select packages with quantity
- Calculate total price

### 5. Bank Information Form
- Select bank from dropdown
- Enter account number
- Enter account holder name
- Validation: all fields required

### 6. Review Form
- Select rating (1-5 stars)
- Enter title
- Enter comment
- Submit review
- Edit review (within 7 days)

## API Integration Points

### Front-end → Back-end API Calls

1. **Booking APIs**
   - `GET /api/booking/my` - Get my bookings
   - `GET /api/booking/{id}` - Get booking by ID
   - `GET /api/booking/check-availability` - Check availability
   - `POST /api/booking` - Create booking
   - `PUT /api/booking/{id}` - Update booking
   - `DELETE /api/booking/{id}` - Cancel booking
   - `POST /api/booking/{id}/refund` - Request refund

2. **Payment APIs**
   - `POST /api/payment/create` - Create payment link
   - `GET /api/payment/status/{orderCode}` - Get payment status
   - `POST /api/payment/cancel/{orderCode}` - Cancel payment

3. **Review APIs**
   - `POST /api/tenant/reviews` - Create review
   - `GET /api/tenant/reviews` - Get my reviews
   - `PUT /api/tenant/reviews/{id}` - Update review
   - `PUT /api/host/review/{id}/reply` - Host reply to review

4. **Host APIs**
   - `GET /api/host/booking` - Get host bookings
   - `PUT /api/host/booking/{id}` - Update booking status
   - `GET /api/host/service-package/my` - Get my package
   - `POST /api/host/packages/purchase` - Purchase package

5. **Admin APIs**
   - `GET /api/admin/refund-requests` - Get refund requests
   - `POST /api/admin/refund-requests/{id}/confirm` - Confirm refund
   - `GET /api/admin/payouts/pending` - Get pending payouts
   - `POST /api/admin/payouts/{id}/confirm` - Process payout

## Test Cases Cần Bổ Sung

Dựa trên phân tích Front-end, các test cases sau cần được bổ sung:

### 1. UI/UX Test Cases
- Date picker validation
- Form validation (required fields, format)
- Error message display
- Success message display
- Loading states
- Navigation flows

### 2. Integration Test Cases
- Front-end API calls với Back-end
- Error handling từ Back-end
- Data transformation (PascalCase ↔ camelCase)
- Redirect flows (payment, refund, review)

### 3. End-to-End Test Cases
- Complete booking flow từ listing → detail → checkout → payment → booking history
- Complete refund flow từ booking history → request refund → admin confirm
- Complete review flow từ booking history → write review → view review

## Kết Luận

File CSV test cases cần được cập nhật với:
1. Chi tiết hơn về UI interactions (click button, fill form, select dropdown)
2. Test cases cho các màn hình Front-end cụ thể
3. Test cases cho các flows end-to-end
4. Test cases cho error handling và edge cases
5. Test cases cho validation ở Front-end và Back-end







