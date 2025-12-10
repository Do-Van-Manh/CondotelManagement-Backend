#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script to generate complete test cases CSV file with all missing test cases
"""
import csv
import os

# Read existing test cases
existing_tests = []
with open('TestCases_AllModules1.csv', 'r', encoding='utf-8') as f:
    reader = csv.DictReader(f)
    existing_tests = list(reader)

# Get existing test IDs
existing_ids = {test['Test Case ID'] for test in existing_tests}

# New test cases to add
new_test_cases = [
    # Auth - Missing test cases
    {
        'Test Case ID': 'TC-AUTH-014',
        'Function Name': 'Google Login',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Login with Google account',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/auth/google-login\n2. Set request body as raw JSON:\n{\n  "idToken": "google_id_token_string"\n}\n3. Click Send',
        'Expected Results': 'Response contains:\n- token: JWT token string\n- user: User profile object\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User has valid Google account'
    },
    {
        'Test Case ID': 'TC-AUTH-015',
        'Function Name': 'Verify OTP',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Verify OTP code',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/auth/verify-otp\n2. Set request body as raw JSON:\n{\n  "email": "user@test.com",\n  "otp": "123456"\n}\n3. Click Send',
        'Expected Results': 'Message = "OTP verified successfully."\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User has received OTP via email'
    },
    {
        'Test Case ID': 'TC-AUTH-016',
        'Function Name': 'Logout',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Logout user',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/auth/logout\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Message = "Logout successful"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in'
    },
    {
        'Test Case ID': 'TC-AUTH-017',
        'Function Name': 'Forgot Password',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Send forgot password email',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/auth/forgot-password\n2. Set request body as raw JSON:\n{\n  "email": "user@test.com"\n}\n3. Click Send',
        'Expected Results': 'Message = "If your email is registered, you will receive a password reset link."\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User exists and is active'
    },
    {
        'Test Case ID': 'TC-AUTH-018',
        'Function Name': 'Admin Check',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Check admin access',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/auth/admin-check\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Message = "Welcome, Admin!"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    },
    
    # Booking - Missing test cases
    {
        'Test Case ID': 'TC-BOOKING-011',
        'Function Name': 'Update Booking',
        'Sheet Name': 'Booking Management',
        'Test Case Description': 'Update booking details',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/booking/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "bookingId": 1,\n  "startDate": "2025-12-05",\n  "endDate": "2025-12-07"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated BookingDTO\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nBooking ID = 1 exists and belongs to user'
    },
    
    # Review - Missing test cases
    {
        'Test Case ID': 'TC-REVIEW-005',
        'Function Name': 'Get Review by ID',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Get review by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/reviews/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- data: ReviewDTO with full review details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nReview ID = 1 exists and belongs to user'
    },
    {
        'Test Case ID': 'TC-REVIEW-006',
        'Function Name': 'Update Review',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Update review within 7 days',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/tenant/reviews/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "reviewId": 1,\n  "rating": 4,\n  "comment": "Updated comment"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- message: "Review updated successfully"\n- data: Updated ReviewDTO\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nReview ID = 1 exists and belongs to user\nReview was created within 7 days'
    },
    {
        'Test Case ID': 'TC-REVIEW-007',
        'Function Name': 'Delete Review',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Delete review within 7 days',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/tenant/reviews/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- message: "Review deleted successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nReview ID = 1 exists and belongs to user\nReview was created within 7 days'
    },
    {
        'Test Case ID': 'TC-REVIEW-008',
        'Function Name': 'Get Reviews by Host',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Host get reviews for their condotels',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/review\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of ReviewDTO:\n- Only reviews for host\'s condotels\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has condotels with reviews'
    },
    {
        'Test Case ID': 'TC-REVIEW-009',
        'Function Name': 'Report Review',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Host report review',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/review/1/report\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- message: "Đã report review"\nStatus = 200\nSuccess = true\nReview status updated to "Reported" in database',
        'Pre-conditions': 'Host is logged in\nReview ID = 1 exists for host\'s condotel'
    },
    
    # Reward Points - Missing test cases
    {
        'Test Case ID': 'TC-REWARD-003',
        'Function Name': 'Get Points History',
        'Sheet Name': 'Reward Points Management',
        'Test Case Description': 'Get reward points history',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/rewards/history?page=1&pageSize=10\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- data: Array of RewardTransactionDTO\n- pagination: page, pageSize, totalCount, totalPages\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nUser has reward points history'
    },
    {
        'Test Case ID': 'TC-REWARD-004',
        'Function Name': 'Redeem Points',
        'Sheet Name': 'Reward Points Management',
        'Test Case Description': 'Redeem points for discount',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/tenant/rewards/redeem\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "pointsToRedeem": 5000,\n  "bookingId": 1\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- message: Redemption message\n- data: Redemption result\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nUser has at least 5000 points\nBooking ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-REWARD-005',
        'Function Name': 'Get Available Promotions',
        'Sheet Name': 'Reward Points Management',
        'Test Case Description': 'Get available reward promotions',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/rewards/promotions\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- data: Array of PromotionDTO\n- count: Number of promotions\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)'
    },
    {
        'Test Case ID': 'TC-REWARD-006',
        'Function Name': 'Validate Redeem Points',
        'Sheet Name': 'Reward Points Management',
        'Test Case Description': 'Validate if points can be redeemed',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/rewards/validate-redeem?points=5000\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- isValid: true/false\n- message: Validation message\n- discountAmount: Calculated discount\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)'
    },
    
    # Tenant Condotel - Missing test cases
    {
        'Test Case ID': 'TC-TENANT-005',
        'Function Name': 'Filter Condotel by Location',
        'Sheet Name': 'Condotel Management',
        'Test Case Description': 'Filter condotel by location',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/condotels?location=Hanoi\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of CondotelDTO:\n- All condotels have location containing "Hanoi"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Condotels with location "Hanoi" exist'
    },
    {
        'Test Case ID': 'TC-TENANT-006',
        'Function Name': 'Filter Condotel by Date Range',
        'Sheet Name': 'Condotel Management',
        'Test Case Description': 'Filter condotel by available date range',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/condotels?fromDate=2025-12-01&toDate=2025-12-05\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of CondotelDTO:\n- All condotels are available in date range\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Condotels available in date range exist'
    },
    {
        'Test Case ID': 'TC-TENANT-007',
        'Function Name': 'Filter Condotel by Beds',
        'Sheet Name': 'Condotel Management',
        'Test Case Description': 'Filter condotel by number of beds',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/condotels?beds=3\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of CondotelDTO:\n- All condotels have beds = 3\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Condotels with beds = 3 exist'
    },
    {
        'Test Case ID': 'TC-TENANT-008',
        'Function Name': 'Filter Condotel by Bathrooms',
        'Sheet Name': 'Condotel Management',
        'Test Case Description': 'Filter condotel by number of bathrooms',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/condotels?bathrooms=2\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of CondotelDTO:\n- All condotels have bathrooms = 2\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Condotels with bathrooms = 2 exist'
    },
    
    # Host Voucher - Missing test cases
    {
        'Test Case ID': 'TC-VOUCHER-004',
        'Function Name': 'Get Vouchers by Host',
        'Sheet Name': 'Voucher Management',
        'Test Case Description': 'Host get all vouchers',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/vouchers\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of VoucherDTO:\n- Only vouchers of current host\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has vouchers'
    },
    {
        'Test Case ID': 'TC-VOUCHER-005',
        'Function Name': 'Update Voucher',
        'Sheet Name': 'Voucher Management',
        'Test Case Description': 'Host update voucher',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/vouchers/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "code": "UPDATED2024",\n  "discountPercentage": 15,\n  "maxUses": 200,\n  "expiryDate": "2025-12-31T00:00:00Z"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated VoucherDTO\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nVoucher ID = 1 exists and belongs to host'
    },
    {
        'Test Case ID': 'TC-VOUCHER-006',
        'Function Name': 'Delete Voucher',
        'Sheet Name': 'Voucher Management',
        'Test Case Description': 'Host delete voucher',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/vouchers/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Status = 204\nSuccess = true\nVoucher deleted in database',
        'Pre-conditions': 'Host is logged in\nVoucher ID = 1 exists and belongs to host'
    },
    
    # Host Booking - Missing test cases
    {
        'Test Case ID': 'TC-HOST-BOOKING-001',
        'Function Name': 'Get Bookings by Host',
        'Sheet Name': 'Booking Management',
        'Test Case Description': 'Host get all bookings for their condotels',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/booking\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of BookingDTO:\n- Only bookings for host\'s condotels\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has condotels with bookings'
    },
    {
        'Test Case ID': 'TC-HOST-BOOKING-002',
        'Function Name': 'Get Bookings by Customer',
        'Sheet Name': 'Booking Management',
        'Test Case Description': 'Host get bookings by customer ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/booking/customer/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains HostBookingDTO:\n- Bookings for customer ID = 1 in host\'s condotels\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nCustomer ID = 1 has bookings in host\'s condotels'
    },
    
    # Host Customer - Missing test cases
    {
        'Test Case ID': 'TC-CUSTOMER-001',
        'Function Name': 'Get Customer Booked',
        'Sheet Name': 'Customer Management',
        'Test Case Description': 'Host get all customers who booked',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/customer\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of CustomerDTO:\n- All customers who booked host\'s condotels\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has bookings'
    },
    
    # Host Report - Missing test cases
    {
        'Test Case ID': 'TC-REPORT-001',
        'Function Name': 'Get Report',
        'Sheet Name': 'Report Management',
        'Test Case Description': 'Host get revenue and booking report',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/report?from=2025-01-01&to=2025-12-31\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains ReportDTO:\n- Revenue, booking statistics for date range\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has bookings in date range'
    },
    
    # Host Profile - Missing test cases
    {
        'Test Case ID': 'TC-HOST-PROFILE-001',
        'Function Name': 'Get Host Profile',
        'Sheet Name': 'Profile Management',
        'Test Case Description': 'Host get profile',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/profile\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains HostProfileDTO:\n- Full host profile information\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in'
    },
    {
        'Test Case ID': 'TC-HOST-PROFILE-002',
        'Function Name': 'Update Host Profile',
        'Sheet Name': 'Profile Management',
        'Test Case Description': 'Host update profile',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/profile\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "phoneContact": "0987654321",\n  "address": "Updated Address"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Profile updated successfully"\nStatus = 200\nSuccess = true\nHost profile updated in database',
        'Pre-conditions': 'Host is logged in'
    },
    
    # Host Location - Missing test cases
    {
        'Test Case ID': 'TC-LOCATION-003',
        'Function Name': 'Get Location by ID',
        'Sheet Name': 'Location Management',
        'Test Case Description': 'Get location by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/location/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains LocationDTO with full location details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nLocation ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-LOCATION-004',
        'Function Name': 'Update Location',
        'Sheet Name': 'Location Management',
        'Test Case Description': 'Host update location',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/location/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "name": "Updated Location",\n  "description": "Updated Description"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Location updated successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nLocation ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-LOCATION-005',
        'Function Name': 'Delete Location',
        'Sheet Name': 'Location Management',
        'Test Case Description': 'Host delete location',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/location/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- message: "Location deleted successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nLocation ID = 1 exists'
    },
    
    # Host Resort - Missing test cases
    {
        'Test Case ID': 'TC-RESORT-003',
        'Function Name': 'Get Resort by ID',
        'Sheet Name': 'Resort Management',
        'Test Case Description': 'Get resort by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/resorts/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains ResortDTO with full resort details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nResort ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-RESORT-004',
        'Function Name': 'Get Resorts by Location',
        'Sheet Name': 'Resort Management',
        'Test Case Description': 'Get resorts by location ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/resorts/location/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of ResortDTO:\n- All resorts in location ID = 1\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nLocation ID = 1 has resorts'
    },
    
    # Host Utility - Missing test cases
    {
        'Test Case ID': 'TC-UTILITY-003',
        'Function Name': 'Get Utility by ID',
        'Sheet Name': 'Utility Management',
        'Test Case Description': 'Get utility by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/utility/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains UtilityDTO with full utility details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nUtility ID = 1 exists and belongs to host'
    },
    {
        'Test Case ID': 'TC-UTILITY-004',
        'Function Name': 'Update Utility',
        'Sheet Name': 'Utility Management',
        'Test Case Description': 'Host update utility',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/utility/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "name": "Updated Utility",\n  "category": "Updated Category",\n  "description": "Updated Description"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Update successful"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nUtility ID = 1 exists and belongs to host'
    },
    {
        'Test Case ID': 'TC-UTILITY-005',
        'Function Name': 'Delete Utility',
        'Sheet Name': 'Utility Management',
        'Test Case Description': 'Host delete utility',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/utility/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- message: "Delete successful"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nUtility ID = 1 exists and belongs to host'
    },
    
    # Host Service Package - Missing test cases
    {
        'Test Case ID': 'TC-SERVICEPKG-003',
        'Function Name': 'Get Service Package by ID',
        'Sheet Name': 'Service Package Management',
        'Test Case Description': 'Get service package by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/service-packages/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains ServicePackageDTO with full details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nService Package ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-SERVICEPKG-004',
        'Function Name': 'Update Service Package',
        'Sheet Name': 'Service Package Management',
        'Test Case Description': 'Host update service package',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/service-packages/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "name": "Updated Service Package",\n  "description": "Updated Description",\n  "price": 200000,\n  "status": "Active"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated ServicePackageDTO\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nService Package ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-SERVICEPKG-005',
        'Function Name': 'Delete Service Package',
        'Sheet Name': 'Service Package Management',
        'Test Case Description': 'Host delete service package',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/service-packages/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- message: "Deleted successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nService Package ID = 1 exists'
    },
    
    # Host Package - Missing test cases
    {
        'Test Case ID': 'TC-HOST-PACKAGE-001',
        'Function Name': 'Get My Package',
        'Sheet Name': 'Host Package Management',
        'Test Case Description': 'Host get active package',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/packages/my-package\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains HostPackageDTO:\n- Active package details with startDate, endDate, status\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has active package'
    },
    {
        'Test Case ID': 'TC-HOST-PACKAGE-002',
        'Function Name': 'Purchase Package',
        'Sheet Name': 'Host Package Management',
        'Test Case Description': 'Host purchase package',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/host/packages/purchase\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "packageId": 1\n}\n4. Click Send',
        'Expected Results': 'Response contains purchase result:\n- Payment link or order information\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nPackage ID = 1 exists'
    },
    
    # Admin - Missing test cases
    {
        'Test Case ID': 'TC-ADMIN-006',
        'Function Name': 'Update User',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Admin update user',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/admin/users/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "fullName": "Updated Name",\n  "phone": "0987654321",\n  "roleId": 3\n}\n4. Click Send',
        'Expected Results': 'Response contains updated UserViewDTO\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nUser ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-ADMIN-007',
        'Function Name': 'Reset Password',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Admin reset user password',
        'Test Case Procedure': '1. Create a request in Postman with PATCH method to https://localhost:5000/api/admin/users/1/reset-password\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "newPassword": "NewPassword123!"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Đặt lại mật khẩu thành công"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nUser ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-ADMIN-008',
        'Function Name': 'Delete User',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Admin delete user (soft delete)',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/admin/users/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Status = 204\nSuccess = true\nUser status updated to "Deleted" in database',
        'Pre-conditions': 'Admin is logged in\nUser ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-ADMIN-009',
        'Function Name': 'Get Revenue Chart',
        'Sheet Name': 'Admin Dashboard Management',
        'Test Case Description': 'Get revenue chart data',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/dashboard/revenue/chart\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains revenue chart data:\n- Monthly/yearly revenue statistics\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    },
    {
        'Test Case ID': 'TC-ADMIN-010',
        'Function Name': 'Get Top Condotels',
        'Sheet Name': 'Admin Dashboard Management',
        'Test Case Description': 'Get top condotels by revenue',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/dashboard/top-condotels\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of top condotels:\n- Condotels sorted by revenue\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    },
    {
        'Test Case ID': 'TC-ADMIN-011',
        'Function Name': 'Get Tenant Analytics',
        'Sheet Name': 'Admin Dashboard Management',
        'Test Case Description': 'Get tenant analytics',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/dashboard/tenant-analytics\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains tenant analytics:\n- Active tenants, booking statistics, etc.\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    },
    {
        'Test Case ID': 'TC-ADMIN-012',
        'Function Name': 'Get Reported Reviews',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Admin get reported reviews',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/review/reported\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of ReviewDTO:\n- Only reviews with Status = "Reported"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nReported reviews exist'
    },
    {
        'Test Case ID': 'TC-ADMIN-013',
        'Function Name': 'Delete Review',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Admin delete review',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/admin/review/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- message: "Đã xóa review"\nStatus = 200\nSuccess = true\nReview status updated to "Deleted" in database',
        'Pre-conditions': 'Admin is logged in\nReview ID = 1 exists'
    },
    
    # Profile - Missing test cases (different endpoints)
    {
        'Test Case ID': 'TC-PROFILE-003',
        'Function Name': 'Get My Profile',
        'Sheet Name': 'Profile Management',
        'Test Case Description': 'Get my profile from profile endpoint',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/profile/me\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains UserProfileDto:\n- Full user profile information\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in'
    },
    {
        'Test Case ID': 'TC-PROFILE-004',
        'Function Name': 'Update Profile',
        'Sheet Name': 'Profile Management',
        'Test Case Description': 'Update profile from profile endpoint',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/profile/me\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "fullName": "Updated Name",\n  "phone": "0987654321",\n  "address": "Updated Address"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Profile updated successfully."\nStatus = 200\nSuccess = true\nUser profile updated in database',
        'Pre-conditions': 'User is logged in'
    },
    
    # Blog - Missing test cases
    {
        'Test Case ID': 'TC-BLOG-003',
        'Function Name': 'Get Blog Categories',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Get all blog categories',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/blog/categories\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of BlogCategoryDto:\n- List of all blog categories\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Blog categories exist in system'
    },
    
    # Admin Blog - Missing test cases
    {
        'Test Case ID': 'TC-ADMIN-BLOG-001',
        'Function Name': 'Get Post by ID',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin get blog post by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/blog/posts/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains BlogPostDetailDto:\n- Full post details including unpublished posts\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nBlog post ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-ADMIN-BLOG-002',
        'Function Name': 'Create Post',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin create blog post',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/admin/blog/posts\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "title": "New Post",\n  "content": "Post content",\n  "categoryId": 1,\n  "status": "Draft"\n}\n4. Click Send',
        'Expected Results': 'Response contains BlogPostDetailDto:\n- Created post with postId\nStatus = 201\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nCategory ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-ADMIN-BLOG-003',
        'Function Name': 'Update Post',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin update blog post',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/admin/blog/posts/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "title": "Updated Post",\n  "content": "Updated content",\n  "status": "Published"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated BlogPostDetailDto\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nBlog post ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-ADMIN-BLOG-004',
        'Function Name': 'Delete Post',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin delete blog post',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/admin/blog/posts/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- message: "Xóa bài viết thành công."\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nBlog post ID = 1 exists'
    },
    
    # Admin Blog Category - Missing test cases
    {
        'Test Case ID': 'TC-ADMIN-BLOG-CAT-001',
        'Function Name': 'Get Categories',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin get all blog categories',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/blog/categories\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of BlogCategoryDto:\n- List of all categories\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    },
    {
        'Test Case ID': 'TC-ADMIN-BLOG-CAT-002',
        'Function Name': 'Create Category',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin create blog category',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/admin/blog/categories\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "name": "New Category"\n}\n4. Click Send',
        'Expected Results': 'Response contains BlogCategoryDto:\n- Created category with categoryId\nStatus = 201\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    },
    {
        'Test Case ID': 'TC-ADMIN-BLOG-CAT-003',
        'Function Name': 'Update Category',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin update blog category',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/admin/blog/categories/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "name": "Updated Category"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated BlogCategoryDto\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nCategory ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-ADMIN-BLOG-CAT-004',
        'Function Name': 'Delete Category',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin delete blog category',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/admin/blog/categories/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- message: "Xóa danh mục thành công."\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nCategory ID = 1 exists'
    },
    
    # Promotion - Missing test cases
    {
        'Test Case ID': 'TC-PROMO-003',
        'Function Name': 'Get Promotion by ID',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Get promotion by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/promotion/1\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains PromotionDTO with full promotion details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Promotion ID = 1 exists'
    },
    {
        'Test Case ID': 'TC-PROMO-004',
        'Function Name': 'Get Promotions by Condotel',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Get promotions by condotel ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/promotions/condotel/1\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of PromotionDTO:\n- All promotions for condotel ID = 1\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Condotel ID = 1 has promotions'
    },
    {
        'Test Case ID': 'TC-PROMO-005',
        'Function Name': 'Get Promotions by Host',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Host get all promotions',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/promotions\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of PromotionDTO:\n- Only promotions of current host\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has promotions'
    },
    {
        'Test Case ID': 'TC-PROMO-006',
        'Function Name': 'Update Promotion',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Host update promotion',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/promotion/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "name": "Updated Promotion",\n  "discountPercentage": 30,\n  "startDate": "2025-12-01",\n  "endDate": "2025-12-31"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Promotion updated successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nPromotion ID = 1 exists and belongs to host'
    },
    {
        'Test Case ID': 'TC-PROMO-007',
        'Function Name': 'Delete Promotion',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Host delete promotion',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/promotion/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- message: "Promotion deleted successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nPromotion ID = 1 exists and belongs to host'
    },
    
    # Upload - Missing test cases
    {
        'Test Case ID': 'TC-UPLOAD-001',
        'Function Name': 'Upload Image',
        'Sheet Name': 'Upload Management',
        'Test Case Description': 'Upload image file',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/upload/image\n2. Set Body type to form-data\n3. Add key "file" with type File, select an image file\n4. Click Send',
        'Expected Results': 'Response contains:\n- imageUrl: Cloudinary URL\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Valid image file exists'
    },
    {
        'Test Case ID': 'TC-UPLOAD-002',
        'Function Name': 'Upload User Image',
        'Sheet Name': 'Upload Management',
        'Test Case Description': 'Upload user profile image',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/upload/user-image\n2. Set Authorization header: Bearer {token}\n3. Set Body type to form-data\n4. Add key "file" with type File, select an image file\n5. Click Send',
        'Expected Results': 'Response contains:\n- message: "Profile image updated successfully"\n- imageUrl: Cloudinary URL\nStatus = 200\nSuccess = true\nUser.ImageUrl updated in database',
        'Pre-conditions': 'User is logged in\nValid image file exists'
    },
    {
        'Test Case ID': 'TC-UPLOAD-003',
        'Function Name': 'Upload Condotel Image',
        'Sheet Name': 'Upload Management',
        'Test Case Description': 'Upload condotel image',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/upload/condotel/1/image\n2. Set Authorization header: Bearer {token}\n3. Set Body type to form-data\n4. Add key "file" with type File, select an image file\n5. Add key "caption" with value "Test Caption"\n6. Click Send',
        'Expected Results': 'Response contains:\n- message: "Condotel image uploaded successfully"\n- imageId: Image ID\n- imageUrl: Cloudinary URL\n- caption: "Test Caption"\nStatus = 200\nSuccess = true\nCondotelImage record created in database',
        'Pre-conditions': 'User is logged in\nCondotel ID = 1 exists\nValid image file exists'
    },
    {
        'Test Case ID': 'TC-UPLOAD-004',
        'Function Name': 'Create Condotel Detail',
        'Sheet Name': 'Upload Management',
        'Test Case Description': 'Create condotel detail',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/upload/condotel/1/detail\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "buildingName": "Building A",\n  "roomNumber": "101",\n  "beds": 2,\n  "bathrooms": 1,\n  "safetyFeatures": "Fire alarm",\n  "hygieneStandards": "Cleaned daily"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Condotel detail created successfully"\n- detailId: Detail ID\nStatus = 200\nSuccess = true\nCondotelDetail record created in database',
        'Pre-conditions': 'User is logged in\nCondotel ID = 1 exists'
    },
    
    # Package - Missing test cases
    {
        'Test Case ID': 'TC-PACKAGE-001',
        'Function Name': 'Get Available Packages',
        'Sheet Name': 'Host Package Management',
        'Test Case Description': 'Get available host packages',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/package\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of PackageDTO:\n- List of all available packages\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Packages exist in system'
    },
    {
        'Test Case ID': 'TC-PACKAGE-002',
        'Function Name': 'Confirm Package Payment',
        'Sheet Name': 'Host Package Management',
        'Test Case Description': 'Confirm package payment after PayOS callback',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/package/confirm-payment?orderCode=123456\n2. Click Send',
        'Expected Results': 'Response contains:\n- message: "THANH TOÁN THÀNH CÔNG! BẠN ĐÃ CHÍNH THỨC TRỞ THÀNH HOST!"\n- roleUpgraded: true\n- packageName: Package name\n- startDate: Start date\n- endDate: End date\nStatus = 200\nSuccess = true\nHostPackage status updated to "Active" in database\nUser role updated to Host',
        'Pre-conditions': 'Order code exists in HostPackages table\nPayment was successful'
    },
    
    # Host - Missing test cases
    {
        'Test Case ID': 'TC-HOST-REGISTER-001',
        'Function Name': 'Register as Host',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Register as host',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/host/register-as-host\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "phoneContact": "0123456789",\n  "address": "Host Address"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Chúc mừng! Bạn đã đăng ký Host thành công."\n- hostId: Host ID\nStatus = 200\nSuccess = true\nHost record created in database',
        'Pre-conditions': 'User is logged in\nUser does not have Host record'
    },
    
    # Host Condotel - Missing test cases
    {
        'Test Case ID': 'TC-HOST-004',
        'Function Name': 'Get Condotel by ID',
        'Sheet Name': 'Condotel Management',
        'Test Case Description': 'Host get condotel by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/condotel/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains CondotelDetailDTO with full condotel details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nCondotel ID = 1 exists and belongs to host'
    },
    {
        'Test Case ID': 'TC-HOST-005',
        'Function Name': 'Delete Condotel',
        'Sheet Name': 'Condotel Management',
        'Test Case Description': 'Host delete condotel',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/condotel/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- message: "Condotel deleted successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nCondotel ID = 1 exists and belongs to host'
    },
]

# Filter out test cases that already exist
new_test_cases_filtered = [tc for tc in new_test_cases if tc['Test Case ID'] not in existing_ids]

# Combine existing and new test cases
all_test_cases = existing_tests + new_test_cases_filtered

# Sort by Test Case ID
all_test_cases.sort(key=lambda x: x['Test Case ID'])

# Write to new file
output_file = 'TestCases_AllModules_Complete.csv'
fieldnames = ['Test Case ID', 'Function Name', 'Sheet Name', 'Test Case Description', 'Test Case Procedure', 'Expected Results', 'Pre-conditions']

with open(output_file, 'w', newline='', encoding='utf-8') as f:
    writer = csv.DictWriter(f, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(all_test_cases)

print(f'Generated {output_file} with {len(all_test_cases)} test cases')
print(f'Added {len(new_test_cases_filtered)} new test cases')
print(f'Existing test cases: {len(existing_tests)}')














