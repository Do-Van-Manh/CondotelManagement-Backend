# Script to reorganize test cases CSV file - Version 2
# Sử dụng cách tiếp cận đọc file thủ công để xử lý CSV có multiline fields

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$inputFile = Join-Path $scriptPath "TestCases_AllModules7.csv"
$outputFile = Join-Path $scriptPath "TestCases_Reorganized.csv"

# Function mapping (giống như script trước)
$functionMapping = @{
    "Register" = "Register"
    "Register as Host" = "Register as Host"
    "Login" = "Login"
    "Google Login" = "Google Login"
    "Logout" = "Logout"
    "Verify Email" = "Verify OTP / Verify Email"
    "Verify OTP" = "Verify OTP / Verify Email"
    "Forgot Password" = "Forgot Password"
    "Reset Password with OTP" = "Reset Password with OTP"
    "Admin Reset Password" = "Admin Reset Password"
    "Admin Reset Password Not Found" = "Admin Reset Password"
    "Admin Reset Password Weak Password" = "Admin Reset Password"
    "Admin Check" = "Authorization / Admin Check"
    "authorization" = "Authorization / Admin Check"
    "Get Current User" = "Get Current User"
    "Update Profile" = "Update Profile"
    "Upload User Image" = "Upload User Image"
    "Get All Users" = "Get All Users"
    "Get User by ID" = "Get User by ID"
    "Get User by ID Not Found" = "Get User by ID"
    "Create User" = "Create User"
    "Create User Duplicate Email" = "Create User"
    "Create User Invalid Data" = "Create User"
    "Update User" = "Update User"
    "Update User Not Found" = "Update User"
    "Update User Invalid Data" = "Update User"
    "Update User Status" = "Update User Status"
    "Update User Status Invalid Status" = "Update User Status"
    "Update User Status Not Found" = "Update User Status"
    "Delete User" = "Update User Status"
    "Get My Reward Points" = "Get My Reward Points"
    "Get Points History" = "Get Points History"
    "Redeem Points" = "Redeem Points"
    "Validate Redeem Points" = "Validate Redeem Points"
    "Calculate Discount from Points" = "Validate Redeem Points"
    "Check Availability" = "Check Availability"
    "Create Booking" = "Create Booking"
    "Update Booking" = "Update Booking"
    "Update Booking Invalid Data" = "Update Booking"
    "Update Booking Not Found" = "Update Booking"
    "Cancel Booking" = "Cancel Booking"
    "Get My Bookings" = "Get My Bookings (Get Bookings by Customer)"
    "Get Bookings by Customer" = "Get My Bookings (Get Bookings by Customer)"
    "Get Bookings by Customer Not Found" = "Get My Bookings (Get Bookings by Customer)"
    "Get Bookings by Host" = "Get Bookings by Host"
    "Get Customer Booked" = "Get Customer Booked"
    "Get Booking by ID" = "Get Booking by ID"
    "Get Booking by ID Not Found" = "Get Booking by ID"
    "Create Payment Link" = "Create Payment Link"
    "Get Payment Status" = "Get Payment Status"
    "Cancel Payment" = "Cancel Payment"
    "Test PayOS Connection" = "Test PayOS Connection"
    "Create Condotel" = "Create Condotel"
    "Create Condotel Invalid Data" = "Create Condotel"
    "Create Condotel Detail" = "Create Condotel Detail"
    "Update Condotel" = "Update Condotel"
    "Update Condotel Invalid Data" = "Update Condotel"
    "Update Condotel Not Found" = "Update Condotel"
    "Upload Condotel Image" = "Upload Condotel Image"
    "Get All Condotels" = "Get All Condotels"
    "View All Condotels" = "Get All Condotels"
    "Get Condotel Details" = "Get Condotel Details"
    "View Condotel Details" = "Get Condotel Details"
    "Get Condotel by ID" = "Get Condotel Details"
    "Get Condotel by ID Not Found" = "Get Condotel Details"
    "Search Condotel by Name" = "Search Condotel by Name"
    "Filter Condotel by Price" = "Filter Condotel by Price"
    "Filter Condotel by Location" = "Filter Condotel by Location"
    "Filter Condotel by Date Range" = "Filter Condotel by Date Range"
    "Filter Condotel by Beds and Bathrooms" = "Filter Condotel by Beds and Bathrooms"
    "Delete Condotel" = "Update Condotel"
    "Delete Condotel Not Found" = "Update Condotel"
    "Create Review" = "Create Review"
    "Create Review Invalid Data" = "Create Review"
    "Update Review" = "Update Review"
    "Update Review After 7 Days" = "Update Review"
    "Update Review Not Found" = "Update Review"
    "Delete Review" = "Delete Review"
    "Delete Review After 7 Days" = "Delete Review"
    "Delete Review Not Found" = "Delete Review"
    "Get My Reviews" = "Get My Reviews"
    "Get Review by ID" = "Get Review by ID"
    "Get Review by ID Not Found" = "Get Review by ID"
    "Host Get Reviews" = "Get My Reviews"
    "Host Reply to Review" = "Host Reply to Review"
    "Host Reply Review Invalid Data" = "Host Reply to Review"
    "Host Reply Review Not Found" = "Host Reply to Review"
    "Host Report Review" = "Host Report Review"
    "Host Report Review Not Found" = "Host Report Review"
    "Get Reported Reviews" = "Get Reported Reviews"
    "Get Reported Reviews Empty" = "Get Reported Reviews"
    "Get Conversations" = "Get Conversations"
    "Get Conversations Not Found" = "Get Conversations"
    "Get Messages" = "Get Messages"
    "Get Messages Not Found" = "Get Messages"
    "Send Direct Message" = "Send Direct Message"
    "Send Direct Message Invalid Data" = "Send Direct Message"
    "Send Direct Message Non-existent Users" = "Send Direct Message"
    "Get Published Blog Posts" = "Get Published Blog Posts"
    "Get Blog Post by ID" = "Get Blog Post by ID"
    "Get Blog Post by ID Not Found" = "Get Blog Post by ID"
    "Get Blog Post by Slug" = "Get Blog Post by ID"
    "Create Blog Post" = "Create Blog Post"
    "Create Blog Post Invalid Data" = "Create Blog Post"
    "Create Blog Post Invalid Category" = "Create Blog Post"
    "Update Blog Post" = "Update Blog Post"
    "Update Blog Post Invalid Data" = "Update Blog Post"
    "Update Blog Post Not Found" = "Update Blog Post"
    "Delete Blog Post" = "Delete Blog Post"
    "Delete Blog Post Not Found" = "Delete Blog Post"
    "Get Blog Categories" = "Get Blog Categories"
    "Get Blog Categories Empty" = "Get Blog Categories"
    "Create Blog Category" = "Create / Update / Delete Blog Category"
    "Create Blog Category Duplicate Name" = "Create / Update / Delete Blog Category"
    "Create Blog Category Invalid Data" = "Create / Update / Delete Blog Category"
    "Update Blog Category" = "Create / Update / Delete Blog Category"
    "Update Blog Category Invalid Data" = "Create / Update / Delete Blog Category"
    "Update Blog Category Not Found" = "Create / Update / Delete Blog Category"
    "Delete Blog Category" = "Create / Update / Delete Blog Category"
    "Delete Blog Category Not Found" = "Create / Update / Delete Blog Category"
    "Get All Promotions" = "Get All Promotions"
    "Get Available Promotions" = "Get Available Promotions"
    "Get Promotion by ID" = "Get Promotion by ID"
    "Get Promotion by ID Not Found" = "Get Promotion by ID"
    "Get Promotions by Condotel" = "Get Promotions by Condotel / by Host"
    "Get Promotions by Condotel Not Found" = "Get Promotions by Condotel / by Host"
    "Get Promotions by Host" = "Get Promotions by Condotel / by Host"
    "Create Promotion" = "Create Promotion"
    "Create Promotion Invalid Data" = "Create Promotion"
    "Update Promotion" = "Update Promotion"
    "Update Promotion Invalid Data" = "Update Promotion"
    "Update Promotion Not Found" = "Update Promotion"
    "Delete Promotion" = "Delete Promotion"
    "Delete Promotion Not Found" = "Delete Promotion"
    "Create Voucher" = "Create Voucher"
    "Create Voucher Invalid Data" = "Create Voucher"
    "Update Voucher" = "Update Voucher"
    "Update Voucher Invalid Data" = "Update Voucher"
    "Update Voucher Not Found" = "Update Voucher"
    "Delete Voucher" = "Delete Voucher"
    "Delete Voucher Not Found" = "Delete Voucher"
    "Get Vouchers by Host" = "Get Vouchers by Host"
    "Get Voucher by ID" = "View Condotel Vouchers"
    "Get Voucher by ID Not Found" = "View Condotel Vouchers"
    "View Condotel Vouchers" = "View Condotel Vouchers"
    "Get All Locations" = "Get All Locations"
    "Get Location by ID" = "Get Location by ID"
    "Get Location by ID Not Found" = "Get Location by ID"
    "Create Location" = "Create Location"
    "Create Location Invalid Data" = "Create Location"
    "Update Location" = "Update Location"
    "Update Location Invalid Data" = "Update Location"
    "Update Location Not Found" = "Update Location"
    "Delete Location" = "Delete Location"
    "Delete Location Not Found" = "Delete Location"
    "Get All Resorts" = "Get All Resorts"
    "Get Resort by ID" = "Get Resort by ID"
    "Get Resort by ID Not Found" = "Get Resort by ID"
    "Get Resorts by Location" = "Get Resort by ID"
    "Get Resorts by Location Not Found" = "Get Resort by ID"
    "Create Resort" = "Create Resort"
    "Create Resort Invalid Data" = "Create Resort"
    "Get Utilities" = "Get Utilities"
    "Get Utility by ID" = "Get Utility by ID"
    "Get Utility by ID Not Found" = "Get Utility by ID"
    "Create Utility" = "Create Utility"
    "Create Utility Invalid Data" = "Create Utility"
    "Update Utility" = "Update Utility"
    "Update Utility Invalid Data" = "Update Utility"
    "Update Utility Not Found" = "Update Utility"
    "Delete Utility" = "Delete Utility"
    "Delete Utility Not Found" = "Delete Utility"
    "Get Service Packages" = "Get Service Packages"
    "Get Available Packages" = "Get Available Packages"
    "Get Service Package by ID" = "Get Service Package by ID"
    "Get Service Package by ID Not Found" = "Get Service Package by ID"
    "Create Service Package" = "Create Service Package"
    "Create Service Package Invalid Data" = "Create Service Package"
    "Update Service Package" = "Update Service Package"
    "Update Service Package Invalid Data" = "Update Service Package"
    "Update Service Package Not Found" = "Update Service Package"
    "Delete Service Package" = "Delete Service Package"
    "Delete Service Package Not Found" = "Delete Service Package"
    "Get My Package" = "Get My Package"
    "Purchase Package" = "Purchase Package"
    "Create Package Payment" = "Create Package Payment"
    "Confirm Package Payment" = "Confirm Package Payment"
    "Get Dashboard Overview" = "Get Dashboard Overview"
    "Get Dashboard Overview Unauthorized" = "Get Dashboard Overview"
    "Get Revenue Chart" = "Get Revenue Chart"
    "Get Revenue Chart Unauthorized" = "Get Revenue Chart"
    "Get Top Condotels" = "Get Top Condotels"
    "Get Top Condotels Unauthorized" = "Get Top Condotels"
    "Get Tenant Analytics" = "Get Tenant Analytics"
    "Get Tenant Analytics Unauthorized" = "Get Tenant Analytics"
    "Get Report" = "Get Dashboard Overview"
    "Upload Image" = "Upload User Image"
    "Get My Profile" = "Get Current User"
    "Get Host Profile Not Found" = "Get Current User"
    "Update Host Profile" = "Update Profile"
    "Update Host Profile Invalid Data" = "Update Profile"
    "Update Host Profile Not Found" = "Update Profile"
    "belongs to current user" = "Get Current User"
}

