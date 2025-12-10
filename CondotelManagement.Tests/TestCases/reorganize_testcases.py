#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script to reorganize test cases according to new sheet structure
"""
import csv
import re
from collections import defaultdict

# Mapping từ function name cũ sang function name mới (tổng hợp)
FUNCTION_MAPPING = {
    # Account Management
    'Register': 'Register',
    'Register as Host': 'Register as Host',
    'Login': 'Login',
    'Google Login': 'Google Login',
    'Logout': 'Logout',
    'Verify Email': 'Verify OTP / Verify Email',
    'Verify OTP': 'Verify OTP / Verify Email',
    'Forgot Password': 'Forgot Password',
    'Reset Password with OTP': 'Reset Password with OTP',
    'Admin Reset Password': 'Admin Reset Password',
    'Admin Reset Password Not Found': 'Admin Reset Password',
    'Admin Reset Password Weak Password': 'Admin Reset Password',
    'Admin Check': 'Authorization / Admin Check',
    'authorization': 'Authorization / Admin Check',
    'Get Current User': 'Get Current User',
    'Update Profile': 'Update Profile',
    'Upload User Image': 'Upload User Image',
    'Get All Users': 'Get All Users',
    'Get User by ID': 'Get User by ID',
    'Get User by ID Not Found': 'Get User by ID',
    'Create User': 'Create User',
    'Create User Duplicate Email': 'Create User',
    'Create User Invalid Data': 'Create User',
    'Update User': 'Update User',
    'Update User Invalid Data': 'Update User',
    'Update User Not Found': 'Update User',
    'Update User Status': 'Update User Status',
    'Update User Status Invalid Status': 'Update User Status',
    'Update User Status Not Found': 'Update User Status',
    'Delete User': 'Update User Status',  # Có thể là deactivate
    'Get My Reward Points': 'Get My Reward Points',
    'Get Points History': 'Get Points History',
    'Redeem Points': 'Redeem Points',
    'Validate Redeem Points': 'Validate Redeem Points',
    'Calculate Discount from Points': 'Validate Redeem Points',
    
    # Booking Management
    'Check Availability': 'Check Availability',
    'Create Booking': 'Create Booking',
    'Update Booking': 'Update Booking',
    'Update Booking Invalid Data': 'Update Booking',
    'Update Booking Not Found': 'Update Booking',
    'Cancel Booking': 'Cancel Booking',
    'Get My Bookings': 'Get My Bookings (Get Bookings by Customer)',
    'Get Bookings by Customer': 'Get My Bookings (Get Bookings by Customer)',
    'Get Bookings by Customer Not Found': 'Get My Bookings (Get Bookings by Customer)',
    'Get Bookings by Host': 'Get Bookings by Host',
    'Get Customer Booked': 'Get Customer Booked',
    'Get Booking by ID': 'Get Booking by ID',
    'Get Booking by ID Not Found': 'Get Booking by ID',
    'Create Payment Link': 'Create Payment Link',
    'Get Payment Status': 'Get Payment Status',
    'Cancel Payment': 'Cancel Payment',
    'Test PayOS Connection': 'Test PayOS Connection',
    
    # Condotel Management
    'Create Condotel': 'Create Condotel',
    'Create Condotel Invalid Data': 'Create Condotel',
    'Create Condotel Detail': 'Create Condotel Detail',
    'Update Condotel': 'Update Condotel',
    'Update Condotel Invalid Data': 'Update Condotel',
    'Update Condotel Not Found': 'Update Condotel',
    'Upload Condotel Image': 'Upload Condotel Image',
    'Get All Condotels': 'Get All Condotels',
    'View All Condotels': 'Get All Condotels',
    'Get Condotel Details': 'Get Condotel Details',
    'View Condotel Details': 'Get Condotel Details',
    'Get Condotel by ID': 'Get Condotel Details',
    'Get Condotel by ID Not Found': 'Get Condotel Details',
    'Search Condotel by Name': 'Search Condotel by Name',
    'Filter Condotel by Price': 'Filter Condotel by Price',
    'Filter Condotel by Location': 'Filter Condotel by Location',
    'Filter Condotel by Date Range': 'Filter Condotel by Date Range',
    'Filter Condotel by Beds and Bathrooms': 'Filter Condotel by Beds and Bathrooms',
    'Delete Condotel': 'Update Condotel',  # Có thể là soft delete
    'Delete Condotel Not Found': 'Update Condotel',
    
    # Communication Management
    'Create Review': 'Create Review',
    'Create Review Invalid Data': 'Create Review',
    'Update Review': 'Update Review',
    'Update Review After 7 Days': 'Update Review',
    'Update Review Not Found': 'Update Review',
    'Delete Review': 'Delete Review',
    'Delete Review After 7 Days': 'Delete Review',
    'Delete Review Not Found': 'Delete Review',
    'Get My Reviews': 'Get My Reviews',
    'Get Review by ID': 'Get Review by ID',
    'Get Review by ID Not Found': 'Get Review by ID',
    'Host Get Reviews': 'Get My Reviews',  # Host xem reviews của condotel
    'Host Reply to Review': 'Host Reply to Review',
    'Host Reply Review Invalid Data': 'Host Reply to Review',
    'Host Reply Review Not Found': 'Host Reply to Review',
    'Host Report Review': 'Host Report Review',
    'Host Report Review Not Found': 'Host Report Review',
    'Get Reported Reviews': 'Get Reported Reviews',
    'Get Reported Reviews Empty': 'Get Reported Reviews',
    'Get Conversations': 'Get Conversations',
    'Get Conversations Not Found': 'Get Conversations',
    'Get Messages': 'Get Messages',
    'Get Messages Not Found': 'Get Messages',
    'Send Direct Message': 'Send Direct Message',
    'Send Direct Message Invalid Data': 'Send Direct Message',
    'Send Direct Message Non-existent Users': 'Send Direct Message',
    
    # Marketing Management - Blog
    'Get Published Blog Posts': 'Get Published Blog Posts',
    'Get Blog Post by ID': 'Get Blog Post by ID',
    'Get Blog Post by ID Not Found': 'Get Blog Post by ID',
    'Get Blog Post by Slug': 'Get Blog Post by ID',
    'Create Blog Post': 'Create Blog Post',
    'Create Blog Post Invalid Data': 'Create Blog Post',
    'Create Blog Post Invalid Category': 'Create Blog Post',
    'Update Blog Post': 'Update Blog Post',
    'Update Blog Post Invalid Data': 'Update Blog Post',
    'Update Blog Post Not Found': 'Update Blog Post',
    'Delete Blog Post': 'Delete Blog Post',
    'Delete Blog Post Not Found': 'Delete Blog Post',
    'Get Blog Categories': 'Get Blog Categories',
    'Get Blog Categories Empty': 'Get Blog Categories',
    'Create Blog Category': 'Create / Update / Delete Blog Category',
    'Create Blog Category Duplicate Name': 'Create / Update / Delete Blog Category',
    'Create Blog Category Invalid Data': 'Create / Update / Delete Blog Category',
    'Update Blog Category': 'Create / Update / Delete Blog Category',
    'Update Blog Category Invalid Data': 'Create / Update / Delete Blog Category',
    'Update Blog Category Not Found': 'Create / Update / Delete Blog Category',
    'Delete Blog Category': 'Create / Update / Delete Blog Category',
    'Delete Blog Category Not Found': 'Create / Update / Delete Blog Category',
    
    # Marketing Management - Promotion
    'Get All Promotions': 'Get All Promotions',
    'Get Available Promotions': 'Get Available Promotions',
    'Get Promotion by ID': 'Get Promotion by ID',
    'Get Promotion by ID Not Found': 'Get Promotion by ID',
    'Get Promotions by Condotel': 'Get Promotions by Condotel / by Host',
    'Get Promotions by Condotel Not Found': 'Get Promotions by Condotel / by Host',
    'Get Promotions by Host': 'Get Promotions by Condotel / by Host',
    'Create Promotion': 'Create Promotion',
    'Create Promotion Invalid Data': 'Create Promotion',
    'Update Promotion': 'Update Promotion',
    'Update Promotion Invalid Data': 'Update Promotion',
    'Update Promotion Not Found': 'Update Promotion',
    'Delete Promotion': 'Delete Promotion',
    'Delete Promotion Not Found': 'Delete Promotion',
    
    # Marketing Management - Voucher
    'Create Voucher': 'Create Voucher',
    'Create Voucher Invalid Data': 'Create Voucher',
    'Update Voucher': 'Update Voucher',
    'Update Voucher Invalid Data': 'Update Voucher',
    'Update Voucher Not Found': 'Update Voucher',
    'Delete Voucher': 'Delete Voucher',
    'Delete Voucher Not Found': 'Delete Voucher',
    'Get Vouchers by Host': 'Get Vouchers by Host',
    'Get Voucher by ID': 'View Condotel Vouchers',  # Có thể là view voucher của condotel
    'Get Voucher by ID Not Found': 'View Condotel Vouchers',
    'View Condotel Vouchers': 'View Condotel Vouchers',
    
    # Master Data Management - Location
    'Get All Locations': 'Get All Locations',
    'Get Location by ID': 'Get Location by ID',
    'Get Location by ID Not Found': 'Get Location by ID',
    'Create Location': 'Create Location',
    'Create Location Invalid Data': 'Create Location',
    'Update Location': 'Update Location',
    'Update Location Invalid Data': 'Update Location',
    'Update Location Not Found': 'Update Location',
    'Delete Location': 'Delete Location',
    'Delete Location Not Found': 'Delete Location',
    
    # Master Data Management - Resort
    'Get All Resorts': 'Get All Resorts',
    'Get Resort by ID': 'Get Resort by ID',
    'Get Resort by ID Not Found': 'Get Resort by ID',
    'Get Resorts by Location': 'Get Resort by ID',  # Có thể là filter
    'Get Resorts by Location Not Found': 'Get Resort by ID',
    'Create Resort': 'Create Resort',
    'Create Resort Invalid Data': 'Create Resort',
    
    # Master Data Management - Utility
    'Get Utilities': 'Get Utilities',
    'Get Utility by ID': 'Get Utility by ID',
    'Get Utility by ID Not Found': 'Get Utility by ID',
    'Create Utility': 'Create Utility',
    'Create Utility Invalid Data': 'Create Utility',
    'Update Utility': 'Update Utility',
    'Update Utility Invalid Data': 'Update Utility',
    'Update Utility Not Found': 'Update Utility',
    'Delete Utility': 'Delete Utility',
    'Delete Utility Not Found': 'Delete Utility',
    
    # Service Management
    'Get Service Packages': 'Get Service Packages',
    'Get Available Packages': 'Get Available Packages',
    'Get Service Package by ID': 'Get Service Package by ID',
    'Get Service Package by ID Not Found': 'Get Service Package by ID',
    'Create Service Package': 'Create Service Package',
    'Create Service Package Invalid Data': 'Create Service Package',
    'Update Service Package': 'Update Service Package',
    'Update Service Package Invalid Data': 'Update Service Package',
    'Update Service Package Not Found': 'Update Service Package',
    'Delete Service Package': 'Delete Service Package',
    'Delete Service Package Not Found': 'Delete Service Package',
    'Get My Package': 'Get My Package',
    'Purchase Package': 'Purchase Package',
    'Create Package Payment': 'Create Package Payment',
    'Confirm Package Payment': 'Confirm Package Payment',
    
    # Dashboard Management
    'Get Dashboard Overview': 'Get Dashboard Overview',
    'Get Dashboard Overview Unauthorized': 'Get Dashboard Overview',
    'Get Revenue Chart': 'Get Revenue Chart',
    'Get Revenue Chart Unauthorized': 'Get Revenue Chart',
    'Get Top Condotels': 'Get Top Condotels',
    'Get Top Condotels Unauthorized': 'Get Top Condotels',
    'Get Tenant Analytics': 'Get Tenant Analytics',
    'Get Tenant Analytics Unauthorized': 'Get Tenant Analytics',
    'Get Report': 'Get Dashboard Overview',  # Có thể là dashboard report
    
    # Other functions
    'Upload Image': 'Upload User Image',  # Có thể là upload user image
    'Get My Profile': 'Get Current User',
    'Get Host Profile Not Found': 'Get Current User',
    'Update Host Profile': 'Update Profile',
    'Update Host Profile Invalid Data': 'Update Profile',
    'Update Host Profile Not Found': 'Update Profile',
    'belongs to current user': 'Get Current User',  # Authorization check
}

# Mapping từ Sheet Name cũ sang Sheet Name mới
SHEET_MAPPING = {
    'Account Management': 'AccountManagement',
    'Booking Management': 'BookingManagement',
    'Condotel Management': 'CondotelManagement',
    'Review Management': 'CommunicationManagement',
    'Chat Management': 'CommunicationManagement',
    'Communication Management': 'CommunicationManagement',
    'Blog Management': 'MarketingManagement',
    'Promotion Management': 'MarketingManagement',
    'Voucher Management': 'MarketingManagement',
    'Marketing Management': 'MarketingManagement',
    'Location Management': 'MasterDataManagement',
    'Resort Management': 'MasterDataManagement',
    'Utility Management': 'MasterDataManagement',
    'Master Data Management': 'MasterDataManagement',
    'Service Package Management': 'ServiceManagement',
    'Service Management': 'ServiceManagement',
    'Dashboard Management': 'DashboardManagement',
    'Admin Management': 'AccountManagement',  # Admin functions thuộc Account Management
    'Profile Management': 'AccountManagement',  # Profile thuộc Account Management
    'Reward Points Management': 'AccountManagement',  # Reward points thuộc Account Management
    'Payment Management': 'BookingManagement',  # Payment thuộc Booking Management
    'Host Management': 'CondotelManagement',  # Host condotel management
    'Tenant Management': 'CondotelManagement',  # Tenant view condotels
}

def normalize_function_name(func_name):
    """Normalize function name theo mapping"""
    # Loại bỏ các suffix như "Not Found", "Invalid Data", etc.
    base_name = func_name
    for old_name, new_name in FUNCTION_MAPPING.items():
        if old_name == func_name:
            return new_name
    # Nếu không tìm thấy, giữ nguyên nhưng loại bỏ suffix
    if ' Not Found' in base_name:
        base_name = base_name.replace(' Not Found', '')
    if ' Invalid Data' in base_name:
        base_name = base_name.replace(' Invalid Data', '')
    if ' Unauthorized' in base_name:
        base_name = base_name.replace(' Unauthorized', '')
    if ' Empty' in base_name:
        base_name = base_name.replace(' Empty', '')
    if ' After 7 Days' in base_name:
        base_name = base_name.replace(' After 7 Days', '')
    if ' Duplicate Email' in base_name:
        base_name = base_name.replace(' Duplicate Email', '')
    if ' Weak Password' in base_name:
        base_name = base_name.replace(' Weak Password', '')
    if ' Invalid Category' in base_name:
        base_name = base_name.replace(' Invalid Category', '')
    if ' Duplicate Name' in base_name:
        base_name = base_name.replace(' Duplicate Name', '')
    if ' Non-existent Users' in base_name:
        base_name = base_name.replace(' Non-existent Users', '')
    return base_name

def normalize_sheet_name(sheet_name):
    """Normalize sheet name theo mapping"""
    return SHEET_MAPPING.get(sheet_name, sheet_name)

def read_test_cases(filename):
    """Đọc test cases từ CSV file"""
    test_cases = []
    with open(filename, 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            test_cases.append(row)
    return test_cases

def reorganize_test_cases(test_cases):
    """Tổ chức lại test cases theo sheet và function mới"""
    reorganized = []
    
    for tc in test_cases:
        # Lấy function name và sheet name
        old_func = tc.get('Function Name', '').strip()
        old_sheet = tc.get('Sheet Name', '').strip()
        
        # Normalize
        new_func = normalize_function_name(old_func)
        new_sheet = normalize_sheet_name(old_sheet)
        
        # Tạo test case mới
        new_tc = tc.copy()
        new_tc['Function Name'] = new_func
        new_tc['Sheet Name'] = new_sheet
        
        reorganized.append(new_tc)
    
    return reorganized

def write_test_cases(filename, test_cases):
    """Ghi test cases ra CSV file"""
    if not test_cases:
        return
    
    fieldnames = ['Test Case ID', 'Function Name', 'Sheet Name', 'Test Case Description', 
                  'Test Case Procedure', 'Expected Results', 'Pre-conditions']
    
    with open(filename, 'w', encoding='utf-8', newline='') as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(test_cases)

def main():
    input_file = 'TestCases_AllModules7.csv'
    output_file = 'TestCases_Reorganized.csv'
    
    print(f"Đang đọc file {input_file}...")
    test_cases = read_test_cases(input_file)
    print(f"Đã đọc {len(test_cases)} test cases")
    
    print("Đang tổ chức lại test cases...")
    reorganized = reorganize_test_cases(test_cases)
    
    print(f"Đang ghi file {output_file}...")
    write_test_cases(output_file, reorganized)
    
    # Thống kê
    sheet_stats = defaultdict(int)
    func_stats = defaultdict(int)
    for tc in reorganized:
        sheet_stats[tc['Sheet Name']] += 1
        func_stats[tc['Function Name']] += 1
    
    print("\n=== THỐNG KÊ ===")
    print("\nTheo Sheet:")
    for sheet, count in sorted(sheet_stats.items()):
        print(f"  {sheet}: {count} test cases")
    
    print("\nTop 20 Function Names:")
    for func, count in sorted(func_stats.items(), key=lambda x: x[1], reverse=True)[:20]:
        print(f"  {func}: {count} test cases")
    
    print(f"\nĐã tạo file {output_file} thành công!")

if __name__ == '__main__':
    main()













