#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script để tạo file Excel cho System Test Cases
Dựa trên template Google Sheets
"""

import csv
from datetime import datetime

# Định nghĩa test cases
test_cases = [
    {
        "scenario": "Scenario A: Authentication & Tenant Booking",
        "id": "SYS-001",
        "description": "Complete Tenant Booking Flow - Test luồng hoàn chỉnh từ đăng ký tenant đến đặt phòng",
        "procedure": """1. Register new tenant với email, password, fullName, phone
2. Verify email với OTP từ database
3. Login với credentials đã verify
4. View condotels (public endpoint, không cần auth)
5. View condotel detail theo ID
6. Check availability với checkIn và checkOut dates
7. Create booking với condotelId, startDate, endDate
8. Get my bookings để verify booking đã được tạo""",
        "expected_results": """1. Register response: 201 Created
2. Verify email response: 200 OK, user status = Active
3. Login response: 200 OK, có token và user info
4. Condotels list: 200 OK, danh sách condotel không rỗng
5. Condotel detail: 200 OK, có đầy đủ thông tin
6. Availability check: 200 OK, available = true
7. Booking created: 200/201 OK, bookingId được trả về
8. My bookings: 200 OK, danh sách bookings có ít nhất 1 item""",
        "preconditions": "Database đã được seed với test data (Location, Resort, Condotel)"
    },
    {
        "scenario": "Scenario A: Authentication & Tenant Booking",
        "id": "SYS-002",
        "description": "Complete Host Registration Flow - Test luồng đăng ký Host và quản lý Condotel",
        "procedure": """1. Register new user với email, password, fullName, phone
2. Verify email với OTP
3. Login với credentials
4. Register as Host với companyName, taxCode, address
5. Create Wallet (bank account) với bankName, accountNumber, accountHolderName
6. Get wallets để verify wallet đã được tạo
7. Create Condotel với name, description, resortId, pricePerNight, beds, bathrooms
8. Get my condotels để verify condotel đã được tạo""",
        "expected_results": """1. Register response: 201 Created
2. Verify email response: 200 OK
3. Login response: 200 OK, có token
4. Host register response: 200 OK, Host record được tạo
5. Wallet created: 200/201 OK, walletId được trả về
6. Wallets list: 200 OK, có ít nhất 1 wallet
7. Condotel created: 200/201 OK hoặc 403 nếu vượt quá package limit
8. My condotels: 200 OK, danh sách condotels""",
        "preconditions": "Database đã được seed với test data. User phải có package active để tạo condotel"
    },
    {
        "scenario": "Scenario A: Authentication & Tenant Booking",
        "id": "SYS-003",
        "description": "Complete Booking with Payment Flow - Test luồng tạo booking và thanh toán",
        "procedure": """1. Login as tenant với credentials
2. Create booking với condotelId, startDate, endDate
3. Create payment link (PayOS) với bookingId
4. Verify booking status sau khi tạo payment link""",
        "expected_results": """1. Login response: 200 OK, có token
2. Booking created: 200/201 OK, bookingId được trả về
3. Payment link: 200 OK (hoặc 400/500 nếu PayOS chưa được cấu hình)
4. Booking detail: 200 OK, booking status = Pending hoặc Confirmed""",
        "preconditions": "Tenant đã login. Condotel đã tồn tại và available. PayOS service có thể chưa được cấu hình (acceptable trong test)"
    },
    {
        "scenario": "Scenario B: Review & Communication",
        "id": "SYS-004",
        "description": "Complete Review Flow - Test luồng review từ tenant và reply từ host",
        "procedure": """1. Create completed booking trong database (status = Completed)
2. Tenant login và tạo review với bookingId, condotelId, rating (1-5), comment
3. Get reviews của tenant để verify review đã được tạo
4. Host login và reply review với reviewId và reply message
5. Verify reply đã được lưu trong database""",
        "expected_results": """1. Booking created với status = Completed
