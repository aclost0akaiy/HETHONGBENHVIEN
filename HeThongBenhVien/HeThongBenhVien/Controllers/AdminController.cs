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

            // --- Dữ liệu biểu đồ tuần (7 ngày gần nhất) ---
            var today = DateTime.Today;
            var weekStart = today.AddDays(-6); // 7 ngày: từ 6 ngày trước đến hôm nay

            var chartLabels = new List<string>();
            var chartAppointments = new List<int>();
            var chartRevenue = new List<decimal>();

            // Pre-load all appointments and prescription details in the date range
            var weekAppointments = _context.Appointments
                .Where(a => a.AppointmentTime.Date >= weekStart && a.AppointmentTime.Date <= today)
                .ToList();

            var weekRecordIds = _context.MedicalRecords
                .Where(m => m.CreatedAt.Date >= weekStart && m.CreatedAt.Date <= today)
                .Select(m => new { m.Id, m.CreatedAt })
                .ToList();

            var allPrescriptionDetails = _context.PrescriptionDetails
                .Include(pd => pd.Prescription)
                .Where(pd => pd.Prescription != null)
                .ToList();

            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                // Nhãn: Thứ + ngày/tháng
                string[] dayNames = { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };
                chartLabels.Add($"{dayNames[(int)date.DayOfWeek]} {date:dd/MM}");

                // Số lượt khám trong ngày
                chartAppointments.Add(weekAppointments.Count(a => a.AppointmentTime.Date == date));

                // Doanh thu trong ngày (dựa trên ngày tạo MedicalRecord)
                var recordIdsForDay = weekRecordIds
                    .Where(m => m.CreatedAt.Date == date)
                    .Select(m => m.Id)
                    .ToList();

                var dayRevenue = allPrescriptionDetails
                    .Where(pd => pd.Prescription != null && recordIdsForDay.Contains(pd.Prescription.MedicalRecordId))
                    .Sum(pd => pd.Price * pd.Quantity);

                chartRevenue.Add(dayRevenue);
            }

            ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(chartLabels);
            ViewBag.ChartAppointments = System.Text.Json.JsonSerializer.Serialize(chartAppointments);
            ViewBag.ChartRevenue = System.Text.Json.JsonSerializer.Serialize(chartRevenue);

            // Tổng doanh thu tuần
            ViewBag.WeeklyRevenue = chartRevenue.Sum();

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
        // QUẢN LÝ KHOA PHÒNG
        // ==========================================
        public async Task<IActionResult> QuanLyKhoaPhong()
        {
            var departments = await _context.Departments.ToListAsync();
            return View(departments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemKhoaPhong(Department dept)
        {
            if (!string.IsNullOrEmpty(dept.DepartmentName))
            {
                _context.Departments.Add(dept);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyKhoaPhong));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaKhoaPhong(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept != null)
            {
                _context.Departments.Remove(dept);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyKhoaPhong));
        }

        // ==========================================
        // QUẢN LÝ KHO DƯỢC
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
                    if (existingMedicine.Price != medicinePrice)
                    {
                        existingMedicine.Price = medicinePrice;
                        _context.Medicines.Update(existingMedicine);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
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

        // ==========================================
        // QUẢN LÝ DỊCH VỤ KHÁM CHỮA BỆNH
        // ==========================================
        public async Task<IActionResult> QuanLyDichVu()
        {
            var services = await _context.MedicalServices.ToListAsync();
            return View(services);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDichVu(MedicalService svc)
        {
            if (!string.IsNullOrEmpty(svc.ServiceName))
            {
                svc.IsActive = true;
                _context.MedicalServices.Add(svc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyDichVu));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaDichVu(int id)
        {
            var svc = await _context.MedicalServices.FindAsync(id);
            if (svc != null)
            {
                _context.MedicalServices.Remove(svc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyDichVu));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDichVu(int id)
        {
            var svc = await _context.MedicalServices.FindAsync(id);
            if (svc != null)
            {
                svc.IsActive = !svc.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyDichVu));
        }

        // ==========================================
        // BẢNG GIÁ VIỆN PHÍ
        // ==========================================
        public async Task<IActionResult> QuanLyGia()
        {
            var services = await _context.MedicalServices.Where(s => s.IsActive).ToListAsync();
            var medicines = await _context.Medicines.ToListAsync();
            ViewBag.Services = services;
            ViewBag.Medicines = medicines;
            return View();
        }

        // ==========================================
        // QUẢN LÝ THIẾT BỊ Y TẾ
        // ==========================================
        public async Task<IActionResult> QuanLyThietBi()
        {
            var equipments = await _context.MedicalEquipments.ToListAsync();
            return View(equipments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemThietBi(MedicalEquipment eq)
        {
            if (!string.IsNullOrEmpty(eq.EquipmentName))
            {
                _context.MedicalEquipments.Add(eq);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyThietBi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaThietBi(int id)
        {
            var eq = await _context.MedicalEquipments.FindAsync(id);
            if (eq != null)
            {
                _context.MedicalEquipments.Remove(eq);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyThietBi));
        }

        // ==========================================
        // THỐNG KÊ DOANH THU
        // ==========================================
        public async Task<IActionResult> ThongKeDoanhThu()
        {
            var today = DateTime.Today;

            // Doanh thu hôm nay
            var todayRecordIds = await _context.MedicalRecords
                .Where(m => m.CreatedAt.Date == today)
                .Select(m => m.Id).ToListAsync();
            var todayRevenue = await _context.PrescriptionDetails
                .Include(pd => pd.Prescription)
                .Where(pd => pd.Prescription != null && todayRecordIds.Contains(pd.Prescription.MedicalRecordId))
                .SumAsync(pd => pd.Price * pd.Quantity);

            // Doanh thu tháng này
            var monthRecordIds = await _context.MedicalRecords
                .Where(m => m.CreatedAt.Month == today.Month && m.CreatedAt.Year == today.Year)
                .Select(m => m.Id).ToListAsync();
            var monthRevenue = await _context.PrescriptionDetails
                .Include(pd => pd.Prescription)
                .Where(pd => pd.Prescription != null && monthRecordIds.Contains(pd.Prescription.MedicalRecordId))
                .SumAsync(pd => pd.Price * pd.Quantity);

            // Tổng doanh thu
            var totalRevenue = await _context.PrescriptionDetails.SumAsync(pd => pd.Price * pd.Quantity);

            // Tổng số đơn thuốc
            var totalPrescriptions = await _context.Prescriptions.CountAsync();

            // Dữ liệu 12 tháng gần nhất cho biểu đồ
            var monthLabels = new List<string>();
            var monthRevenueData = new List<decimal>();
            var monthAppointmentData = new List<int>();

            for (int i = 11; i >= 0; i--)
            {
                var monthDate = today.AddMonths(-i);
                monthLabels.Add($"T{monthDate.Month}/{monthDate.Year % 100}");

                var mRecIds = _context.MedicalRecords
                    .Where(m => m.CreatedAt.Month == monthDate.Month && m.CreatedAt.Year == monthDate.Year)
                    .Select(m => m.Id).ToList();

                var mRevenue = _context.PrescriptionDetails
                    .Include(pd => pd.Prescription)
                    .Where(pd => pd.Prescription != null && mRecIds.Contains(pd.Prescription.MedicalRecordId))
                    .Sum(pd => pd.Price * pd.Quantity);
                monthRevenueData.Add(mRevenue);

                var mAppts = _context.Appointments
                    .Count(a => a.AppointmentTime.Month == monthDate.Month && a.AppointmentTime.Year == monthDate.Year);
                monthAppointmentData.Add(mAppts);
            }

            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.MonthRevenue = monthRevenue;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalPrescriptions = totalPrescriptions;
            ViewBag.MonthLabels = System.Text.Json.JsonSerializer.Serialize(monthLabels);
            ViewBag.MonthRevenueData = System.Text.Json.JsonSerializer.Serialize(monthRevenueData);
            ViewBag.MonthAppointmentData = System.Text.Json.JsonSerializer.Serialize(monthAppointmentData);

            return View();
        }

        // ==========================================
        // BÁO CÁO HOẠT ĐỘNG
        // ==========================================
        public async Task<IActionResult> BaoCaoHoatDong()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Thống kê chung
            ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalDoctors = await _context.Users.CountAsync(u => u.Role == "Doctor");
            ViewBag.TotalMedicines = await _context.Medicines.CountAsync();

            // Thống kê tháng
            ViewBag.MonthAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentTime >= startOfMonth);
            ViewBag.MonthCompleted = await _context.Appointments
                .CountAsync(a => a.AppointmentTime >= startOfMonth && (a.Status == 4 || a.Status == 5));
            ViewBag.MonthPending = await _context.Appointments
                .CountAsync(a => a.AppointmentTime >= startOfMonth && a.Status < 4);

            // Thống kê theo trạng thái
            ViewBag.StatusWaiting = await _context.Appointments.CountAsync(a => a.Status == 0 || a.Status == 1);
            ViewBag.StatusExamining = await _context.Appointments.CountAsync(a => a.Status == 2);
            ViewBag.StatusCompleted = await _context.Appointments.CountAsync(a => a.Status == 4 || a.Status == 5);
            ViewBag.StatusEmergency = await _context.Appointments.CountAsync(a => a.Status == 6);

            // Top bác sĩ (số lượt khám)
            var topDoctors = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Where(m => m.Appointment != null)
                .GroupBy(m => m.Appointment!.PatientId)
                .Select(g => new { PatientId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();
            ViewBag.TopDoctorData = topDoctors;

            return View();
        }

        // ==========================================
        // ĐÁNH GIÁ CHẤT LƯỢNG
        // ==========================================
        public async Task<IActionResult> DanhGiaChatLuong()
        {
            // Tỷ lệ khám xong / tổng
            var totalAppts = await _context.Appointments.CountAsync();
            var completedAppts = await _context.Appointments.CountAsync(a => a.Status == 4 || a.Status == 5);
            ViewBag.CompletionRate = totalAppts > 0 ? Math.Round((double)completedAppts / totalAppts * 100, 1) : 0;

            // Thời gian trung bình (giả lập)
            ViewBag.AvgWaitTime = 15; // phút
            ViewBag.AvgExamTime = 25; // phút

            // Tổng số bác sĩ & bệnh nhân
            ViewBag.TotalDoctors = await _context.Users.CountAsync(u => u.Role == "Doctor");
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalServices = await _context.MedicalServices.CountAsync(s => s.IsActive);
            ViewBag.TotalEquipments = await _context.MedicalEquipments.CountAsync();

            return View();
        }

        // ==========================================
        // CẤU HÌNH HỆ THỐNG
        // ==========================================
        public IActionResult CauHinhHeThong()
        {
            return View();
        }

        // ==========================================
        // CÁC CHỨC NĂNG KHÁC (giữ nguyên)
        // ==========================================
        public IActionResult QuanLyVienPhi() { return View(); }
        public IActionResult QuanLyBHYT() { return View(); }
        public IActionResult QuanLyTiepDon() { return View(); }
        public IActionResult QuanLyXetNghiem() { return View(); }
        public IActionResult QuanLyCDHA() { return View(); }
        public IActionResult QuanLyPhauThuat() { return View(); }
        public IActionResult QuanLyNganHangMau() { return View(); }
        public IActionResult SaoLuuDuLieu() { return View(); }
        public IActionResult NhatKyHeThong() { return View(); }
    }
}