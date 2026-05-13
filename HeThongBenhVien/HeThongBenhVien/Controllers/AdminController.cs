using HeThongBenhVien.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            var totalAppointments = _context.Appointments.Count();
            var totalPatients = _context.Patients.Count();
            var totalRevenue = _context.PrescriptionDetails.Sum(d => d.Price * d.Quantity);
            var emergencyCases = _context.Appointments.Count(a =>
                (a.Status == 6 || (a.Reason != null && a.Reason.ToLower().Contains("cấp cứu")))
                && a.Status != 4 && a.Status != 5);

            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TotalPatients = totalPatients;
            ViewBag.PatientOccupancy = totalPatients;
            ViewBag.DailyRevenue = totalRevenue;
            ViewBag.EmergencyCases = emergencyCases;

            return View();
        }

        // ==========================================
        // QUẢN LÝ NHÂN SỰ
        // ==========================================
        public async Task<IActionResult> QuanLyNhanSu()
        {
            var danhSachNhanSu = await _context.Users.ToListAsync();
            return View(danhSachNhanSu);
        }

        [HttpGet]
        public IActionResult ThemNhanSu()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemNhanSu(User user)
        {
            if (ModelState.IsValid)
            {
                var daTonTai = await _context.Users.AnyAsync(u => u.Username == user.Username);
                if (daTonTai)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác!");
                    return View(user);
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(QuanLyNhanSu));
            }
            return View(user);
        }

        // ==========================================
        // QUẢN LÝ TÀI KHOẢN & BỆNH NHÂN
        // ==========================================
        public IActionResult QuanLyTaiKhoan()
        {
            return RedirectToAction(nameof(QuanLyBenhNhan));
        }

        public async Task<IActionResult> QuanLyBenhNhan()
        {
            var danhSachBenhNhan = await _context.Users.Where(u => u.Role == "BenhNhan").ToListAsync();
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
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
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
            if (benhNhan == null) return NotFound();
            return View(benhNhan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBenhNhan(int id, User user)
        {
            if (id != user.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var benhNhan = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == "BenhNhan");
                if (benhNhan == null) return NotFound();

                if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.Id != id))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
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

        // ==========================================
        // QUẢN LÝ LỊCH LÀM VIỆC (ĐÃ FIX TÌM KIẾM & BỘ LỌC)
        // ==========================================
        public async Task<IActionResult> QuanLyLichLamViec(int? month, int? year, string searchString)
        {
            // Mặc định lấy tháng và năm hiện tại nếu không chọn
            int currentMonth = month ?? DateTime.Now.Month;
            int currentYear = year ?? DateTime.Now.Year;

            // Khởi tạo truy vấn
            var query = _context.LichLamViecs
                .Include(l => l.User)
                .Where(l => l.User != null && l.User.Role == "Doctor")
                .Where(l => l.MonthNumber == currentMonth && l.YearNumber == currentYear);

            // Xử lý tìm kiếm theo tên bác sĩ
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(l => l.User.FullName.ToLower().Contains(searchString.ToLower()));
            }

            var danhSachLich = await query.OrderBy(l => l.WorkDate).ToListAsync();

            // Truyền dữ liệu về View để giữ trạng thái bộ lọc
            ViewBag.CurrentMonth = currentMonth;
            ViewBag.CurrentYear = currentYear;
            ViewBag.SearchString = searchString;

            // Truyền danh sách Bác sĩ ra Modal
            ViewBag.DanhSachBacSi = await _context.Users
                .Where(u => u.Role == "Doctor")
                .ToListAsync();

            return View(danhSachLich);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemLichLamViec(int UserId, DateTime WorkDate, string ShiftName)
        {
            if (UserId == 0 || WorkDate == default || string.IsNullOrEmpty(ShiftName))
            {
                return RedirectToAction(nameof(QuanLyLichLamViec));
            }

            string workTime = "";
            if (ShiftName.Contains("("))
            {
                workTime = ShiftName.Split('(')[1].Replace(")", "").Trim();
            }

            var lichMoi = new LichLamViec
            {
                UserId = UserId,
                WorkDate = WorkDate.Date,
                ShiftName = ShiftName,
                WorkTime = workTime,
                WeekNumber = System.Globalization.ISOWeek.GetWeekOfYear(WorkDate),
                MonthNumber = WorkDate.Month,
                YearNumber = WorkDate.Year
            };

            _context.LichLamViecs.Add(lichMoi);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(QuanLyLichLamViec));
        }

        // ==========================================
        // CÁC CHỨC NĂNG CÒN LẠI
        // ==========================================
        public async Task<IActionResult> QuanLyKhoDuoc()
        {
            var allMedicines = await _context.Medicines.ToListAsync();
            var allPrescriptionDetails = await _context.PrescriptionDetails
                .Include(pd => pd.Prescription)
                .ThenInclude(p => p.MedicalRecord)
                .ThenInclude(m => m.Appointment)
                .ThenInclude(a => a.Patient)
                .ToListAsync();

            ViewBag.TotalMedicines = allMedicines.Count;
            ViewBag.AllMedicines = allMedicines;
            ViewBag.AllPrescriptionDetails = allPrescriptionDetails;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemThuoc(string medicineName, decimal medicinePrice)
        {
            if (!string.IsNullOrEmpty(medicineName) && medicinePrice > 0)
            {
                medicineName = medicineName.Trim();
                var existingMedicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Name.ToLower() == medicineName.ToLower());

                if (existingMedicine != null)
                {
                    // Cập nhật giá nếu khác
                    if (existingMedicine.Price != medicinePrice)
                    {
                        existingMedicine.Price = medicinePrice;
                        _context.Medicines.Update(existingMedicine);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Thêm thuốc mới
                    var newMedicine = new Medicine
                    {
                        Name = medicineName,
                        Price = medicinePrice
                    };
                    _context.Medicines.Add(newMedicine);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(QuanLyKhoDuoc));
        }

        public IActionResult QuanLyKhoaPhong() { return View(); }
        public IActionResult QuanLyDichVu() { return View(); }
        public IActionResult QuanLyGia() { return View(); }
        public IActionResult QuanLyThietBi() { return View(); }
        public IActionResult ThongKeDoanhThu() { return View(); }
        public IActionResult CauHinhHeThong() { return View(); }
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