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

# New test cases to add - Missing endpoints
new_test_cases = []

# Auth - Missing test cases
if 'TC-AUTH-014' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-AUTH-014',
        'Function Name': 'Google Login',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Login with Google account',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/auth/google-login\n2. Set request body as raw JSON:\n{\n  "idToken": "google-id-token-here"\n}\n3. Click Send',
        'Expected Results': 'Response contains:\n- token: JWT token string\n- user: User profile object\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User has Google account'
    })

if 'TC-AUTH-015' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-AUTH-015',
        'Function Name': 'Verify OTP',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Verify OTP code',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/auth/verify-otp\n2. Set request body as raw JSON:\n{\n  "email": "tenant@test.com",\n  "otp": "123456"\n}\n3. Click Send',
        'Expected Results': 'Message = "OTP verified successfully."\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User has received OTP via email'
    })

if 'TC-AUTH-016' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-AUTH-016',
        'Function Name': 'Logout',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Logout user',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/auth/logout\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Message = "Logout successful"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in'
    })

if 'TC-AUTH-017' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-AUTH-017',
        'Function Name': 'Forgot Password',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Request password reset',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/auth/forgot-password\n2. Set request body as raw JSON:\n{\n  "email": "tenant@test.com"\n}\n3. Click Send',
        'Expected Results': 'Message = "If your email is registered, you will receive a password reset link."\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User exists and is active'
    })

if 'TC-AUTH-018' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-AUTH-018',
        'Function Name': 'Admin Check',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Check admin access',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/auth/admin-check\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Message = "Welcome, Admin!"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    })

# Booking - Missing test cases
if 'TC-BOOKING-011' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-BOOKING-011',
        'Function Name': 'Update Booking',
        'Sheet Name': 'Booking Management',
        'Test Case Description': 'Update booking details',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/booking/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "bookingId": 1,\n  "condotelId": 1,\n  "startDate": "2025-12-02",\n  "endDate": "2025-12-04"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated BookingDTO\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nBooking ID = 1 exists and belongs to current user'
    })

# Tenant Review - Missing test cases
if 'TC-REVIEW-005' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REVIEW-005',
        'Function Name': 'Get Review by ID',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Get review by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/reviews/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- data: ReviewDTO with full review details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nReview ID = 1 exists and belongs to current user'
    })

if 'TC-REVIEW-006' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REVIEW-006',
        'Function Name': 'Update Review',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Update review within 7 days',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/tenant/reviews/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "reviewId": 1,\n  "rating": 4,\n  "comment": "Updated review comment"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- message: "Review updated successfully"\n- data: Updated ReviewDTO\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nReview ID = 1 exists, belongs to current user, and created within 7 days'
    })

if 'TC-REVIEW-007' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REVIEW-007',
        'Function Name': 'Delete Review',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Delete review within 7 days',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/tenant/reviews/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- message: "Review deleted successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nReview ID = 1 exists, belongs to current user, and created within 7 days'
    })

if 'TC-REVIEW-008' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REVIEW-008',
        'Function Name': 'Host Get Reviews',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Host get all reviews for their condotels',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/review\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of ReviewDTO:\n- Only reviews for condotels belonging to current host\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has condotels with reviews'
    })

if 'TC-REVIEW-009' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REVIEW-009',
        'Function Name': 'Host Report Review',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Host report inappropriate review',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/review/1/report\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Message = "Đã report review"\nStatus = 200\nSuccess = true\nReview status updated to "Reported" in database',
        'Pre-conditions': 'Host is logged in\nReview ID = 1 exists for host\'s condotel'
    })

# Tenant Reward - Missing test cases
if 'TC-REWARD-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REWARD-003',
        'Function Name': 'Get Points History',
        'Sheet Name': 'Reward Points Management',
        'Test Case Description': 'Get reward points history',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/rewards/history?page=1&pageSize=10\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- data: Array of reward history records\n- pagination: page, pageSize, totalCount, totalPages\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nUser has reward points history'
    })

