# HƯỚNG DẪN SỬA LỖI KẾT NỐI SQL SERVER

## 🔍 BƯỚC 1: Kiểm tra SQL Server đang chạy

### Cách 1: Dùng SQL Server Configuration Manager
1. Mở **SQL Server Configuration Manager**
2. Vào **SQL Server Services**
3. Kiểm tra **SQL Server (LONG)** hoặc **SQL Server (MSSQLSERVER)** đang **Running**
4. Nếu không chạy, click **Start**

### Cách 2: Dùng Services
1. Nhấn `Win + R`, gõ `services.msc`
2. Tìm **SQL Server (LONG)** hoặc **SQL Server (MSSQLSERVER)**
3. Kiểm tra trạng thái là **Running**
4. Nếu không, click chuột phải → **Start**

### Cách 3: Dùng Command Prompt
```cmd
sc query MSSQLSERVER
sc query MSSQL$LONG
```

---

## 🔍 BƯỚC 2: Tìm Instance Name đúng

### Cách 1: Dùng SQL Server Management Studio (SSMS)
1. Mở **SQL Server Management Studio**
2. Khi connect, xem danh sách **Server name** dropdown
3. Ghi lại tên instance chính xác

### Cách 2: Dùng Command Prompt
```cmd
sqlcmd -L
```
Hoặc:
```cmd
Get-Service | Where-Object {$_.Name -like "*SQL*"}
```

### Cách 3: Kiểm tra trong Registry
1. Nhấn `Win + R`, gõ `regedit`
2. Vào: `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL`
3. Xem các instance đã cài đặt

---

## 🔧 BƯỚC 3: Các Connection String thay thế

### Option 1: Default Instance (MSSQLSERVER)
```json
"MyCnn": "server=localhost;database=CondotelDB_Ver3;uid=sa;pwd=123;TrustServerCertificate=True"
```
hoặc
```json
"MyCnn": "server=.;database=CondotelDB_Ver3;uid=sa;pwd=123;TrustServerCertificate=True"
```

### Option 2: Named Instance (nếu instance name là LONG)
```json
"MyCnn": "server=localhost\\LONG;database=CondotelDB_Ver3;uid=sa;pwd=123;TrustServerCertificate=True"
```
hoặc
```json
"MyCnn": "server=.\\LONG;database=CondotelDB_Ver3;uid=sa;pwd=123;TrustServerCertificate=True"
```

### Option 3: Dùng tên máy tính hiện tại
1. Kiểm tra tên máy tính: `Win + R` → `sysdm.cpl` → Tab **Computer Name**
2. Thay `DESKTOP-F488CFL` bằng tên máy tính hiện tại:
```json
"MyCnn": "server=YOUR_COMPUTER_NAME\\LONG;database=CondotelDB_Ver3;uid=sa;pwd=123;TrustServerCertificate=True"
```

### Option 4: Dùng IP Address (nếu trên cùng máy)
```json
"MyCnn": "server=127.0.0.1\\LONG;database=CondotelDB_Ver3;uid=sa;pwd=123;TrustServerCertificate=True"
```

### Option 5: Dùng Windows Authentication (nếu không dùng sa)
```json
"MyCnn": "server=localhost\\LONG;database=CondotelDB_Ver3;Integrated Security=True;TrustServerCertificate=True"
```

---

## 🔧 BƯỚC 4: Kiểm tra SQL Server cho phép Remote Connections

1. Mở **SQL Server Configuration Manager**
2. Vào **SQL Server Network Configuration** → **Protocols for LONG** (hoặc MSSQLSERVER)
3. Đảm bảo **TCP/IP** và **Named Pipes** đều **Enabled**
4. Click chuột phải **TCP/IP** → **Properties** → Tab **IP Addresses**
5. Scroll xuống **IPAll**, đảm bảo **TCP Port** là **1433** (hoặc port khác nếu bạn đã cấu hình)
6. **Restart SQL Server service** sau khi thay đổi

---

## 🔧 BƯỚC 5: Kiểm tra Firewall

1. Mở **Windows Defender Firewall**
2. Vào **Advanced settings**
3. Kiểm tra có rule cho SQL Server port (thường là 1433)
4. Nếu không có, tạo rule mới cho port 1433

---

## 🧪 BƯỚC 6: Test Connection

### Cách 1: Dùng SQL Server Management Studio
1. Mở SSMS
2. Thử connect với các connection string ở trên
3. Nếu connect được, copy connection string đó vào `appsettings.json`

### Cách 2: Dùng Command Prompt
```cmd
sqlcmd -S localhost\LONG -U sa -P 123 -Q "SELECT @@VERSION"
```
hoặc
```cmd
sqlcmd -S localhost -U sa -P 123 -Q "SELECT @@VERSION"
```

### Cách 3: Dùng PowerShell
```powershell
$connectionString = "Server=localhost\LONG;Database=CondotelDB_Ver3;User Id=sa;Password=123;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $connection.Open()
    Write-Host "Connection successful!"
    $connection.Close()
} catch {
    Write-Host "Connection failed: $_"
}
```

---

## 📝 BƯỚC 7: Cập nhật appsettings.json

Sau khi tìm được connection string đúng, cập nhật file `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MyCnn": "server=localhost\\LONG;database=CondotelDB_Ver3;uid=sa;pwd=123;TrustServerCertificate=True"
  }
}
```

**Lưu ý:** 
- Trong JSON, cần escape backslash: `\\` thay vì `\`
- Hoặc dùng forward slash: `/` (một số trường hợp)

---

## 🚨 CÁC LỖI THƯỜNG GẶP

### Lỗi: "Cannot connect to DESKTOP-F488CFL\LONG"
**Nguyên nhân:** Tên máy tính đã thay đổi hoặc instance không tồn tại
**Giải pháp:** Dùng `localhost` hoặc tên máy tính hiện tại

### Lỗi: "Login failed for user 'sa'"
**Nguyên nhân:** Password sai hoặc SQL Authentication chưa enable
**Giải pháp:** 
1. Kiểm tra password trong SQL Server
2. Enable SQL Authentication trong SSMS: Server Properties → Security → SQL Server and Windows Authentication mode

### Lỗi: "A network-related or instance-specific error"
**Nguyên nhân:** SQL Server service không chạy hoặc port bị chặn
**Giải pháp:** 
1. Start SQL Server service
2. Kiểm tra firewall
3. Kiểm tra TCP/IP protocol enabled

---

## ✅ CHECKLIST

- [ ] SQL Server service đang chạy
- [ ] Đã tìm được instance name chính xác
- [ ] TCP/IP protocol enabled
- [ ] Firewall cho phép port SQL Server
- [ ] Connection string đúng format
- [ ] Đã test connection thành công
- [ ] Đã cập nhật appsettings.json

---

## 💡 TIP: Tạo Connection String Helper

Nếu thường xuyên gặp vấn đề này, có thể tạo một helper để test connection:

```csharp
// Thêm vào Program.cs để test connection khi start
var connectionString = builder.Configuration.GetConnectionString("MyCnn");
try
{
    using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
    connection.Open();
    Console.WriteLine("✅ Database connection successful!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Database connection failed: {ex.Message}");
    Console.WriteLine($"Connection string: {connectionString}");
    throw;
}
```