2. Review created: 200 OK, reviewId được trả về
3. Reviews list: 200 OK, có ít nhất 1 review
4. Reply response: 200 OK, review có reply
5. Database: Review.Reply không null và có nội dung""",
        "preconditions": "Booking đã completed. Tenant và Host đã có accounts"
    },
    {
        "scenario": "Scenario B: Review & Communication",
        "id": "SYS-005",
        "description": "Complete Package Purchase Flow - Test luồng Host mua package dịch vụ",
        "procedure": """1. Host login với credentials
2. Get available packages từ service-package/available endpoint
3. Get my current package từ service-package/my endpoint""",
        "expected_results": """1. Login response: 200 OK, có token
2. Available packages: 200 OK, danh sách packages (có thể rỗng)
3. My package: 200 OK, package info hoặc null nếu chưa có package""",
        "preconditions": "Host đã login. Service packages có thể đã được seed trong database"
    },
    {
        "scenario": "Scenario C: Wallet & Payout",
        "id": "SYS-006",
        "description": "Complete Wallet and Payout Flow - Test luồng tạo wallet và xử lý payout",
        "procedure": """1. Host login
2. Create wallet với bankName, accountNumber, accountHolderName
3. Get wallets để verify
4. Create completed booking (>= 15 days ago) trong database
5. Admin login và process payout với bookingId""",
        "expected_results": """1. Login response: 200 OK
2. Wallet created: 200/201 OK
3. Wallets list: 200 OK, có ít nhất 1 wallet
4. Booking created với status = Completed, IsPaidToHost = false
5. Payout response: 200 OK hoặc 400/404 nếu cần setup thêm""",
        "preconditions": "Host và Admin đã có accounts. Booking đã completed >= 15 ngày"
    },
    {
        "scenario": "Scenario C: Wallet & Payout",
        "id": "SYS-007",
        "description": "Complete Admin Management Flow - Test luồng quản lý của Admin",
        "procedure": """1. Admin login
2. Get all users từ admin/users endpoint
3. Get user by ID từ admin/users/{id}
4. Get all locations từ admin/location
5. Get all resorts từ admin/resort
6. Get dashboard overview từ admin/dashboard/overview
7. Get revenue chart từ admin/dashboard/revenue/chart""",
        "expected_results": """1. Login response: 200 OK, có token
2. Users list: 200 OK, danh sách users
3. User detail: 200 OK, user info
4. Locations list: 200 OK
5. Resorts list: 200 OK
6. Dashboard: 200 OK, có overview data (totalUsers, totalBookings, etc.)
7. Revenue chart: 200 OK, có revenue data""",
        "preconditions": "Admin đã login. Database đã được seed với test data"
    },
    {
        "scenario": "Scenario D: Security & Authorization",
        "id": "SYS-008",
        "description": "Authorization and Security Flow - Test các luồng bảo mật và phân quyền",
        "procedure": """1. Access protected endpoint (/api/booking/my) without token → Expect 401
2. Login as Tenant và access Host endpoint (/api/host/condotel) → Expect 403
3. Create booking của user khác và try to delete → Expect 403/404
4. Admin login và access admin endpoints → Expect 200 OK""",
        "expected_results": """1. Unauthorized response: 401 Unauthorized
2. Forbidden response: 403 Forbidden (wrong role)
3. Forbidden/NotFound: 403 hoặc 404 (ownership check)
4. Admin access: 200 OK (correct role)""",
        "preconditions": "Test data đã được seed. Có Tenant, Host, Admin accounts"
    },
    {
        "scenario": "Scenario D: Security & Authorization",
        "id": "SYS-009",
        "description": "Complete Search and Filter Flow - Test luồng tìm kiếm và lọc Condotel",
        "procedure": """1. Search condotels by name với query parameter ?name=Test
2. Filter by price với ?minPrice=50000&maxPrice=150000
3. Filter by beds and bathrooms với ?beds=2&bathrooms=1
4. Filter by location với ?locationId=1
5. Combined filters với nhiều parameters cùng lúc""",
        "expected_results": """1. Search by name: 200 OK, danh sách condotels match name
