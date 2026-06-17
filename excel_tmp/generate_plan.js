const xlsx = require('xlsx');
const fs = require('fs');

const tasks = [
    // 1. Phân tích và Khởi tạo dự án (15 lines)
    "1. KHỞI TẠO DỰ ÁN",
    "1.1. Họp kick-off dự án",
    "1.2. Xác định mục tiêu và phạm vi",
    "1.3. Thành lập nhóm dự án",
    "1.4. Phân công nhiệm vụ",
    "1.5. Thiết lập công cụ quản lý dự án",
    "1.6. Xác định các bên liên quan (Stakeholders)",
    "1.7. Thu thập yêu cầu tổng quan",
    "1.8. Lập kế hoạch quản lý rủi ro",
    "1.9. Dự toán chi phí cơ bản",
    "1.10. Ký kết tài liệu sơ bộ",
    "1.11. Thu thập yêu cầu chi tiết",
    "1.12. Phân tích quy trình nghiệp vụ",
    "1.13. Liệt kê yêu cầu chức năng (Functional)",
    "1.14. Liệt kê yêu cầu phi chức năng (Non-Functional)",
    "1.15. Phê duyệt tài liệu yêu cầu (BRD)",

    // 2. Thiết kế hệ thống (25 lines)
    "2. THIẾT KẾ HỆ THỐNG",
    "2.1. Thiết kế kiến trúc tổng thể (Architecture)",
    "2.2. Lựa chọn công nghệ (Node.js, C#, React, SQL...)",
    "2.3. Thiết kế CSDL (Database Design)",
    "2.4. Phác thảo ERD cơ bản",
    "2.5. Xác định bảng Users, Roles",
    "2.6. Xác định bảng Patients, Appointments",
    "2.7. Xác định bảng MedicalRecords, Vitals",
    "2.8. Xác định bảng Prescriptions, Medicines",
    "2.9. Xác định bảng Billing, Payments",
    "2.10. Chuẩn hóa CSDL",
    "2.11. Review và duyệt thiết kế CSDL",
    "2.12. Thiết kế giao diện (UI/UX Design)",
    "2.13. Thiết kế Wireframes trang chủ",
    "2.14. Thiết kế Wireframes Dashboard",
    "2.15. Thiết kế Mockup trang Quản lý Bệnh nhân",
    "2.16. Thiết kế Mockup trang Lịch hẹn",
    "2.17. Thiết kế Mockup trang Khám bệnh",
    "2.18. Thiết kế Mockup Medical Record",
    "2.19. Chốt bảng màu (Color Palette) & Typography",
    "2.20. Prototype trải nghiệm người dùng",
    "2.21. Phê duyệt thiết kế UI/UX",
    "2.22. Thiết kế API",
    "2.23. Định nghĩa endpoints",
    "2.24. Định nghĩa cấu trúc JSON (Request/Response)",
    "2.25. Phê duyệt tài liệu API (Swagger/Postman)",

    // 3. Xây dựng môi trường & Core (10 lines)
    "3. XÂY DỰNG MÔI TRƯỜNG & CORE (FOUNDATION)",
    "3.1. Cài đặt Server/Hosting",
    "3.2. Cấu hình Database Engine (SQL Server/MySQL)",
    "3.3. Khởi tạo Source framework",
    "3.4. Tích hợp thư viện bảo mật (JWT, Bcrypt)",
    "3.5. Xây dựng base Repository pattern",
    "3.6. Xây dựng Middleware xử lý lỗi (Error Handler)",
    "3.7. Cấu hình CORS và Logging",
    "3.8. Xây dựng hệ thống Routing cơ bản",
    "3.9. Kiểm tra CI/CD cơ bản",

    // 4. Phát triển Module Hệ thống & Phân quyền (10 lines)
    "4. MODULE HỆ THỐNG & PHÂN QUYỀN",
    "4.1. Thiết kế UI màn hình Đăng nhập",
    "4.2. API Đăng nhập và tạo JWT",
    "4.3. API Đăng xuất",
    "4.4. Xây dựng quản lý Role (Vai trò)",
    "4.5. Phân quyền nghiệp vụ (Admin, Doctor, Nurse, Receptionist)",
    "4.6. UI Quản lý tài khoản nhân viên",
    "4.7. API Thêm/Sửa/Xóa nhân viên",
    "4.8. Chức năng Reset Password",
    "4.9. Unit test module Phân quyền",

    // 5. Phát triển Module Lễ tân & Danh mục (10 lines)
    "5. MODULE LỄ TÂN & DANH MỤC",
    "5.1. Cấu trúc DB danh mục khoa, phòng ban",
    "5.2. UI Quản lý Khoa và Phòng",
    "5.3. API CRUD Danh mục Khoa",
    "5.4. API CRUD Danh mục Phòng",
    "5.5. Quản lý danh mục Dịch vụ khám",
    "5.6. Quản lý danh mục Thuốc",
    "5.7. Tích hợp tìm kiếm, phân trang linh hoạt",
    "5.8. Xử lý logic Validation",
    "5.9. Review code module Danh mục",

    // 6. Phát triển Module Đăng ký & Lịch hẹn (15 lines)
    "6. MODULE ĐĂNG KÝ KHÁM & QUẢN LÝ LỊCH HẸN",
    "6.1. Thiết kế UI Form đăng ký bệnh nhân mới",
    "6.2. Thiết kế UI Danh sách lịch hẹn",
    "6.3. API Tạo lịch hẹn mới",
    "6.4. Sinh số thứ tự động cho bệnh nhân",
    "6.5. Tích hợp tạo QR code cho Lịch hẹn",
    "6.6. Chức năng tra cứu bệnh nhân cũ theo CMND/CCCD",
    "6.7. API Cập nhật trạng thái lịch hẹn",
    "6.8. Workflow: Đang chờ -> Đang khám -> Hoàn thành",
    "6.9. Lọc lịch hẹn theo ngày, theo bác sĩ, theo tình trạng",
    "6.10. Tích hợp tính năng gọi số thứ tự",
    "6.11. Cập nhật UI hiển thị màn chờ",
    "6.12. Text-to-speech gọi tên bệnh nhân",
    "6.13. Review UX cho quá trình đẩy bệnh nhân vào phòng khám",
    "6.14. Unit test module Lịch hẹn",

    // 7. Phát triển Module Khám bệnh (20 lines)
    "7. MODULE KHÁM BỆNH (BÁC SĨ)",
    "7.1. Giao diện trang khám bệnh chính",
    "7.2. UI Khu vực Hành chính bệnh nhân",
    "7.3. UI Nhập Chỉ số sinh tồn (Vitals)",
    "7.4. UI Ghi nhận Triệu chứng & Bệnh sử",
    "7.5. Tính năng Voice-to-text điền nhanh triệu chứng",
    "7.6. UI Chẩn đoán bệnh (ICD-10 list)",
    "7.7. API Lưu trữ chỉ số sinh tồn và triệu chứng",
    "7.8. Chỉ định Cận lâm sàng (Xét nghiệm/X-Quang)",
    "7.9. UI Kê đơn thuốc",
    "7.10. Cảnh báo tương tác thuốc cơ bản",
    "7.11. Cảnh báo dị ứng cá nhân",
    "7.12. API Lưu trữ Đơn thuốc (Prescription)",
    "7.13. Cập nhật trạng thái khám (Completed)",
    "7.14. Tính năng 'Digital Signature' khóa bệnh án",
    "7.15. Thiết kế UI/Layout in toa thuốc",
    "7.16. Logic xuất PDF cho toa thuốc",
    "7.17. Review bảo mật thông tin khám bệnh",
    "7.18. Tối ưu load danh sách thuốc khi kê toa",
    "7.19. Unit testing phần kê toa và khóa hồ sơ",

    // 8. Phát triển Module Cận lâm sàng (10 lines)
    "8. MODULE CẬN LÂM SÀNG (XÉT NGHIỆM)",
    "8.1. UI Phòng Xét nghiệm chờ xử lý",
    "8.2. API Nhận danh sách chỉ định từ Bác sĩ",
    "8.3. UI Nhập kết quả xét nghiệm",
    "8.4. Tích hợp upload hình ảnh kết quả X-Quang",
    "8.5. Cảnh báo chỉ số vượt ngưỡng bằng màu sắc",
    "8.6. API Lưu trữ kết quả cận lâm sàng",
    "8.7. Đồng bộ trả kết quả về module Khám bệnh",
    "8.8. In kết quả cận lâm sàng (Export PDF)",
    "8.9. Unit testing module Cận lâm sàng",

    // 9. Phát triển Module Viện phí & Thanh toán (15 lines)
    "9. MODULE QUẢN LÝ VIỆN PHÍ & THANH TOÁN",
    "9.1. Giao diện Danh sách bệnh nhân chờ thanh toán",
    "9.2. Tổng hợp chi phí Khám, Cận lâm sàng, Thuốc",
    "9.3. Giao diện chi tiết hóa đơn",
    "9.4. Tích hợp hiển thị Tổng bill",
    "9.5. Xử lý miễn giảm / bảo hiểm (nếu có)",
    "9.6. Chức năng xác nhận thu tiền",
    "9.7. Tạo QR code thanh toán MoMo/VNPAY (Mockup/Demo)",
    "9.8. API Lịch sử thanh toán",
    "9.9. Logic không cho nhận thuốc nếu chưa thanh toán",
    "9.10. Form in Hóa đơn thanh toán tài chính",
    "9.11. Kiểm tra tranh chấp đồng thời (Concurrency billing)",
    "9.12. Report doanh thu trong ngày ngắn gọn",
    "9.13. Review và tối ưu tính toán giá trị lớn",
    "9.14. Unit testing module Viện phí",

    // 10. Phát triển Module Quản lý Kho Dược (15 lines)
    "10. MODULE QUẢN LÝ KHO DƯỢC",
    "10.1. UI Danh sách tồn kho thuốc",
    "10.2. Cảnh báo thuốc sắp hết hạn (Date alert)",
    "10.3. Cảnh báo lượng tồn kho thấp",
    "10.4. API Thêm Lô thuốc mới (Nhập kho)",
    "10.5. UI Phiếu xuất kho (cho Y lệnh từ Bác sĩ)",
    "10.6. Logic duyệt đơn thuốc đã thanh toán",
    "10.7. Logic trừ tồn kho tự động",
    "10.8. Thống kê nhập/xuất trong tháng",
    "10.9. Kiểm kê kho (Stock take)",
    "10.10. Khóa dữ liệu kỳ kế toán của kho",
    "10.11. API Phân trang và tìm kiếm theo dược chất",
    "10.12. Đảm bảo Transaction khi trừ kho",
    "10.13. Unit testing Module Kho Dược",
    "10.14. Integration testing Kho Dược với Viện Phí",

    // 11. Báo cáo Tụ động & Dashboard (10 lines)
    "11. BÁO CÁO & DASHBOARD",
    "11.1. Dashboard Admin: Tổng số KH trong ngày",
    "11.2. Biểu đồ doanh thu 7 ngày gần nhất",
    "11.3. Thống kê tỷ lệ mắc bệnh (theo ICD)",
    "11.4. Báo cáo xuất/nhập/tồn thuốc (Export Excel)",
    "11.5. Báo cáo công tác bác sĩ",
    "11.6. Truy vấn hiệu suất cơ sở dữ liệu",
    "11.7. View tổng quan cho Tổng giám đốc",
    "11.8. Caching dữ liệu cho Dashboard",
    "11.9. Hoàn thiện Unit testing Module Báo cáo",

    // 12. Kiểm thử phần mềm (Testing) (15 lines)
    "12. KIỂM THỬ PHẦN MỀM (TESTING & QA)",
    "12.1. Lập Test Plan",
    "12.2. Viết Test Cases cho các chức năng chính",
    "12.3. Thực hiện Function Testing (Khám, Thanh toán)",
    "12.4. Thực hiện UI Testing (Responsive & Glitches)",
    "12.5. Kiểm tra tính tương thích chéo trình duyệt (Cross-browser)",
    "12.6. Performance Testing (Tải hệ thống với 10,000 reqs)",
    "12.7. Security Testing cơ bản (SQL Injection, XSS)",
    "12.8. Ghi nhận lỗi (Log Bugs) vào Jira/Trello",
    "12.9. Tái kiểm tra (Regression Testing)",
    "12.10. Bug Fixing Phase 1",
    "12.11. Bug Fixing Phase 2",
    "12.12. User Acceptance Testing (UAT) lần 1",
    "12.13. UAT lần 2",
    "12.14. Phê duyệt kết quả kiểm thử",

    // 13. Triển khai & Vận hành (10 lines)
    "13. TRIỂN KHAI VÀ VẬN HÀNH (DEPLOYMENT)",
    "13.1. Chuẩn bị môi trường Production",
    "13.2. Cấu hình tên miền (Domain)",
    "13.3. Cài đặt chứng chỉ bảo mật (SSL/TLS)",
    "13.4. Triển khai Database lêm Production",
    "13.5. Chạy Data Seeding (Dữ liệu Admin, Master Data)",
    "13.6. Deploy Frontend và Backend Container",
    "13.7. Cấu hình backup dữ liệu định kỳ",
    "13.8. Cấu hình server monitoring (Logs/CPU/RAM)",
    "13.9. Smoke testing trực tiếp trên Production",

    // 14. Bàn giao và Đào tạo (10 lines)
    "14. ĐÀO TẠO & HOÀN THIỆN HỒ SƠ",
    "14.1. Cập nhật tài liệu HDSD (End User Manual)",
    "14.2. Cập nhật tài liệu Quản trị (Admin Guide)",
    "14.3. Tổ chức training Nhóm Lễ tân",
    "14.4. Tổ chức training Nhóm Bác sĩ & Y tá",
    "14.5. Tổ chức training Kế toán & Kho",
    "14.6. Bàn giao mã nguồn (Source Code)",
    "14.7. Họp nghiệm thu dự án (Sign-off)",
    "14.8. Chuyển sang giai đoạn Bảo trì (Support)",
    "14.9. Tối ưu code rác lần cuối"
];

