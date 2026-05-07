using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBenhVien.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly Data.ApplicationDbContext _context;

        public AdminController(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // Lấy dữ liệu từ database
            var totalAppointments = _context.Appointments.Count();
            var totalPatients = _context.Patients.Count();
            var medicalRecordsCount = _context.MedicalRecords.Count();
            var emergencyCases = _context.Appointments.Count(a => a.Reason.ToLower().Contains("cấp cứu"));
            
            // Giả lập doanh thu dựa trên số ca khám bệnh hoàn tất (Ví dụ mỗi ca 500k)
            var dailyRevenue = medicalRecordsCount * 500000;

            // Truyền sang View
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TotalPatients = totalPatients;
            ViewBag.DailyRevenue = dailyRevenue;
            ViewBag.EmergencyCases = emergencyCases;
            
            return View();
        }

        // 1 - 10: Existing
        public IActionResult QuanLyTaiKhoan() { return View(); }
        public IActionResult QuanLyNhanSu() { return View(); }
        public IActionResult QuanLyKhoaPhong() { return View(); }
        public IActionResult QuanLyLichLamViec() { return View(); }
        public IActionResult QuanLyDichVu() { return View(); }
        public IActionResult QuanLyGia() { return View(); }
        public IActionResult QuanLyKhoDuoc() { return View(); }
        public IActionResult QuanLyThietBi() { return View(); }
        public IActionResult ThongKeDoanhThu() { return View(); }
        public IActionResult CauHinhHeThong() { return View(); }

        // 11 - 20: New Added Functions
        public IActionResult QuanLyBenhNhan() { return View(); }
        public IActionResult QuanLyVienPhi() { return View(); }
        public IActionResult QuanLyBHYT() { return View(); }
        public IActionResult BaoCaoHoatDong() { return View(); }
        public IActionResult QuanLyTiepDon() { return View(); }
        public IActionResult QuanLyXetNghiem() { return View(); }
        public IActionResult QuanLyCDHA() { return View(); }
        public IActionResult QuanLyPhauThuat() { return View(); }
        public IActionResult QuanLyNganHangMau() { return View(); }
        public IActionResult DanhGiaChatLuong() { return View(); }
    }
}