# Sheet mapping
$sheetMapping = @{
    "Account Management" = "AccountManagement"
    "Booking Management" = "BookingManagement"
    "Condotel Management" = "CondotelManagement"
    "Review Management" = "CommunicationManagement"
    "Chat Management" = "CommunicationManagement"
    "Communication Management" = "CommunicationManagement"
    "Blog Management" = "MarketingManagement"
    "Promotion Management" = "MarketingManagement"
    "Voucher Management" = "MarketingManagement"
    "Marketing Management" = "MarketingManagement"
    "Location Management" = "MasterDataManagement"
    "Resort Management" = "MasterDataManagement"
    "Utility Management" = "MasterDataManagement"
    "Master Data Management" = "MasterDataManagement"
    "Service Package Management" = "ServiceManagement"
    "Service Management" = "ServiceManagement"
    "Dashboard Management" = "DashboardManagement"
    "Admin Management" = "AccountManagement"
    "Profile Management" = "AccountManagement"
    "Reward Points Management" = "AccountManagement"
    "Payment Management" = "BookingManagement"
    "Host Management" = "CondotelManagement"
    "Tenant Management" = "CondotelManagement"
}

function Normalize-FunctionName {
    param([string]$funcName)
    
    if ([string]::IsNullOrWhiteSpace($funcName)) {
        return $funcName
    }
    
    $funcName = $funcName.Trim()
    
    if ($functionMapping.ContainsKey($funcName)) {
        return $functionMapping[$funcName]
    }
    
    # Remove common suffixes
    $normalized = $funcName -replace " Not Found$", "" `
                             -replace " Invalid Data$", "" `
                             -replace " Unauthorized$", "" `
                             -replace " Empty$", "" `
                             -replace " After 7 Days$", "" `
                             -replace " Duplicate Email$", "" `
                             -replace " Weak Password$", "" `
                             -replace " Invalid Category$", "" `
                             -replace " Duplicate Name$", "" `
                             -replace " Non-existent Users$", ""
    
    return $normalized
}

function Normalize-SheetName {
    param([string]$sheetName)
    
    if ([string]::IsNullOrWhiteSpace($sheetName)) {
        return $sheetName
    }
    
    $sheetName = $sheetName.Trim()
    
    if ($sheetMapping.ContainsKey($sheetName)) {
        return $sheetMapping[$sheetName]
    }
    
    return $sheetName
}

Write-Host "Đang đọc file $inputFile..."

# Đọc file và xử lý từng dòng, tìm và thay thế trực tiếp trong text
$content = Get-Content $inputFile -Raw -Encoding UTF8

# Thay thế Sheet Names
Write-Host "Đang thay thế Sheet Names..."
foreach ($oldSheet in $sheetMapping.Keys) {
    $newSheet = $sheetMapping[$oldSheet]
    # Thay thế trong CSV format: ,"Sheet Name", hoặc ,Sheet Name,
    $pattern1 = ",`"$oldSheet`","
    $pattern2 = ",$oldSheet,"
    $content = $content -replace [regex]::Escape($pattern1), ",`"$newSheet`","
    $content = $content -replace [regex]::Escape($pattern2), ",$newSheet,"
}

# Thay thế Function Names
Write-Host "Đang thay thế Function Names..."
foreach ($oldFunc in $functionMapping.Keys) {
    $newFunc = $functionMapping[$oldFunc]
    # Thay thế trong CSV format: ,"Function Name", hoặc ,Function Name,
    $pattern1 = ",`"$oldFunc`","
    $pattern2 = ",$oldFunc,"
    $content = $content -replace [regex]::Escape($pattern1), ",`"$newFunc`","
    $content = $content -replace [regex]::Escape($pattern2), ",$newFunc,"
}

Write-Host "Đang ghi file $outputFile..."
$content | Out-File -FilePath $outputFile -Encoding UTF8 -NoNewline

Write-Host "Hoàn thành! Đã tạo file $outputFile"

# Thống kê
Write-Host "`nĐang tạo thống kê..."
try {
    $testCases = Import-Csv -Path $outputFile -Encoding UTF8 -ErrorAction SilentlyContinue
    if ($testCases) {
        $sheetStats = $testCases | Where-Object { $_.'Sheet Name' -and $_.'Sheet Name' -notmatch '^[,\s]*$' } | 
                      Group-Object 'Sheet Name' | Select-Object Name, Count | Sort-Object Name
        $funcStats = $testCases | Where-Object { $_.'Function Name' -and $_.'Function Name' -notmatch '^[,\s]*$' } | 
                     Group-Object 'Function Name' | Select-Object Name, Count | Sort-Object Count -Descending | Select-Object -First 20
        
        Write-Host "`n=== THỐNG KÊ ==="
        Write-Host "`nTheo Sheet:"
        foreach ($stat in $sheetStats) {
            Write-Host "  $($stat.Name): $($stat.Count) test cases"
        }
        
        Write-Host "`nTop 20 Function Names:"
        foreach ($stat in $funcStats) {
            Write-Host "  $($stat.Name): $($stat.Count) test cases"
        }
    }
} catch {
    Write-Host "Không thể tạo thống kê chi tiết, nhưng file đã được tạo thành công."
}













