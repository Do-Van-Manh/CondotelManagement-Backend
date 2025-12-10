# Hướng dẫn sửa lỗi encoding tiếng Việt trong Excel

## Vấn đề
File CSV hiển thị sai tiếng Việt trong Excel (ví dụ: "Kiá»ƒm tra" thay vì "Kiểm tra")

## Giải pháp

### Bước 1: Đóng Excel
**QUAN TRỌNG**: Đóng tất cả cửa sổ Excel đang mở file CSV

### Bước 2: Chạy script fix encoding
Mở PowerShell trong thư mục `CondotelManagement.Tests/Integration` và chạy:

```powershell
.\FixCSVEncoding.ps1
```

Script này sẽ:
- Tạo backup file
- Thêm UTF-8 BOM vào file
- Hướng dẫn cách mở file đúng

### Bước 3: Mở file trong Excel (CÁCH ĐÚNG)

**CÁCH TỐT NHẤT - Import vào Excel:**

1. Mở Excel (file CSV vẫn đóng)
2. Chọn tab **Data**
3. Click **Get Data** → **From File** → **From Text/CSV**
4. Chọn file `SystemWorkflowTests.csv`
5. Trong cửa sổ preview:
   - Tìm dropdown **File Origin** (hoặc **Encoding**)
   - Chọn **65001: Unicode (UTF-8)** hoặc **UTF-8**
   - Kiểm tra xem tiếng Việt hiển thị đúng chưa (ví dụ: "Kiểm tra tình trạng")
6. Click **Load** để import

**CÁCH KHÁC - Mở trực tiếp:**

1. Đóng file CSV nếu đang mở
2. Mở Excel
3. **File** → **Open**
4. Chọn file `SystemWorkflowTests.csv`
5. Nếu Excel hiển thị cửa sổ "Text Import Wizard":
   - Step 1: Chọn **Delimited**
   - Step 2: Chọn delimiter là **Comma**
   - Step 3: Chọn **File origin**: **65001: Unicode (UTF-8)**
6. Click **Finish**

## Kiểm tra

Sau khi mở, kiểm tra các dòng sau có hiển thị đúng không:

✅ **ĐÚNG:**
- "Kiểm tra tình trạng"
- "Condotel có sẵn trong khoảng thời gian này"
- "Đặt phòng"
- "Yêu cầu hoàn tiền đã được tạo thành công"

❌ **SAI (cần fix):**
- "Kiá»ƒm tra tÃ¬nh tráº¡ng"
- "Condotel cÃ³ sáºµn trong khoáº£ng thá»i gian nÃ y"
- "Äáº·t phÃ²ng"
- "YÃªu cáº§u hoÃ n tiá»n Ä'Ã£ Ä'Æ°á»£c táº¡o thÃ nh cÃ´ng"

## Lưu ý

- File CSV đã được lưu với UTF-8 encoding
- Excel không tự động nhận UTF-8 khi double-click
- **Luôn dùng cách Import** (Data > Get Data) để đảm bảo encoding đúng
- Nếu vẫn lỗi, thử mở file bằng Notepad++ và chọn Encoding > Convert to UTF-8-BOM, sau đó save và mở lại trong Excel

## Nếu vẫn không được

1. Mở file bằng **Notepad++**
2. Menu **Encoding** → **Convert to UTF-8-BOM**
3. **Save** file
4. Đóng Notepad++
5. Mở lại file trong Excel bằng cách **Import** (Data > Get Data)