if 'TC-REWARD-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REWARD-004',
        'Function Name': 'Redeem Points',
        'Sheet Name': 'Reward Points Management',
        'Test Case Description': 'Redeem points for booking discount',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/tenant/rewards/redeem\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "bookingId": 1,\n  "pointsToRedeem": 5000\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- message: Success message\n- data: Redemption result with discount applied\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nUser has at least 5000 points\nBooking ID = 1 exists with Status = "Pending"'
    })

if 'TC-REWARD-005' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REWARD-005',
        'Function Name': 'Get Available Promotions',
        'Sheet Name': 'Reward Points Management',
        'Test Case Description': 'Get available promotions for user',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/rewards/promotions\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- data: Array of PromotionDTO\n- count: Number of promotions\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nPromotions exist in system'
    })

if 'TC-REWARD-006' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REWARD-006',
        'Function Name': 'Validate Redeem Points',
        'Sheet Name': 'Reward Points Management',
        'Test Case Description': 'Validate if points can be redeemed',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/rewards/validate-redeem?points=5000\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- isValid: true/false\n- message: Validation message\n- discountAmount: Calculated discount amount\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)'
    })

# Tenant Condotel - Missing filter test cases
if 'TC-TENANT-005' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-TENANT-005',
        'Function Name': 'Filter Condotel by Location',
        'Sheet Name': 'Condotel Management',
        'Test Case Description': 'Filter condotels by location',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/condotels?location=Da Nang\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of CondotelDTO:\n- All condotels have location containing "Da Nang"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Condotels with location "Da Nang" exist'
    })

if 'TC-TENANT-006' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-TENANT-006',
        'Function Name': 'Filter Condotel by Date Range',
        'Sheet Name': 'Condotel Management',
        'Test Case Description': 'Filter condotels by available date range',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/condotels?fromDate=2025-12-01&toDate=2025-12-05\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of CondotelDTO:\n- All condotels are available in date range\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Condotels available in date range exist'
    })

if 'TC-TENANT-007' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-TENANT-007',
        'Function Name': 'Filter Condotel by Beds and Bathrooms',
        'Sheet Name': 'Condotel Management',
        'Test Case Description': 'Filter condotels by number of beds and bathrooms',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/tenant/condotels?beds=3&bathrooms=2\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of CondotelDTO:\n- All condotels have beds = 3 and bathrooms = 2\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Condotels with beds=3 and bathrooms=2 exist'
    })

# Host Voucher - Missing test cases
if 'TC-VOUCHER-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-VOUCHER-004',
        'Function Name': 'Get Vouchers by Host',
        'Sheet Name': 'Voucher Management',
        'Test Case Description': 'Host get all their vouchers',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/vouchers\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of VoucherDTO:\n- Only vouchers belonging to current host\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has vouchers'
    })

if 'TC-VOUCHER-005' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-VOUCHER-005',
        'Function Name': 'Update Voucher',
        'Sheet Name': 'Voucher Management',
        'Test Case Description': 'Host update voucher',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/vouchers/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "code": "UPDATED2024",\n  "discountPercentage": 15,\n  "maxUses": 200,\n  "expiryDate": "2025-12-31T00:00:00Z"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated VoucherDTO\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nVoucher ID = 1 exists and belongs to host'
    })

if 'TC-VOUCHER-006' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-VOUCHER-006',
        'Function Name': 'Delete Voucher',
        'Sheet Name': 'Voucher Management',
        'Test Case Description': 'Host delete voucher',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/vouchers/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Status = 204\nSuccess = true\nVoucher deleted from database',
        'Pre-conditions': 'Host is logged in\nVoucher ID = 1 exists and belongs to host'
    })

# Host Booking - Missing test cases
if 'TC-BOOKING-012' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-BOOKING-012',
        'Function Name': 'Get Bookings by Host',
        'Sheet Name': 'Booking Management',
        'Test Case Description': 'Host get all bookings for their condotels',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/booking\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of HostBookingDTO:\n- Only bookings for condotels belonging to current host\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has condotels with bookings'
    })

