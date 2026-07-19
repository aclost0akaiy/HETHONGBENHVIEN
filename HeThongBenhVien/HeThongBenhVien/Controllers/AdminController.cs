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
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            // Tổng lượt khám (Today vs Yesterday)
            var appointmentsToday = _context.Appointments.Count(a => a.AppointmentTime.Date == today);
            var appointmentsYesterday = _context.Appointments.Count(a => a.AppointmentTime.Date == yesterday);
            var totalAppointments = _context.Appointments.Count(); // Giữ nguyên tổng hoặc dùng số hôm nay. Đề bài hay dùng tổng.
            var apptPercent = appointmentsYesterday == 0 ? (appointmentsToday > 0 ? 100 : 0) : Math.Round((double)(appointmentsToday - appointmentsYesterday) / appointmentsYesterday * 100, 1);

            // Doanh thu ngày
            var revenueToday = _context.Prescriptions
                .Where(p => p.CreatedAt.Date == today)
                .SelectMany(p => p.PrescriptionDetails)
                .Sum(pd => pd.Price * pd.Quantity);
            var revenueYesterday = _context.Prescriptions
                .Where(p => p.CreatedAt.Date == yesterday)
                .SelectMany(p => p.PrescriptionDetails)
                .Sum(pd => pd.Price * pd.Quantity);

            var revPercent = revenueYesterday == 0 ? (revenueToday > 0 ? 100 : 0) : Math.Round((double)(revenueToday - revenueYesterday) / (double)revenueYesterday * 100, 1);


            // Công suất giường
            var patientOccupancy = _context.MedicalRecords.Count(r => r.AdmissionDate != null && r.DischargeDate == null);
            var totalBeds = _context.Departments.Sum(d => d.TotalBeds);
            if (totalBeds == 0) totalBeds = 500;
            var occupancyRate = Math.Round((double)patientOccupancy / totalBeds * 100, 1);

            // Tiến độ tải khoa phòng
            var departments = _context.Departments.ToList();
            foreach (var d in departments)
            {
                d.OccupiedBeds = _context.MedicalRecords.Count(r => r.DepartmentId == d.Id && r.AdmissionDate != null && r.DischargeDate == null);
            }
            ViewBag.DepartmentLoads = departments.OrderByDescending(d => d.TotalBeds > 0 ? (d.OccupiedBeds * 100 / d.TotalBeds) : 0).Take(4).ToList();

            // Ca cấp cứu
            var emergencyCases = _context.Appointments.Count(a => 
                (a.Status == 6 || (a.Reason != null && a.Reason.ToLower().Contains("cấp cứu"))) && a.Status != 4 && a.Status != 5);
            var emergencyToday = _context.Appointments.Count(a => 
                (a.Status == 6 || (a.Reason != null && a.Reason.ToLower().Contains("cấp cứu"))) && a.Status != 4 && a.Status != 5 && a.AppointmentTime.Date == today);
            var emergencyYesterday = _context.Appointments.Count(a => 
                (a.Status == 6 || (a.Reason != null && a.Reason.ToLower().Contains("cấp cứu"))) && a.Status != 4 && a.Status != 5 && a.AppointmentTime.Date == yesterday);
            var emergencyPercent = emergencyYesterday == 0 ? (emergencyToday > 0 ? 100 : 0) : Math.Round((double)(emergencyToday - emergencyYesterday) / emergencyYesterday * 100, 1);

            var startDate = DateTime.Today.AddDays(-6);
            var endDate = DateTime.Today.AddDays(1);

            var weeklyAppointments = _context.Appointments
                .Where(a => a.AppointmentTime >= startDate && a.AppointmentTime < endDate)
                .AsEnumerable()
                .GroupBy(a => a.AppointmentTime.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var weeklyRevenue = _context.Prescriptions
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt < endDate)
                .SelectMany(p => p.PrescriptionDetails, (prescription, detail) => new { prescription.CreatedAt, detail.Price, detail.Quantity })
                .AsEnumerable()
                .GroupBy(x => x.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Price * x.Quantity));

            var dateKeys = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i))
                .ToList();

            var labels = dateKeys
                .Select(date => date.ToString("dd/MM"))
                .ToArray();

            var counts = dateKeys
                .Select(date => weeklyAppointments.TryGetValue(date, out var count) ? count : 0)
                .ToArray();

            var revenues = dateKeys
                .Select(date => weeklyRevenue.TryGetValue(date, out var value) ? value : 0m)
                .ToArray();

            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.DailyRevenue = revenueToday; // Thay bằng doanh thu hôm nay
            ViewBag.PatientOccupancy = patientOccupancy;
            ViewBag.TotalBeds = totalBeds;
            ViewBag.EmergencyCases = emergencyCases;

            ViewBag.ApptPercent = apptPercent;
            ViewBag.RevPercent = revPercent;
            ViewBag.OccupancyRate = occupancyRate;
            ViewBag.EmergencyPercent = emergencyPercent;
            ViewBag.WeeklyChartLabels = labels;
            ViewBag.WeeklyChartCounts = counts;
            ViewBag.WeeklyChartRevenues = revenues;

            return View();
        }

        // ==========================================
        // QUẢN LÝ NHÂN SỰ
        // ==========================================
        public async Task<IActionResult> QuanLyNhanSu()
        {
            // Lấy toàn bộ nhân sự để hiển thị, bao gồm cả khoa phòng
            var danhSachNhanSu = await _context.Users
                .Include(u => u.Department)
                .Where(u => u.Role == "Doctor" || u.Role == "Admin")
                .ToListAsync();

            // Tính số bệnh nhân đang chờ khám của từng bác sĩ (Status = 1, 2, 3)
            var activeApptCounts = await _context.Appointments
                .Where(a => a.Status == 1 || a.Status == 2 || a.Status == 3)
                .GroupBy(a => a.DoctorId)
                .Select(g => new { DoctorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DoctorId ?? 0, x => x.Count);

            ViewBag.ActiveApptCounts = activeApptCounts;
            ViewBag.CurrentUsername = User?.Identity?.Name;
            ViewBag.AllDoctors = danhSachNhanSu.Where(u => u.Role == "Doctor").ToList();

            var sbarLogs = await _context.PatientTransferLogs
                .Include(l => l.FromDoctor)
                .Include(l => l.ToDoctor)
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
            ViewBag.SbarLogs = sbarLogs;

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

        [HttpPost]
        public async Task<IActionResult> SendDoctorSMS(int doctorId, string message)
        {
            // Không cho phép gửi cho chính mình (Admin)
            var senderUsername = User?.Identity?.Name;
            var sender = await _context.Users.FirstOrDefaultAsync(u => u.Username == senderUsername);
            if (sender != null && sender.Id == doctorId)
            {
                return Json(new { success = false, message = "Không thể gửi tin nhắn cho chính mình!" });
            }

            var doctor = await _context.Users.FindAsync(doctorId);
            if (doctor == null) return NotFound();

            // Chỉ gửi cho bác sĩ (role Doctor)
            if (doctor.Role != "Doctor")
            {
                return Json(new { success = false, message = "Chỉ có thể gửi tin nhắn cho Bác sĩ!" });
            }

            var notification = new Notification
            {
                DoctorId = doctorId,
                Message = message + $" (Gửi lúc: {DateTime.Now:dd/MM/yyyy HH:mm})",
                Type = NotificationType.AdminMessage,
                IsRead = false,
                IsForPatient = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã gửi tin nhắn đến bác sĩ " + doctor.FullName });
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

                // Tự động tạo PatientCode
                string patientCode = "BN" + DateTime.Now.ToString("yyMMddHHmmss");
                user.PatientCode = patientCode;
                user.Role = "BenhNhan";

                // Tạo bản ghi Patient tương ứng
                var patient = new Patient
                {
                    FullName = user.FullName ?? "Bệnh nhân mới",
                    PatientCode = patientCode,
                    CCCD = user.SDT, // Tạm dùng SDT làm CCCD nếu không có
                    Age = 30, // Giá trị mặc định
                    Gender = "Khác"
                };

                _context.Patients.Add(patient);
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

            // Lấy danh sách xét nghiệm của bệnh nhân này thông qua PatientCode
            var patientLabTests = new List<LabTest>();
            if (!string.IsNullOrEmpty(benhNhan.PatientCode))
            {
                patientLabTests = await _context.LabTests
                    .Include(l => l.MedicalRecord)
                        .ThenInclude(m => m.Appointment)
                            .ThenInclude(a => a.Patient)
                    .Where(l => l.MedicalRecord.Appointment.Patient.PatientCode == benhNhan.PatientCode)
                    .OrderByDescending(l => l.CreatedAt)
                    .ToListAsync();
            }
            
            ViewBag.LabTests = patientLabTests;
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

                // Nếu chưa có PatientCode, tạo mới và tạo bản ghi Patient
                if (string.IsNullOrEmpty(benhNhan.PatientCode))
                {
                    string patientCode = "BN" + DateTime.Now.ToString("yyMMddHHmmss");
                    benhNhan.PatientCode = patientCode;

                    var patient = new Patient
                    {
                        FullName = benhNhan.FullName ?? "Bệnh nhân cũ",
                        PatientCode = patientCode,
                        CCCD = benhNhan.SDT,
                        Age = 30,
                        Gender = "Khác"
                    };
                    _context.Patients.Add(patient);
                }

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
            var list = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            foreach (var dept in list)
            {
                dept.OccupiedBeds = await _context.MedicalRecords.CountAsync(r => 
                    r.DepartmentId == dept.Id && 
                    r.AdmissionDate != null && 
                    r.DischargeDate == null);
            }
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemKhoaPhong(Department dept)
        {
            if (!string.IsNullOrEmpty(dept.DepartmentName))
            {
                // Tự động sinh mã khoa theo thứ tự 1, 2, 3, ...
                var maxCode = await _context.Departments
                    .Select(d => d.DepartmentCode)
                    .ToListAsync();
                int nextNum = 1;
                if (maxCode.Any())
                {
                    var nums = maxCode
                        .Select(c => { int n; return int.TryParse(c, out n) ? n : 0; })
                        .ToList();
                    nextNum = nums.Max() + 1;
                }
                dept.DepartmentCode = nextNum.ToString();
                dept.TotalBeds = 50; // Force 50 beds
                
                // Đảm bảo không bị null khi lưu vào database
                dept.Description = dept.Description ?? string.Empty;
                dept.HeadDoctor = dept.HeadDoctor ?? string.Empty;
                dept.Phone = dept.Phone ?? string.Empty;

                _context.Departments.Add(dept);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyKhoaPhong));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatKhoaPhong(Department dept)
        {
            if (dept.Id > 0)
            {
                var existing = await _context.Departments.FindAsync(dept.Id);
                if (existing != null)
                {
                    existing.DepartmentCode = dept.DepartmentCode;
                    existing.DepartmentName = dept.DepartmentName;
                    existing.Description = dept.Description ?? string.Empty;
                    existing.HeadDoctor = dept.HeadDoctor ?? string.Empty;
                    existing.TotalBeds = 50; // Force 50 beds
                    existing.Phone = dept.Phone ?? string.Empty;
                    existing.IsActive = dept.IsActive;
                    
                    _context.Departments.Update(existing);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(QuanLyKhoaPhong));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaKhoaPhong(int id)
        {
            var item = await _context.Departments.FindAsync(id);
            if (item != null) { _context.Departments.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(QuanLyKhoaPhong));
        }

        // ==========================================
        // QUẢN LÝ DỊCH VỤ
        // ==========================================
        public async Task<IActionResult> QuanLyDichVu()
        {
            var list = await _context.MedicalServices.OrderBy(s => s.ServiceName).ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDichVu(MedicalService svc)
        {
            if (!string.IsNullOrEmpty(svc.ServiceName))
            {
                _context.MedicalServices.Add(svc);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyDichVu));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaDichVu(int id)
        {
            var item = await _context.MedicalServices.FindAsync(id);
            if (item != null) { _context.MedicalServices.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(QuanLyDichVu));
        }

        // ==========================================
        // BẢNG GIÁ VIỆN PHÍ
        // ==========================================
        public async Task<IActionResult> QuanLyGia()
        {
            var list = await _context.HospitalFees.OrderBy(f => f.Category).ThenBy(f => f.FeeName).ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemGia(HospitalFee fee)
        {
            if (!string.IsNullOrEmpty(fee.FeeName))
            {
                _context.HospitalFees.Add(fee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyGia));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaGia(int id)
        {
            var item = await _context.HospitalFees.FindAsync(id);
            if (item != null) { _context.HospitalFees.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(QuanLyGia));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaGia(int id, string feeName, decimal price, int insuranceCoverage)
        {
            var fee = await _context.HospitalFees.FindAsync(id);
            if (fee != null)
            {
                fee.FeeName = feeName;
                fee.Price = price;
                fee.InsuranceCoverage = insuranceCoverage;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyGia));
        }

        // ==========================================
        // QUẢN LÝ KHO DƯỢC
        // ==========================================
        public async Task<IActionResult> QuanLyKhoDuoc()
        {
            // Load all medicines
            var allMedicines = await _context.Medicines.OrderBy(m => m.Name).ToListAsync();
            ViewBag.AllMedicines = allMedicines;
            ViewBag.TotalMedicines = allMedicines.Count;

            // Advanced Pharmacy Management: Cảnh báo sắp hết hạn / sắp hết tồn kho
            var lowStockMedicines = allMedicines.Where(m => m.StockQuantity <= m.MinStock && m.IsActive).ToList();
            var expiredMedicines = allMedicines.Where(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value < DateTime.Now && m.IsActive).ToList();
            var expiring1MonthMedicines = allMedicines.Where(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value >= DateTime.Now && m.ExpiryDate.Value <= DateTime.Now.AddDays(30) && m.IsActive).ToList();
            var expiring3MonthsMedicines = allMedicines.Where(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value > DateTime.Now.AddDays(30) && m.ExpiryDate.Value <= DateTime.Now.AddMonths(3) && m.IsActive).ToList();
            var nearlyOutOfStockMedicines = allMedicines.Where(m => m.StockQuantity <= 1000 && m.IsActive).ToList();
            
            ViewBag.LowStockMedicines = lowStockMedicines;
            ViewBag.ExpiredMedicines = expiredMedicines;
            ViewBag.Expiring1MonthMedicines = expiring1MonthMedicines;
            ViewBag.Expiring3MonthsMedicines = expiring3MonthsMedicines;
            ViewBag.NearlyOutOfStockMedicines = nearlyOutOfStockMedicines;

            // Load all prescription details with patient relationships
            var allPrescriptionDetails = await _context.PrescriptionDetails
                .Include(pd => pd.Prescription)
                    .ThenInclude(p => p.MedicalRecord)
                        .ThenInclude(mr => mr.Appointment)
                            .ThenInclude(a => a.Patient)
                .ToListAsync();
            ViewBag.AllPrescriptionDetails = allPrescriptionDetails;

            return View(allMedicines);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemThuoc(Medicine medicine)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(medicine.Name))
            {
                // Case-insensitive duplicate check
                var existingMedicine = await _context.Medicines
                    .FirstOrDefaultAsync(m => m.Name.ToLower() == medicine.Name.ToLower());

                if (existingMedicine != null)
                {
                    // If exists and price is different, update the price and stock
                    if (existingMedicine.Price != medicine.Price)
                    {
                        existingMedicine.Price = medicine.Price;
                    }
                    existingMedicine.StockQuantity += medicine.StockQuantity;
                    _context.Medicines.Update(existingMedicine);
                }
                else
                {
                    // Create new medicine
                    _context.Medicines.Add(medicine);
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyKhoDuoc));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaThuoc(int id)
        {
            var item = await _context.Medicines.FindAsync(id);
            if (item != null) { _context.Medicines.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(QuanLyKhoDuoc));
        }

        // ==========================================
        // QUẢN LÝ THIẾT BỊ Y TẾ
        // ==========================================
        public async Task<IActionResult> QuanLyThietBi()
        {
            var list = await _context.MedicalEquipments.OrderBy(e => e.EquipmentName).ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemThietBi(MedicalEquipment eq)
        {
            if (!string.IsNullOrEmpty(eq.EquipmentName))
            {
                _context.MedicalEquipments.Add(eq);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyThietBi));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaThietBi(int id)
        {
            var item = await _context.MedicalEquipments.FindAsync(id);
            if (item != null) { _context.MedicalEquipments.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(QuanLyThietBi));
        }

        // ==========================================
        // THỐNG KÊ DOANH THU
        // ==========================================
        public async Task<IActionResult> ThongKeDoanhThu()
        {
            var totalRecords = await _context.MedicalRecords.CountAsync();
            var totalPatients = await _context.Patients.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();

            ViewBag.TotalRevenue = totalRecords * 500000;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPatients = totalPatients;
            ViewBag.TotalAppointments = totalAppointments;

            // Dữ liệu biểu đồ 7 ngày
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Now.Date.AddDays(-6 + i)).ToList();
            var dailyData = last7Days.Select(d => new {
                Date = d.ToString("dd/MM"),
                Count = _context.MedicalRecords.Count(r => r.CreatedAt.Date == d),
                Revenue = _context.MedicalRecords.Count(r => r.CreatedAt.Date == d) * 500000
            }).ToList();

            ViewBag.ChartLabels = dailyData.Select(x => x.Date).ToArray();
            ViewBag.ChartCounts = dailyData.Select(x => x.Count).ToArray();
            ViewBag.ChartRevenues = dailyData.Select(x => x.Revenue).ToArray();

            return View();
        }

        // ==========================================
        // CẤU HÌNH HỆ THỐNG
        // ==========================================

        public IActionResult CauHinhHeThong() { return View(); }

        // ==========================================
        // QUẢN LÝ VIỆN PHÍ (THANH TOÁN)
        // ==========================================
        public async Task<IActionResult> QuanLyVienPhi()
        {
            var records = await _context.MedicalRecords
                .Include(r => r.Appointment).ThenInclude(a => a.Patient)
                .OrderByDescending(r => r.CreatedAt).ToListAsync();
            return View(records);
        }

        // ==========================================
        // QUẢN LÝ BHYT
        // ==========================================
        public async Task<IActionResult> QuanLyBHYT()
        {
            var list = await _context.InsuranceCards.Include(i => i.Patient).OrderByDescending(i => i.CreatedAt).ToListAsync();
            ViewBag.Patients = await _context.Patients.ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBHYT(InsuranceCard card)
        {
            if (!string.IsNullOrEmpty(card.CardNumber))
            {
                _context.InsuranceCards.Add(card);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyBHYT));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaBHYT(int id)
        {
            var item = await _context.InsuranceCards.FindAsync(id);
            if (item != null) { _context.InsuranceCards.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(QuanLyBHYT));
        }

        // ==========================================
        // BÁO CÁO HOẠT ĐỘNG
        // ==========================================
        public async Task<IActionResult> BaoCaoHoatDong()
        {
            ViewBag.TotalPatients = await _context.Patients.CountAsync();
            ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
            ViewBag.TotalRecords = await _context.MedicalRecords.CountAsync();
            ViewBag.TotalDoctors = await _context.Users.CountAsync(u => u.Role == "Doctor");
            ViewBag.TotalLabTests = await _context.LabTests.CountAsync();
            ViewBag.TotalMedicines = await _context.Medicines.CountAsync();
            ViewBag.TotalDepartments = await _context.Departments.CountAsync();
            ViewBag.TotalEquipments = await _context.MedicalEquipments.CountAsync();

            // 1. Phân tích loại bệnh (Pie Chart)
            var rawDiseases = await _context.MedicalRecords
                .Where(m => !string.IsNullOrEmpty(m.Diagnosis))
                .Select(m => m.Diagnosis)
                .ToListAsync();

            var diseaseGroups = rawDiseases
                .Select(d => {
                    // Extract core disease name for grouping
                    var parts = d.Split('-');
                    if (parts.Length > 1) return parts[1].Trim();
                    return d.Split(',')[0].Trim();
                })
                .GroupBy(d => d)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(6)
                .ToList();
                
            ViewBag.DiseaseLabels = System.Text.Json.JsonSerializer.Serialize(diseaseGroups.Select(x => x.Label));
            ViewBag.DiseaseData = System.Text.Json.JsonSerializer.Serialize(diseaseGroups.Select(x => x.Count));

            // 2. Doanh thu theo phòng ban (Bar Chart) - Từ tiền thuốc
            var departmentRevenueQuery = await _context.PrescriptionDetails
                .Include(pd => pd.Prescription)
                    .ThenInclude(p => p.MedicalRecord)
                        .ThenInclude(mr => mr.Department)
                .Where(pd => pd.Prescription != null 
                          && pd.Prescription.MedicalRecord != null 
                          && pd.Prescription.MedicalRecord.Department != null)
                .Select(pd => new { 
                    DepartmentName = pd.Prescription.MedicalRecord.Department.DepartmentName,
                    Revenue = pd.Price * pd.Quantity
                })
                .ToListAsync();

            var deptGroups = departmentRevenueQuery
                .GroupBy(x => x.DepartmentName)
                .Select(g => new { Label = g.Key, Total = g.Sum(x => x.Revenue) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            ViewBag.DoctorLabels = System.Text.Json.JsonSerializer.Serialize(deptGroups.Select(x => x.Label));
            ViewBag.DoctorData = System.Text.Json.JsonSerializer.Serialize(deptGroups.Select(x => x.Total));

            return View();
        }

        // ==========================================
        // QUẢN LÝ TIẾP ĐÓN
        // ==========================================
        public async Task<IActionResult> QuanLyTiepDon()
        {
            var list = await _context.Receptions.Include(r => r.Patient).OrderByDescending(r => r.CheckInTime).ToListAsync();
            ViewBag.Patients = await _context.Patients.ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemTiepDon(Reception rec)
        {
            if (rec.PatientId > 0)
            {
                rec.ReceptionCode = "TD" + DateTime.Now.ToString("yyyyMMddHHmmss");
                rec.QueueNumber = (await _context.Receptions.CountAsync(r => r.CheckInTime.Date == DateTime.Today)) + 1;
                _context.Receptions.Add(rec);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyTiepDon));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThaiTiepDon(int id, string status)
        {
            var item = await _context.Receptions.FindAsync(id);
            if (item != null)
            {
                item.Status = status;
                if (status == "Đã khám") item.CheckOutTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyTiepDon));
        }

        // ==========================================
        // QUẢN LÝ XÉT NGHIỆM
        // ==========================================
        public async Task<IActionResult> QuanLyXetNghiem()
        {
            var list = await _context.LabTests.Include(l => l.MedicalRecord)
                .ThenInclude(m => m.Appointment).ThenInclude(a => a.Patient)
                .OrderByDescending(l => l.CreatedAt).ToListAsync();
            return View(list);
        }

        // ==========================================
        // CHẨN ĐOÁN HÌNH ẢNH
        // ==========================================
        public async Task<IActionResult> QuanLyCDHA()
        {
            var list = await _context.DiagnosticImages.Include(d => d.Patient).OrderByDescending(d => d.RequestDate).ToListAsync();
            ViewBag.Patients = await _context.Patients.ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemCDHA(DiagnosticImage img)
        {
            if (img.PatientId > 0)
            {
                img.RequestCode = "CDHA" + DateTime.Now.ToString("yyyyMMddHHmmss");
                _context.DiagnosticImages.Add(img);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyCDHA));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatCDHA(int id, string status, string result, string conclusion)
        {
            var item = await _context.DiagnosticImages.FindAsync(id);
            if (item != null)
            {
                item.Status = status;
                if (!string.IsNullOrEmpty(result)) item.Result = result;
                if (!string.IsNullOrEmpty(conclusion)) item.Conclusion = conclusion;
                if (status == "Có kết quả") item.CompletedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyCDHA));
        }

        // ==========================================
        // QUẢN LÝ PHẪU THUẬT
        // ==========================================
        public async Task<IActionResult> QuanLyPhauThuat()
        {
            var list = await _context.Surgeries.Include(s => s.Patient).OrderByDescending(s => s.ScheduledDate).ToListAsync();
            ViewBag.Patients = await _context.Patients.ToListAsync();
            ViewBag.Doctors = await _context.Users.Where(u => u.Role == "Doctor").ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemPhauThuat(Surgery surgery)
        {
            if (surgery.PatientId > 0)
            {
                surgery.SurgeryCode = "PT" + DateTime.Now.ToString("yyyyMMddHHmmss");
                _context.Surgeries.Add(surgery);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyPhauThuat));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatPhauThuat(int id, string status)
        {
            var item = await _context.Surgeries.FindAsync(id);
            if (item != null) { item.Status = status; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(QuanLyPhauThuat));
        }

        // ==========================================
        // NGÂN HÀNG MÁU
        // ==========================================
        public async Task<IActionResult> QuanLyNganHangMau()
        {
            var list = await _context.BloodBanks.OrderByDescending(b => b.CollectionDate).ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemMau(BloodBank blood)
        {
            if (!string.IsNullOrEmpty(blood.BloodType))
            {
                blood.BagCode = "MAU" + DateTime.Now.ToString("yyyyMMddHHmmss");
                _context.BloodBanks.Add(blood);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyNganHangMau));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaMau(int id)
        {
            var item = await _context.BloodBanks.FindAsync(id);
            if (item != null) { _context.BloodBanks.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(QuanLyNganHangMau));
        }

        // ==========================================
        // ADMIN SBAR HANDOVER & STATUS TOGGLE
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ToggleDoctorStatus(int doctorId)
        {
            var doctor = await _context.Users.FindAsync(doctorId);
            if (doctor == null) return NotFound();

            doctor.IsBusy = !doctor.IsBusy;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isBusy = doctor.IsBusy });
        }

        [HttpPost]
        public async Task<IActionResult> TransferAllActiveDoctorsAdmin(int fromDoctorId, List<int> targetDoctorIds, List<int> patientCounts, string situation, string background, string assessment, string recommendation, bool isEmergencyConsultation)
        {
            var fromDoctor = await _context.Users.FindAsync(fromDoctorId);
            if (fromDoctor == null) return NotFound();

            // Lấy tất cả các ca khám đang chờ của bác sĩ này
            var activeAppointments = await _context.Appointments
                .Where(a => a.DoctorId == fromDoctorId && 
                            (a.Status == 1 || a.Status == 2 || a.Status == 3))
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            fromDoctor.IsBusy = true;

            int apptIndex = 0;
            int totalTransferred = 0;

            if (targetDoctorIds != null && patientCounts != null)
            {
                for (int i = 0; i < targetDoctorIds.Count; i++)
                {
                    int targetDocId = targetDoctorIds[i];
                    int countToTransfer = patientCounts[i];

                    var targetDoctor = await _context.Users.FindAsync(targetDocId);
                    if (targetDoctor == null) continue;

                    int transferredForThisDoc = 0;
                    while (transferredForThisDoc < countToTransfer && apptIndex < activeAppointments.Count)
                    {
                        var appt = activeAppointments[apptIndex];
                        appt.DoctorId = targetDocId;

                        var transferLog = new PatientTransferLog
                        {
                            AppointmentId = appt.Id,
                            FromDoctorId = fromDoctorId,
                            ToDoctorId = targetDocId,
                            Situation = situation,
                            Background = background,
                            Assessment = assessment,
                            Recommendation = recommendation,
                            IsEmergencyConsultation = isEmergencyConsultation,
                            CreatedAt = DateTime.Now
                        };
                        _context.PatientTransferLogs.Add(transferLog);

                        transferredForThisDoc++;
                        apptIndex++;
                        totalTransferred++;
                    }
                }
            }

            // Nếu vẫn còn sót bệnh nhân nào chưa được gán, chuyển sang bác sĩ đầu tiên trong danh sách nhận bàn giao
            if (apptIndex < activeAppointments.Count && targetDoctorIds != null && targetDoctorIds.Count > 0)
            {
                int firstDocId = targetDoctorIds[0];
                while (apptIndex < activeAppointments.Count)
                {
                    var appt = activeAppointments[apptIndex];
                    appt.DoctorId = firstDocId;

                    var transferLog = new PatientTransferLog
                    {
                        AppointmentId = appt.Id,
                        FromDoctorId = fromDoctorId,
                        ToDoctorId = firstDocId,
                        Situation = situation,
                        Background = background,
                        Assessment = assessment,
                        Recommendation = recommendation,
                        IsEmergencyConsultation = isEmergencyConsultation,
                        CreatedAt = DateTime.Now
                    };
                    _context.PatientTransferLogs.Add(transferLog);
                    apptIndex++;
                    totalTransferred++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã chuyển giao thành công {totalTransferred} ca khám cho các bác sĩ tiếp nhận và báo bận.";
            return RedirectToAction("QuanLyNhanSu");
        }

        [HttpGet]
        public async Task<IActionResult> GetTransferLogById(int id)
        {
            var log = await _context.PatientTransferLogs
                .Include(l => l.FromDoctor)
                .Include(l => l.ToDoctor)
                .Include(l => l.Appointment)
                .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (log == null) return NotFound();

            return Json(new {
                patientName = log.Appointment?.Patient?.FullName ?? "N/A",
                fromDoctor = log.FromDoctor?.FullName ?? "N/A",
                toDoctor = log.ToDoctor?.FullName ?? "N/A",
                situation = log.Situation,
                background = log.Background,
                assessment = log.Assessment,
                recommendation = log.Recommendation,
                isEmergencyConsultation = log.IsEmergencyConsultation,
                createdAt = log.CreatedAt.ToString("HH:mm dd/MM/yyyy")
            });
        }

        // ==========================================
        // SAO LƯU & NHẬT KÝ
        // ==========================================
        public IActionResult SaoLuuDuLieu() { return View(); }
        public IActionResult NhatKyHeThong() { return View(); }
    }
}