2. Filter by price: 200 OK, condotels trong price range
3. Filter by beds/bathrooms: 200 OK, condotels match criteria
4. Filter by location: 200 OK, condotels ở location đó
5. Combined filters: 200 OK, condotels match tất cả criteria""",
        "preconditions": "Database đã được seed với condotels có các attributes khác nhau"
    },
    {
        "scenario": "Scenario E: Voucher & Promotion",
        "id": "SYS-010",
        "description": "Complete Voucher Flow - Test luồng Host tạo voucher và Tenant sử dụng",
        "procedure": """1. Host login
2. Host creates voucher với code, condotelId, discountPercentage, maxUses, expiryDate
3. Get vouchers by host
4. Public view vouchers for condotel (không cần auth)
5. Tenant login và create booking (voucher support depends on implementation)""",
        "expected_results": """1. Login response: 200 OK
2. Voucher created: 200 OK, voucherId được trả về
3. Host vouchers: 200 OK, danh sách vouchers
4. Condotel vouchers: 200 OK, danh sách vouchers available
5. Booking created: 200/201 OK (voucher có thể được apply nếu supported)""",
        "preconditions": "Host đã login. Condotel đã tồn tại"
    },
    {
        "scenario": "Scenario E: Voucher & Promotion",
        "id": "SYS-011",
        "description": "Complete Authentication Flow - Test luồng authentication hoàn chỉnh",
        "procedure": """1. Register new user
2. Verify email với OTP từ database
3. Login với verified account
4. Get current user info từ /api/auth/me
5. Forgot password flow: Send OTP
6. Reset password với OTP và new password
7. Login với new password""",
        "expected_results": """1. Register: 201 Created, user status = Pending
2. Verify email: 200 OK, user status = Active
3. Login: 200 OK, có token và user info
4. Get me: 200 OK, user profile info
5. Send OTP: 200 OK, OTP được gửi
6. Reset password: 200 OK, password được cập nhật
7. Login với new password: 200 OK, có token""",
        "preconditions": "Database sẵn sàng. Email service được mock"
    },
    {
        "scenario": "Scenario F: Refund & Cancellation",
        "id": "SYS-012",
        "description": "Complete Refund Request Flow - Test luồng Tenant yêu cầu refund và Admin xử lý",
        "procedure": """1. Create confirmed booking trong database
2. Tenant login và request refund với bookingId, bankCode, accountNumber, accountHolder
3. Verify refund request was created trong database
4. Admin login và view refund requests
5. Admin approve/reject refund (nếu có endpoint)""",
        "expected_results": """1. Booking created với status = Confirmed
2. Refund request: 200 OK hoặc 400 BadRequest
3. Database: RefundRequest được tạo với status = Pending
4. Refunds list: 200 OK hoặc 404 nếu chưa có
5. Approve/Reject: 200 OK (nếu endpoint được implement)""",
        "preconditions": "Tenant và Admin đã có accounts. Booking đã confirmed"
    },
    {
        "scenario": "Scenario E: Voucher & Promotion",
        "id": "SYS-013",
        "description": "Complete Promotion Flow - Test luồng Host tạo promotion và Tenant xem",
        "procedure": """1. Host login
2. Host creates promotion với condotelId, discountPercentage, startDate, endDate, description
3. Get promotions by host
4. Public view promotions for condotel (không cần auth)
5. Tenant view condotel detail (promotions sẽ được hiển thị)""",
        "expected_results": """1. Login response: 200 OK
2. Promotion created: 200/201 OK hoặc 400 BadRequest
3. Host promotions: 200 OK, danh sách promotions
4. Condotel promotions: 200 OK, danh sách promotions available
5. Condotel detail: 200 OK, có promotions info""",
        "preconditions": "Host đã login. Condotel đã tồn tại"
    },
    {
        "scenario": "Scenario G: Package Management",
        "id": "SYS-014",
        "description": "Complete Package Limit Enforcement Flow - Test việc enforce giới hạn số lượng condotel theo package",
        "procedure": """1. Host login