if 'TC-BOOKING-013' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-BOOKING-013',
        'Function Name': 'Get Bookings by Customer',
        'Sheet Name': 'Booking Management',
        'Test Case Description': 'Host get bookings by customer ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/booking/customer/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains HostBookingDTO:\n- Only bookings for customer ID = 1 in host\'s condotels\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nCustomer ID = 1 has bookings in host\'s condotels'
    })

# Host Customer - Missing test cases
if 'TC-CUSTOMER-001' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-CUSTOMER-001',
        'Function Name': 'Get Customer Booked',
        'Sheet Name': 'Customer Management',
        'Test Case Description': 'Host get all customers who booked their condotels',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/customer\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of CustomerDTO:\n- List of customers who have bookings in host\'s condotels\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has bookings'
    })

# Host Report - Missing test cases
if 'TC-REPORT-001' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-REPORT-001',
        'Function Name': 'Get Report',
        'Sheet Name': 'Report Management',
        'Test Case Description': 'Host get revenue report',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/report?from=2025-01-01&to=2025-12-31\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains report data:\n- Revenue, bookings count, statistics for date range\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has bookings in date range'
    })

# Host Location - Missing test cases
if 'TC-LOCATION-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-LOCATION-003',
        'Function Name': 'Get Location by ID',
        'Sheet Name': 'Location Management',
        'Test Case Description': 'Get location by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/location/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains LocationDTO with full location details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nLocation ID = 1 exists'
    })

if 'TC-LOCATION-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-LOCATION-004',
        'Function Name': 'Update Location',
        'Sheet Name': 'Location Management',
        'Test Case Description': 'Host update location',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/location/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "name": "Updated Location",\n  "description": "Updated Description"\n}\n4. Click Send',
        'Expected Results': 'Message = "Location updated successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nLocation ID = 1 exists'
    })

if 'TC-LOCATION-005' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-LOCATION-005',
        'Function Name': 'Delete Location',
        'Sheet Name': 'Location Management',
        'Test Case Description': 'Host delete location',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/location/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Message = "Location deleted successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nLocation ID = 1 exists'
    })

# Host Resort - Missing test cases
if 'TC-RESORT-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-RESORT-003',
        'Function Name': 'Get Resort by ID',
        'Sheet Name': 'Resort Management',
        'Test Case Description': 'Get resort by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/resorts/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains ResortDTO with full resort details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nResort ID = 1 exists'
    })

if 'TC-RESORT-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-RESORT-004',
        'Function Name': 'Get Resorts by Location',
        'Sheet Name': 'Resort Management',
        'Test Case Description': 'Get resorts by location ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/resorts/location/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of ResortDTO:\n- Only resorts in location ID = 1\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nLocation ID = 1 has resorts'
    })

# Host Utility - Missing test cases
if 'TC-UTILITY-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-UTILITY-003',
        'Function Name': 'Get Utility by ID',
        'Sheet Name': 'Utility Management',
        'Test Case Description': 'Get utility by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/utility/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains UtilityDTO with full utility details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nUtility ID = 1 exists and belongs to host'
    })

if 'TC-UTILITY-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-UTILITY-004',
        'Function Name': 'Update Utility',
        'Sheet Name': 'Utility Management',
        'Test Case Description': 'Host update utility',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/utility/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "name": "Updated Utility",\n  "category": "Updated Category",\n  "description": "Updated Description"\n}\n4. Click Send',
        'Expected Results': 'Message = "Update successful"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nUtility ID = 1 exists and belongs to host'
    })

if 'TC-UTILITY-005' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-UTILITY-005',
        'Function Name': 'Delete Utility',
        'Sheet Name': 'Utility Management',
        'Test Case Description': 'Host delete utility',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/utility/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Message = "Delete successful"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nUtility ID = 1 exists and belongs to host'
    })

# Host Service Package - Missing test cases
if 'TC-SERVICEPKG-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-SERVICEPKG-003',
        'Function Name': 'Get Service Package by ID',
        'Sheet Name': 'Service Package Management',
        'Test Case Description': 'Get service package by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/service-packages/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains ServicePackageDTO with full details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nService Package ID = 1 exists'
    })

