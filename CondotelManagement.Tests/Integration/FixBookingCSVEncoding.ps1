$filePath = "D:\CondotelManagement\CondotelManagement-Backend\CondotelManagement.Tests\Integration\BookingWorkflowTests.csv"
$content = Get-Content $filePath -Raw -Encoding UTF8
$utf8WithBom = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllText($filePath, $content, $utf8WithBom)
Write-Host "File encoding fixed to UTF-8 BOM"







