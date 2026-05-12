using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using HeThongBenhVien.Models;

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

        // ==========================================
        // CHỨC NĂNG QUẢN LÝ NHÂN SỰ (PHẦN CỦA BẠN - ĐÃ CHẠY ĐƯỢC)
        // ==========================================

        // 1. Lấy toàn bộ nhân sự từ Database truyền ra giao diện
        public async Task<IActionResult> QuanLyNhanSu()
        {
            var danhSachNhanSu = await _context.Users.ToListAsync();
            return View(danhSachNhanSu);
        }

        // 2. Hiển thị form Thêm Nhân Sự (GET)
        [HttpGet]
        public IActionResult ThemNhanSu()
        {
            return View();
        }

        // 3. Xử lý nhận dữ liệu từ form và lưu vào SQL Server (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemNhanSu(User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem Username đã bị trùng chưa
                var daTonTai = await _context.Users.AnyAsync(u => u.Username == user.Username);
                if (daTonTai)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác!");
                    return View(user);
                }

                // Lưu vào database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Lưu xong thì quay lại trang danh sách nhân sự
                return RedirectToAction(nameof(QuanLyNhanSu));
            }
            return View(user);
        }

        // ==========================================
        // CHỨC NĂNG QUẢN LÝ TÀI KHOẢN (HIỂN THỊ TÀI KHOẢN BỆNH NHÂN)
        // ==========================================
        public IActionResult QuanLyTaiKhoan()
        {
            return RedirectToAction(nameof(QuanLyBenhNhan));
        }

        public IActionResult QuanLyKhoaPhong() { return View(); }
        public IActionResult QuanLyLichLamViec() { return View(); }
        public IActionResult QuanLyDichVu() 
        { 
            var services = new List<MedicalService>(); // Thêm mock data hoặc query _context.MedicalServices khi DB đã sẵn sàng
            return View(services); 
        }
        
        public IActionResult QuanLyGia() 
        { 
            var services = new List<MedicalService>(); // Thêm mock data hoặc query _context.MedicalServices khi DB đã sẵn sàng
            return View(services); 
        }
        
        public IActionResult QuanLyKhoDuoc() { return View(); }
        
        public IActionResult QuanLyThietBi() 
        { 
            var equipments = new List<MedicalEquipment>(); // Thêm mock data hoặc query _context.MedicalEquipments khi DB đã sẵn sàng
            return View(equipments); 
        }
        
        public IActionResult ThongKeDoanhThu() { return View(); }
        public IActionResult CauHinhHeThong() { return View(); }
        public IActionResult SaoLuuDuLieu() { return View(); }
        public IActionResult NhatKyHeThong() { return View(); }

        // 11 - 20: Các hàm mới
        public async Task<IActionResult> QuanLyBenhNhan()
        {
            var danhSachBenhNhan = await _context.Users
                .Where(u => u.Role == "BenhNhan")
                .ToListAsync();
            return View(danhSachBenhNhan);
        }

        [HttpGet]
        public IActionResult ThemBenhNhan()
        {
            return View(new User { Role = "BenhNhan" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBenhNhan(User user)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác.");
                    return View(user);
                }

                user.Role = "BenhNhan";
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(QuanLyBenhNhan));
            }

            user.Role = "BenhNhan";
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditBenhNhan(int id)
        {
            var benhNhan = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "BenhNhan");
            if (benhNhan == null)
            {
                return NotFound();
            }

            return View(benhNhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBenhNhan(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var benhNhan = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "BenhNhan");
                if (benhNhan == null)
                {
                    return NotFound();
                }

                if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.Id != id))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác.");
                    return View(user);
                }

                benhNhan.Username = user.Username;
                benhNhan.Password = user.Password;
                benhNhan.FullName = user.FullName;
                benhNhan.Email = user.Email;
                benhNhan.SDT = user.SDT;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(QuanLyBenhNhan));
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBenhNhan(int id)
        {
            var benhNhan = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "BenhNhan");
            if (benhNhan != null)
            {
                _context.Users.Remove(benhNhan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(QuanLyBenhNhan));
        }

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