if 'TC-SERVICEPKG-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-SERVICEPKG-004',
        'Function Name': 'Update Service Package',
        'Sheet Name': 'Service Package Management',
        'Test Case Description': 'Host update service package',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/service-packages/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "name": "Updated Service Package",\n  "description": "Updated Description",\n  "price": 200000,\n  "status": "Active"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated ServicePackageDTO\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nService Package ID = 1 exists'
    })

if 'TC-SERVICEPKG-005' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-SERVICEPKG-005',
        'Function Name': 'Delete Service Package',
        'Sheet Name': 'Service Package Management',
        'Test Case Description': 'Host delete service package',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/service-packages/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Message = "Deleted successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nService Package ID = 1 exists'
    })

# Host Package - Missing test cases
if 'TC-HOSTPKG-001' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-HOSTPKG-001',
        'Function Name': 'Get My Package',
        'Sheet Name': 'Host Package Management',
        'Test Case Description': 'Get current host package subscription',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/packages/my-package\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains HostPackageDTO:\n- Current active package information or null if no package\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in'
    })

if 'TC-HOSTPKG-002' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-HOSTPKG-002',
        'Function Name': 'Purchase Package',
        'Sheet Name': 'Host Package Management',
        'Test Case Description': 'Host purchase or upgrade package',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/host/packages/purchase\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "packageId": 1\n}\n4. Click Send',
        'Expected Results': 'Response contains package purchase result:\n- OrderCode, payment link, etc.\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nPackage ID = 1 exists'
    })

# Admin - Missing test cases
if 'TC-ADMIN-006' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMIN-006',
        'Function Name': 'Admin Reset Password',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Admin reset user password',
        'Test Case Procedure': '1. Create a request in Postman with PATCH method to https://localhost:5000/api/admin/users/1/reset-password\n2. Set Authorization header: Bearer {admin-token}\n3. Set request body as raw JSON:\n{\n  "newPassword": "NewPassword123!"\n}\n4. Click Send',
        'Expected Results': 'Message = "Đặt lại mật khẩu thành công"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nUser ID = 1 exists'
    })

if 'TC-ADMIN-007' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMIN-007',
        'Function Name': 'Get Dashboard Overview',
        'Sheet Name': 'Admin Dashboard Management',
        'Test Case Description': 'Get dashboard overview statistics',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/dashboard/overview\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Response contains DashboardOverviewDto:\n- totalUsers, totalBookings, totalRevenue, etc.\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    })

if 'TC-ADMIN-008' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMIN-008',
        'Function Name': 'Get Revenue Chart',
        'Sheet Name': 'Admin Dashboard Management',
        'Test Case Description': 'Get revenue chart data',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/dashboard/revenue/chart\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Response contains revenue chart data\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    })

if 'TC-ADMIN-009' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMIN-009',
        'Function Name': 'Get Top Condotels',
        'Sheet Name': 'Admin Dashboard Management',
        'Test Case Description': 'Get top condotels by bookings',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/dashboard/top-condotels\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Response contains array of top condotels data\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    })

if 'TC-ADMIN-010' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMIN-010',
        'Function Name': 'Get Tenant Analytics',
        'Sheet Name': 'Admin Dashboard Management',
        'Test Case Description': 'Get tenant analytics data',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/dashboard/tenant-analytics\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Response contains tenant analytics data\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    })

if 'TC-ADMIN-011' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMIN-011',
        'Function Name': 'Get Reported Reviews',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Admin get reported reviews',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/review/reported\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Response contains array of ReviewDTO:\n- Only reviews with Status = "Reported"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nReported reviews exist'
    })

if 'TC-ADMIN-012' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMIN-012',
        'Function Name': 'Delete Review',
        'Sheet Name': 'Review Management',
        'Test Case Description': 'Admin delete review',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/admin/review/1\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Message = "Đã xóa review"\nStatus = 200\nSuccess = true\nReview status updated to "Deleted" in database',
        'Pre-conditions': 'Admin is logged in\nReview ID = 1 exists'
    })

