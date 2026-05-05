using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;
using System.Linq;
using System.Threading.Tasks;

namespace HeThongBenhVien.Controllers
{
    public class CreateAppointmentModel
    {
        public string? PatientName { get; set; }
        public string? Gender { get; set; }
        public int Age { get; set; }
        public string? Reason { get; set; }
        public System.DateTime AppointmentTime { get; set; }
        public int Status { get; set; }
    }

    [Authorize(Roles = "Doctor,Admin")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var today = System.DateTime.Today;

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.AppointmentTime.Date >= today)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var viewModel = new DoctorDashboardViewModel
            {
                TodayPatientsCount = appointments.Count(a => a.AppointmentTime.Date == today),
                CompletedPatientsCount = appointments.Count(a => (a.Status == 4 || a.Status == 5) && a.AppointmentTime.Date == today),
                WaitingResultsCount = appointments.Count(a => a.Status == 3 && a.AppointmentTime.Date == today),
                EmergencyCount = appointments.Count(a => (a.Reason.Contains("cấp cứu", System.StringComparison.OrdinalIgnoreCase) || a.Reason.Contains("thắt ngực", System.StringComparison.OrdinalIgnoreCase)) && a.AppointmentTime.Date == today),
                UpcomingAppointments = appointments.Where(a => a.Status != 4 && a.Status != 5).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment(CreateAppointmentModel model)
        {
            if (model != null && !string.IsNullOrEmpty(model.PatientName))
            {
                // Lưu thông tin bệnh nhân mới
                var newPatient = new Patient
                {
                    FullName = model.PatientName,
                    Gender = string.IsNullOrEmpty(model.Gender) ? "Chưa xác định" : model.Gender,
                    Age = model.Age,
                    PatientCode = "BN" + new System.Random().Next(10000, 99999).ToString()
                };
                _context.Patients.Add(newPatient);
                await _context.SaveChangesAsync();

                // Tạo lịch khám mới cho bệnh nhân này
                var newAppointment = new Appointment
                {
                    PatientId = newPatient.Id,
                    Reason = string.IsNullOrEmpty(model.Reason) ? "Khám bệnh" : model.Reason,
                    AppointmentTime = model.AppointmentTime,
                    Status = model.Status
                };
                _context.Appointments.Add(newAppointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> DanhSach(string searchString)
        {
            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.FullName.Contains(searchString) 
                                      || p.PatientCode.Contains(searchString));
            }

            var patients = await query.ToListAsync();
            
            ViewData["SearchString"] = searchString;
            return View(patients);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPatient(Patient model)
        {
            if (ModelState.IsValid)
            {
                // Check if PatientCode already exists
                if (await _context.Patients.AnyAsync(p => p.PatientCode == model.PatientCode))
                {
                    ModelState.AddModelError("PatientCode", "Mã bệnh nhân này đã tồn tại.");
                    // In a real app we'd probably re-render the modal or return a view with error
                    return RedirectToAction(nameof(DanhSach)); 
                }

                _context.Patients.Add(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DanhSach));
        }

        [HttpGet]
        public async Task<IActionResult> EditPatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(int id, Patient model)
        {
            if (id != model.Id) return NotFound();
            
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DanhSach));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> KhamBenh(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var record = new MedicalRecord { AppointmentId = appointment.Id, Appointment = appointment };
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhamBenh(MedicalRecord model)
        {
            model.Id = 0; // Reset Id to prevent model binder from binding the route 'id' to MedicalRecord.Id
            
            if (ModelState.IsValid)
            {
                _context.MedicalRecords.Add(model);
                
                // Cập nhật trạng thái lịch khám thành Đã khám xong (4)
                var appointment = await _context.Appointments.FindAsync(model.AppointmentId);
                if (appointment != null)
                {
                    appointment.Status = 4;
                    _context.Appointments.Update(appointment);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(HoSoBenhAn));
            }

            // Nếu lỗi, nạp lại thông tin bệnh nhân
            model.Appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == model.AppointmentId);
                
            return View(model);
        }

        public async Task<IActionResult> ChiTietBenhAn(int id)
        {
            var record = await _context.MedicalRecords
                .Include(r => r.Appointment)
                .ThenInclude(a => a.Patient)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null) return NotFound();

            return View(record);
        }

        public async Task<IActionResult> HoSoBenhAn(string searchString)
        {
            var query = _context.MedicalRecords
                .Include(m => m.Appointment)
                .ThenInclude(a => a.Patient)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(m => m.Appointment.Patient.FullName.Contains(searchString) 
                                      || m.Appointment.Patient.PatientCode.Contains(searchString)
                                      || m.Diagnosis.Contains(searchString));
            }

            var records = await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
            
            ViewData["SearchString"] = searchString;
            return View(records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanhDieuTri(int id)
        {
            var record = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record != null && record.Appointment != null)
            {
                record.Appointment.Status = 5; // 5 = Hoàn thành điều trị
                _context.Appointments.Update(record.Appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(HoSoBenhAn));
        }
        public IActionResult LichHen() { return View(); }
        public IActionResult KeDonThuoc() { return View(); }
        public IActionResult ChiDinhXetNghiem() { return View(); }
        public IActionResult KetQuaCanLamSang() { return View(); }
        public IActionResult SinhHieu() { return View(); }
        public IActionResult LichSuKham() { return View(); }
        public IActionResult LichMo() { return View(); }
        public IActionResult ThongKe() { return View(); }
        public IActionResult CanhBaoTinhTrang() { return View(); }
        public IActionResult HenTaiKham() { return View(); }
        public IActionResult QuanLyGiuong() { return View(); }
        public IActionResult HoiChanOnline() { return View(); }
        public IActionResult ThongBao() { return View(); }
        public IActionResult CaiDat() { return View(); }
    }
}