2. Check current package từ service-package/my
3. Get current condotel count từ host/condotel
4. Try to create condotel
5. Verify response (403 nếu vượt quá giới hạn, 200/201 nếu trong giới hạn)
6. Verify condotel count sau khi tạo""",
        "expected_results": """1. Login: 200 OK
2. My package: 200 OK, package info
3. Condotels count: 200 OK, số lượng hiện tại
4. Create condotel: 200/201 OK nếu trong limit, 403 Forbidden nếu vượt limit
5. Updated count: 200 OK, count tăng lên nếu tạo thành công""",
        "preconditions": "Host đã login. Package đã được assign với giới hạn cụ thể"
    },
    {
        "scenario": "Scenario A: Authentication & Tenant Booking",
        "id": "SYS-015",
        "description": "Complete Multi-Step Booking with Voucher Flow - Test luồng phức tạp từ tìm kiếm đến đặt phòng với voucher",
        "procedure": """1. Search condotels với query parameters
2. View condotel detail theo ID
3. View vouchers for condotel (public)
4. Tenant login
5. Check availability với checkIn và checkOut dates
6. Create booking (voucher support depends on implementation)
7. Verify booking was created trong my bookings""",
        "expected_results": """1. Search: 200 OK, danh sách condotels
2. Detail: 200 OK, đầy đủ thông tin condotel
3. Vouchers: 200 OK, danh sách vouchers available
4. Login: 200 OK, có token
5. Availability: 200 OK, available = true/false
6. Booking: 200/201 OK, bookingId được trả về
7. My bookings: 200 OK, có ít nhất 1 booking""",
        "preconditions": "Database đã được seed. Condotel và vouchers đã tồn tại"
    }
]

def create_csv_file():
    """Tạo file CSV với format phù hợp cho Excel"""
    filename = "System_Test_Cases_Template.csv"
    
    with open(filename, 'w', newline='', encoding='utf-8-sig') as csvfile:
        writer = csv.writer(csvfile)
        
        # Header - Metadata
        writer.writerow([])
        writer.writerow(['Workflow', 'Condotel Management System - Main Workflows'])
        writer.writerow(['Test requirement', 'Test các luồng chính của hệ thống Condotel Management end-to-end, bao gồm Authentication, Booking, Host Management, Payment, Review, và các chức năng khác'])
        writer.writerow(['Number of TCs', len(test_cases)])
        writer.writerow([])
        
        # Testing Round Summary
        writer.writerow(['Testing Round Summary'])
        writer.writerow(['', 'Passed', 'Failed', 'Pending', 'N/A'])
        writer.writerow(['Round 1', '0', '0', str(len(test_cases)), '0'])
        writer.writerow(['Round 2', '0', '0', str(len(test_cases)), '0'])
        writer.writerow(['Round 3', '0', '0', str(len(test_cases)), '0'])
        writer.writerow([])
        
        # Test Case Details Header
        writer.writerow(['Test Case ID', 'Test Case Description', 'Test Case Procedure', 'Expected Results', 'Pre-conditions'])
        
        # Group test cases by scenario
        current_scenario = None
        for tc in test_cases:
            if tc['scenario'] != current_scenario:
                current_scenario = tc['scenario']
                # Write scenario header (with empty row before)
                writer.writerow([])
                writer.writerow([current_scenario, '', '', '', ''])
            
            # Write test case
            writer.writerow([
                tc['id'],
                tc['description'],
                tc['procedure'],
                tc['expected_results'],
                tc['preconditions']
            ])
    
    print(f"✅ Đã tạo file: {filename}")
    print(f"📊 Tổng số test cases: {len(test_cases)}")
    print(f"📝 File có thể mở bằng Excel hoặc Google Sheets")

if __name__ == "__main__":
    create_csv_file()