# Admin Blog - Missing test cases
if 'TC-ADMINBLOG-001' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMINBLOG-001',
        'Function Name': 'Get Blog Post by ID',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin get blog post by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/blog/posts/1\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Response contains BlogPostDetailDto with full post details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nBlog Post ID = 1 exists'
    })

if 'TC-ADMINBLOG-002' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMINBLOG-002',
        'Function Name': 'Create Blog Post',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin create blog post',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/admin/blog/posts\n2. Set Authorization header: Bearer {admin-token}\n3. Set request body as raw JSON:\n{\n  "title": "New Blog Post",\n  "slug": "new-blog-post",\n  "content": "Blog post content",\n  "categoryId": 1,\n  "status": "Published"\n}\n4. Click Send',
        'Expected Results': 'Response contains created BlogPostDetailDto\nStatus = 201\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nCategory ID = 1 exists'
    })

if 'TC-ADMINBLOG-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMINBLOG-003',
        'Function Name': 'Update Blog Post',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin update blog post',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/admin/blog/posts/1\n2. Set Authorization header: Bearer {admin-token}\n3. Set request body as raw JSON:\n{\n  "title": "Updated Blog Post",\n  "content": "Updated content",\n  "status": "Published"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated BlogPostDetailDto\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nBlog Post ID = 1 exists'
    })

if 'TC-ADMINBLOG-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMINBLOG-004',
        'Function Name': 'Delete Blog Post',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin delete blog post',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/admin/blog/posts/1\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Message = "Xóa bài viết thành công."\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nBlog Post ID = 1 exists'
    })

# Admin Blog Category - Missing test cases
if 'TC-ADMINBLOGCAT-001' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMINBLOGCAT-001',
        'Function Name': 'Get Blog Categories',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin get all blog categories',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/admin/blog/categories\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Response contains array of BlogCategoryDto\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    })

if 'TC-ADMINBLOGCAT-002' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMINBLOGCAT-002',
        'Function Name': 'Create Blog Category',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin create blog category',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/admin/blog/categories\n2. Set Authorization header: Bearer {admin-token}\n3. Set request body as raw JSON:\n{\n  "name": "New Category"\n}\n4. Click Send',
        'Expected Results': 'Response contains created BlogCategoryDto\nStatus = 201\nSuccess = true',
        'Pre-conditions': 'Admin is logged in'
    })

if 'TC-ADMINBLOGCAT-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMINBLOGCAT-003',
        'Function Name': 'Update Blog Category',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin update blog category',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/admin/blog/categories/1\n2. Set Authorization header: Bearer {admin-token}\n3. Set request body as raw JSON:\n{\n  "name": "Updated Category"\n}\n4. Click Send',
        'Expected Results': 'Response contains updated BlogCategoryDto\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nCategory ID = 1 exists'
    })

if 'TC-ADMINBLOGCAT-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-ADMINBLOGCAT-004',
        'Function Name': 'Delete Blog Category',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Admin delete blog category',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/admin/blog/categories/1\n2. Set Authorization header: Bearer {admin-token}\n3. Click Send',
        'Expected Results': 'Message = "Xóa danh mục thành công."\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Admin is logged in\nCategory ID = 1 exists'
    })

# Payment - Missing test cases
if 'TC-PAYMENT-005' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PAYMENT-005',
        'Function Name': 'Get Payment Status',
        'Sheet Name': 'Payment Management',
        'Test Case Description': 'Get payment status by payment link ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/payment/payos/status/{paymentLinkId}\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- data: { status, amount, amountPaid, amountRemaining, orderCode, transactions }\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nPayment link ID exists'
    })

if 'TC-PAYMENT-006' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PAYMENT-006',
        'Function Name': 'Cancel Payment',
        'Sheet Name': 'Payment Management',
        'Test Case Description': 'Cancel payment link',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/payment/payos/cancel/{paymentLinkId}\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "reason": "User cancelled"\n}\n4. Click Send',
        'Expected Results': 'Message = "Payment cancelled successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in (Tenant role)\nPayment link ID exists and is pending'
    })

