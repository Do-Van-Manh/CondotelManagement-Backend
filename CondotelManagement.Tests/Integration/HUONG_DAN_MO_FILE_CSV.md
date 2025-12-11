# Hướng dẫn mở file CSV với tiếng Việt đúng trong Excel

## Vấn đề
File CSV có thể hiển thị sai tiếng Việt trong Excel nếu không được mở đúng cách.

## Giải pháp

### Cách 1: Import vào Excel (Khuyến nghị)
1. Mở Excel
2. Chọn **Data** → **Get Data** → **From File** → **From Text/CSV**
3. Chọn file `SystemWorkflowTests.csv`
4. Trong cửa sổ preview:
   - **File Origin**: Chọn **65001: Unicode (UTF-8)**
   - Kiểm tra xem tiếng Việt hiển thị đúng chưa
5. Click **Load** để import

### Cách 2: Mở trực tiếp và chọn encoding
1. Đóng file CSV nếu đang mở
2. Mở Excel
3. Chọn **File** → **Open**
4. Chọn file `SystemWorkflowTests.csv`
5. Nếu Excel hỏi encoding, chọn **UTF-8** hoặc **65001: Unicode (UTF-8)**
6. Click **OK**

### Cách 3: Sử dụng Notepad++ để kiểm tra
1. Mở file bằng Notepad++
2. Menu **Encoding** → **Convert to UTF-8-BOM**
3. Save file
4. Mở lại trong Excel

## Kiểm tra
Sau khi mở, kiểm tra các dòng sau có hiển thị đúng không:
- "Kiểm tra tình trạng" (không phải "Kiá»ƒm tra tÃ¬nh tráº¡ng")
- "Condotel có sẵn" (không phải "Condotel cÃ³ sáºµn")
- "Đặt phòng" (không phải "Äáº·t phÃ²ng")

## Lưu ý
- File đã được lưu với UTF-8 BOM encoding
- Nếu vẫn lỗi, thử cách 1 (Import) - đây là cách tốt nhất