// Let's create an array of objects
const data = [];

let baseDate = new Date(2026, 4, 1); // Start from May 1st, 2026

function addDays(date, days) {
    let result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}

function formatDate(date) {
    const d = new Date(date);
    return `${d.getDate().toString().padStart(2, '0')}/${(d.getMonth() + 1).toString().padStart(2, '0')}/${d.getFullYear()}`;
}

let currentDate = baseDate;

const assignees = ["Trưởng Nhóm", "Dev Frontend", "Dev Backend", "Dev Fullstack", "Tester/QA", "BA (Phân tích)"];

for (let i = 0; i < tasks.length; i++) {
    const isHeader = !tasks[i].includes(".");
    
    let duration = isHeader ? "" : (Math.floor(Math.random() * 3) + 1); // 1-3 days
    let startDate = isHeader ? "" : formatDate(currentDate);
    
    if (!isHeader) {
        currentDate = addDays(currentDate, duration);
    }
    
    let endDate = isHeader ? "" : formatDate(currentDate);
    let assignee = isHeader ? "" : assignees[Math.floor(Math.random() * assignees.length)];
    let status = isHeader ? "" : ((i < 100) ? "Hoàn thành" : (i < 140 ? "Đang tiến hành" : "Chưa bắt đầu"));
    
    data.push({
        "STT": isHeader ? "" : i,
        "Tên Công Việc": tasks[i],
        "Thời Gian Bắt Đầu": startDate,
        "Thời Gian Kết Thúc": endDate,
        "Số Ngày": duration,
        "Người Phụ Trách": assignee,
        "Trạng Thái": status,
        "Ghi Chú": ""
    });
}