if 'TC-PAYMENT-007' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PAYMENT-007',
        'Function Name': 'Create Package Payment',
        'Sheet Name': 'Payment Management',
        'Test Case Description': 'Create payment link for package purchase',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/payment/create-package-payment\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "orderCode": "1234567890123456",\n  "amount": 500000,\n  "description": "Package upgrade"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- success: true\n- data: { checkoutUrl, qrCode, orderCode, paymentLinkId }\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in\nAmount >= 10000 VND'
    })

if 'TC-PAYMENT-008' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PAYMENT-008',
        'Function Name': 'Test PayOS Connection',
        'Sheet Name': 'Payment Management',
        'Test Case Description': 'Test PayOS API connection',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/payment/payos/test\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains:\n- success: true\n- message: "PayOS configuration loaded"\n- config: PayOS configuration details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'PayOS is configured'
    })

# Package - Missing test cases
if 'TC-PACKAGE-001' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PACKAGE-001',
        'Function Name': 'Get Available Packages',
        'Sheet Name': 'Host Package Management',
        'Test Case Description': 'Get available packages',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/package\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of PackageDTO:\n- List of available packages\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Packages exist in system'
    })

if 'TC-PACKAGE-002' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PACKAGE-002',
        'Function Name': 'Confirm Package Payment',
        'Sheet Name': 'Host Package Management',
        'Test Case Description': 'Confirm package payment after PayOS callback',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/package/confirm-payment?orderCode=1234567890123456\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains:\n- message: Success message\n- roleUpgraded: true\n- packageName, startDate, endDate, duration\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Package order with orderCode exists\nPayment is successful'
    })

# Upload - Missing test cases
if 'TC-UPLOAD-001' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-UPLOAD-001',
        'Function Name': 'Upload Image',
        'Sheet Name': 'Upload Management',
        'Test Case Description': 'Upload image to Cloudinary',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/upload/image\n2. Set Body to form-data\n3. Add key "file" with type File, select an image file\n4. Click Send (no authentication required)',
        'Expected Results': 'Response contains:\n- imageUrl: Cloudinary image URL\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Valid image file exists'
    })

if 'TC-UPLOAD-002' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-UPLOAD-002',
        'Function Name': 'Upload User Image',
        'Sheet Name': 'Upload Management',
        'Test Case Description': 'Upload user profile image',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/upload/user-image\n2. Set Authorization header: Bearer {token}\n3. Set Body to form-data\n4. Add key "file" with type File, select an image file\n5. Click Send',
        'Expected Results': 'Response contains:\n- message: "Profile image updated successfully"\n- imageUrl: Cloudinary image URL\nStatus = 200\nSuccess = true\nUser.ImageUrl updated in database',
        'Pre-conditions': 'User is logged in\nValid image file exists'
    })

if 'TC-UPLOAD-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-UPLOAD-003',
        'Function Name': 'Upload Condotel Image',
        'Sheet Name': 'Upload Management',
        'Test Case Description': 'Upload condotel image',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/upload/condotel/1/image\n2. Set Authorization header: Bearer {token}\n3. Set Body to form-data\n4. Add key "file" with type File, select an image file\n5. Add key "caption" with value "Test caption" (optional)\n6. Click Send',
        'Expected Results': 'Response contains:\n- message: "Condotel image uploaded successfully"\n- imageId: Image ID\n- imageUrl: Cloudinary image URL\n- caption: Image caption\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in\nCondotel ID = 1 exists\nValid image file exists'
    })

if 'TC-UPLOAD-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-UPLOAD-004',
        'Function Name': 'Create Condotel Detail',
        'Sheet Name': 'Upload Management',
        'Test Case Description': 'Create condotel detail record',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/upload/condotel/1/detail\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "buildingName": "Building A",\n  "roomNumber": "101",\n  "beds": 2,\n  "bathrooms": 1,\n  "safetyFeatures": "Fire alarm",\n  "hygieneStandards": "Daily cleaning",\n  "status": "Active"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Condotel detail created successfully"\n- detailId: Detail ID\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in\nCondotel ID = 1 exists'
    })

