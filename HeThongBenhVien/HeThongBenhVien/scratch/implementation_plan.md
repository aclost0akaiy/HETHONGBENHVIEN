# Kế hoạch triển khai - Thiết lập Lý do Cấp cứu & Chuyển Trạng thái Ưu tiên

Yêu cầu:
Khi nhấn "Đặt Cấp Cứu", bác sĩ phải nhập lý do cấp cứu (hiển thị popup modal). Khi xác nhận, trạng thái của bệnh nhân sẽ được chuyển thành **Ưu tiên** (Status = 6) để đưa lên đầu danh sách và hiển thị biểu tượng cấp cứu phù hợp, đồng thời chữ hiển thị lý do khám sẽ không bị đổi sang màu đỏ.

## Thay đổi đề xuất

### 1. Controllers

#### [MODIFY] [DoctorController.cs](file:///c:/Users/PC/Downloads/HETHONGBENHVIEN/HeThongBenhVien/HeThongBenhVien/Controllers/DoctorController.cs)
- Cập nhật hàm đếm số ca cấp cứu `emergencyCount` ở phương thức `Dashboard` để hỗ trợ tiền tố lý do linh hoạt dạng `[CẤP CỨU` (thay vì chỉ khớp chính xác `[CẤP CỨU]`).
- Cập nhật phương thức `ToggleEmergency` để nhận thêm tham số `string? emergencyReason`:
  - Khi thiết lập cấp cứu: Lưu lý do dưới dạng `[CẤP CỨU: {emergencyReason}] {Lý do cũ}` và đổi `Status` thành `6` (Ưu tiên).
  - Khi hủy cấp cứu (Bỏ ưu tiên): Loại bỏ tiền tố `[CẤP CỨU: ...]` khỏi `Reason` và đưa `Status` trở lại `1` (Đang chờ).

### 2. Views

#### [MODIFY] [Dashboard.cshtml](file:///c:/Users/PC/Downloads/HETHONGBENHVIEN/HeThongBenhVien/HeThongBenhVien/Views/Doctor/Dashboard.cshtml)
- Thêm **Modal nhập lý do cấp cứu** (`#emergencyModal`) với một ô nhập văn bản (textarea) để bác sĩ điền lý do cấp cứu/ưu tiên.
- Thêm hàm JavaScript `showEmergencyModal(apptId, patientName)` để mở modal và gán thông tin.
- Thay đổi mục dropdown "Đặt Cấp Cứu":
  - Nếu đã là Cấp cứu: Hiển thị nút bấm để gọi trực tiếp hành động xóa (Bỏ ưu tiên).
  - Nếu chưa là Cấp cứu: Gọi hàm `showEmergencyModal` để mở modal nhập lý do.
- Cập nhật hiển thị cột "Lý do khám":
  - Không bôi đỏ chữ lý do khám nữa.
  - Hiển thị lý do khám gốc dạng bình thường. Nếu là ca cấp cứu, hiển thị thêm lý do cấp cứu nhỏ màu xám ở dòng dưới (ví dụ: `CẤP CỨU: Suy hô hấp`).
- Sửa các hàm kiểm tra tiền tố `[CẤP CỨU]` thành kiểm tra `[CẤP CỨU` (để hỗ trợ `[CẤP CỨU: lý do]`).

## Kế hoạch kiểm tra

### Kiểm tra thủ công
1. Vào Dashboard bác sĩ, chọn một bệnh nhân bất kỳ trong "Danh sách chờ khám tiếp theo".
2. Click vào dropdown hành động (`...`), chọn **Đặt Cấp Cứu**.
3. Xác nhận modal nhập lý do hiển thị, nhập lý do (ví dụ: `Suy hô hấp cấp`) và bấm **Xác nhận**.
4. Kiểm tra xem bệnh nhân đó có được chuyển trạng thái sang **Ưu tiên** (badge đỏ nhạt) và xuất hiện badge đỏ đậm **CẤP CỨU** bên cạnh tên bệnh nhân hay không.
5. Xác nhận chữ lý do khám của bệnh nhân hiển thị bình thường (không bị đỏ) và có ghi chú lý do cấp cứu ở dòng dưới.
6. Click lại dropdown hành động của bệnh nhân đó, chọn **Bỏ ưu tiên** và xác nhận trạng thái được phục hồi về **Đang chờ**, badge cấp cứu biến mất và lý do khám trở lại bình thường.
