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
            // Tổng lượt khám
            var totalAppointments = _context.Appointments.Count();

            // Doanh thu ngày: Sum of PrescriptionDetail (Price * Quantity)
            var dailyRevenue = _context.PrescriptionDetails.Sum(pd => pd.Price * pd.Quantity);

            // Giường trống: Patient count (represented as patient count, normalize to /500)
            var patientOccupancy = _context.Patients.Count();

            // Ca cấp cứu: Appointments with Status==6 or reason contains "cấp cứu" AND Status != 4,5
            var emergencyCases = _context.Appointments.Count(a => 
                a.Status == 6 || 
                (a.Reason != null && a.Reason.ToLower().Contains("cấp cứu") && a.Status != 4 && a.Status != 5));

            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.DailyRevenue = dailyRevenue;
            ViewBag.PatientOccupancy = patientOccupancy;
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
        // QUẢN LÝ KHOA PHÒNG
        // ==========================================
        public async Task<IActionResult> QuanLyKhoaPhong()
        {
            var list = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemKhoaPhong(Department dept)
        {
            if (!string.IsNullOrEmpty(dept.DepartmentName))
            {
                _context.Departments.Add(dept);
                await _context.SaveChangesAsync();
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

        // ==========================================
        // QUẢN LÝ KHO DƯỢC
        // ==========================================
        public async Task<IActionResult> QuanLyKhoDuoc()
        {
            // Load all medicines
            var allMedicines = await _context.Medicines.OrderBy(m => m.Name).ToListAsync();
            ViewBag.AllMedicines = allMedicines;
            ViewBag.TotalMedicines = allMedicines.Count;

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
        // ĐÁNH GIÁ CHẤT LƯỢNG
        // ==========================================
        public async Task<IActionResult> DanhGiaChatLuong()
        {
            var list = await _context.QualityReviews.OrderByDescending(q => q.CreatedAt).ToListAsync();
            return View(list);
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDanhGia(QualityReview review)
        {
            if (!string.IsNullOrEmpty(review.Department))
            {
                _context.QualityReviews.Add(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DanhGiaChatLuong));
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaDanhGia(int id)
        {
            var item = await _context.QualityReviews.FindAsync(id);
            if (item != null) { _context.QualityReviews.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(DanhGiaChatLuong));
        }

        // ==========================================
        // SAO LƯU & NHẬT KÝ
        // ==========================================
        public IActionResult SaoLuuDuLieu() { return View(); }
        public IActionResult NhatKyHeThong() { return View(); }
    }
}