# Blog - Missing test cases
if 'TC-BLOG-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-BLOG-003',
        'Function Name': 'Get Blog Categories',
        'Sheet Name': 'Blog Management',
        'Test Case Description': 'Get all blog categories',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/blog/categories\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of BlogCategoryDto\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Blog categories exist'
    })

# Promotion - Missing test cases
if 'TC-PROMO-003' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PROMO-003',
        'Function Name': 'Get Promotion by ID',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Get promotion by ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/promotion/1\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains PromotionDTO with full details\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Promotion ID = 1 exists'
    })

if 'TC-PROMO-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PROMO-004',
        'Function Name': 'Get Promotions by Condotel',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Get promotions by condotel ID',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/promotions/condotel/1\n2. Click Send (no authentication required)',
        'Expected Results': 'Response contains array of PromotionDTO:\n- Only promotions for condotel ID = 1\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Condotel ID = 1 has promotions'
    })

if 'TC-PROMO-005' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PROMO-005',
        'Function Name': 'Get Promotions by Host',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Host get all their promotions',
        'Test Case Procedure': '1. Create a request in Postman with GET method to https://localhost:5000/api/host/promotions\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Response contains array of PromotionDTO:\n- Only promotions belonging to current host\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nHost has promotions'
    })

if 'TC-PROMO-006' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PROMO-006',
        'Function Name': 'Update Promotion',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Host update promotion',
        'Test Case Procedure': '1. Create a request in Postman with PUT method to https://localhost:5000/api/host/promotion/1\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "condotelId": 1,\n  "name": "Updated Promotion",\n  "discountPercentage": 30,\n  "startDate": "2025-12-01",\n  "endDate": "2025-12-31",\n  "status": "Active"\n}\n4. Click Send',
        'Expected Results': 'Message = "Promotion updated successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nPromotion ID = 1 exists and belongs to host'
    })

if 'TC-PROMO-007' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-PROMO-007',
        'Function Name': 'Delete Promotion',
        'Sheet Name': 'Promotion Management',
        'Test Case Description': 'Host delete promotion',
        'Test Case Procedure': '1. Create a request in Postman with DELETE method to https://localhost:5000/api/host/promotion/1\n2. Set Authorization header: Bearer {token}\n3. Click Send',
        'Expected Results': 'Message = "Promotion deleted successfully"\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'Host is logged in\nPromotion ID = 1 exists and belongs to host'
    })

# Host Registration - Missing test cases
if 'TC-HOST-004' not in existing_ids:
    new_test_cases.append({
        'Test Case ID': 'TC-HOST-004',
        'Function Name': 'Register as Host',
        'Sheet Name': 'Account Management',
        'Test Case Description': 'Register user as host',
        'Test Case Procedure': '1. Create a request in Postman with POST method to https://localhost:5000/api/host/register-as-host\n2. Set Authorization header: Bearer {token}\n3. Set request body as raw JSON:\n{\n  "phoneContact": "0123456789",\n  "address": "Host Address"\n}\n4. Click Send',
        'Expected Results': 'Response contains:\n- message: "Chúc mừng! Bạn đã đăng ký Host thành công."\n- hostId: Host ID\nStatus = 200\nSuccess = true',
        'Pre-conditions': 'User is logged in\nUser does not have host account'
    })

# Write all test cases to new CSV file
all_test_cases = existing_tests + new_test_cases

# Write to new CSV file
with open('TestCases_AllModules_Complete.csv', 'w', encoding='utf-8', newline='') as f:
    fieldnames = ['Test Case ID', 'Function Name', 'Sheet Name', 'Test Case Description', 'Test Case Procedure', 'Expected Results', 'Pre-conditions']
    writer = csv.DictWriter(f, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(all_test_cases)

print(f'Total test cases: {len(all_test_cases)}')
print(f'Existing test cases: {len(existing_tests)}')
print(f'New test cases added: {len(new_test_cases)}')
print(f'New file created: TestCases_AllModules_Complete.csv')