// Generate total duration for headers
// (A simple way to just show total phase dates)
let startIdx = 0;
while(startIdx < data.length) {
    if (data[startIdx]["STT"] === "") {
        let hrRowIndex = startIdx;
        let subEndIdx = startIdx + 1;
        while(subEndIdx < data.length && data[subEndIdx]["STT"] !== "") {
            subEndIdx++;
        }
        if (subEndIdx > startIdx + 1) {
            data[hrRowIndex]["Thời Gian Bắt Đầu"] = data[startIdx+1]["Thời Gian Bắt Đầu"];
            data[hrRowIndex]["Thời Gian Kết Thúc"] = data[subEndIdx-1]["Thời Gian Kết Thúc"];
            data[hrRowIndex]["Trạng Thái"] = "...";
            // recalculate duration manually or skip
        }
    }
    startIdx++;
}

// Add row styles and auto column width
const wb = xlsx.utils.book_new();
const ws = xlsx.utils.json_to_sheet(data);

ws['!cols'] = [
    { wch: 5 },  // STT
    { wch: 55 }, // Tên Công Việc
    { wch: 15 }, // Start
    { wch: 15 }, // End
    { wch: 10 }, // Duration
    { wch: 20 }, // Assignee
    { wch: 15 }, // Status
    { wch: 20 }  // Notes
];

xlsx.utils.book_append_sheet(wb, ws, 'Project Plan');

const outputPath = "c:/Users/PC/Downloads/HETHONGBENHVIEN/HeThongBenhVien/_PRO2192_Project plan_Updated.xlsx";
xlsx.writeFile(wb, outputPath);
console.log(`Excel file successfully created at ${outputPath} with ${data.length} tasks!`);
