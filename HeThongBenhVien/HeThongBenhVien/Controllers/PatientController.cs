using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace HeThongBenhVien.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        // Mock PatientId for demo purposes (giả lập Bệnh nhân ID 1 đang đăng nhập)
        private readonly int currentPatientId = 1;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<int> GetCurrentPatientId()
        {
            var username = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null && !string.IsNullOrEmpty(user.PatientCode))
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientCode == user.PatientCode);
                    if (patient != null) return patient.Id;
                }
            }
            return currentPatientId;
        }

        public async Task<IActionResult> Portal()
        {
            int pid = await GetCurrentPatientId();
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == pid);
            ViewBag.PatientName = patient?.FullName ?? "Bệnh nhân Mock";
            ViewBag.UnreadNotificationCount = await _context.Notifications
                .CountAsync(n => n.PatientId == pid && !n.IsRead);
            return View();
        }

        private async Task<List<PatientBookingStatusItem>> GetPatientBookingStatuses(int patientId)
        {
            var bookings = await _context.Appointments
                .Where(a => a.PatientId == patientId && (a.Status == 0 || a.Status == 9))
                .OrderByDescending(a => a.AppointmentTime)
                .ToListAsync();

            var notifications = await _context.Notifications
                .Include(n => n.Doctor)
                .Where(n => n.PatientId == patientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return bookings.Select(a =>
            {
                Notification? matched = null;
                if (a.Status == 9)
                {
                    var datePart = a.AppointmentTime.ToString("dd/MM/yyyy");
                    var timePart = a.AppointmentTime.ToString("HH:mm");
                    matched = notifications.FirstOrDefault(n =>
                        n.Message.Contains(datePart) || n.Message.Contains(timePart));
                }

                return new PatientBookingStatusItem
                {
                    Appointment = a,
                    DoctorSms = matched?.Message,
                    DoctorName = matched?.Doctor?.FullName
                };
            }).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Booking()
        {
            int pid = await GetCurrentPatientId();
            ViewBag.MyBookings = await GetPatientBookingStatuses(pid);
            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctorsByDepartment(int departmentId, DateTime appointmentDate)
        {
            var doctors = await _context.Users
                .Where(u => u.Role == "Doctor" && _context.DoctorDepartments.Any(dd => dd.DoctorId == u.Id && dd.DepartmentId == departmentId))
                .Select(u => new { id = u.Id, fullName = u.FullName })
                .ToListAsync();
            return Json(doctors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Booking(string FullName, string PhoneNumber, string Reason, DateTime AppointmentTime, int? DoctorId)
        {
            Patient? patient = null;

            // 1. Ưu tiên: Nếu bệnh nhân đang đăng nhập, lấy đúng Patient record của họ
            var loggedInUsername = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(loggedInUsername))
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == loggedInUsername);
                if (currentUser != null && !string.IsNullOrEmpty(currentUser.PatientCode))
                {
                    patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientCode == currentUser.PatientCode);
                }
            }

            // 2. Nếu chưa tìm được, tìm theo số điện thoại (lưu ở CCCD field)
            if (patient == null)
            {
                patient = await _context.Patients.FirstOrDefaultAsync(p => p.CCCD == PhoneNumber);
            }

            // 3. Nếu không có, tạo bệnh nhân mới
            if (patient == null)
            {
                patient = new Patient
                {
                    FullName = FullName,
                    CCCD = PhoneNumber, // Lưu tạm SĐT vào CCCD
                    Age = 30, // Default
                    Gender = "Khác",
                    PatientCode = "BN" + DateTime.Now.ToString("yyMMddHHmmss")
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            var model = new Appointment
            {
                PatientId = patient.Id,
                Reason = Reason,
                AppointmentTime = AppointmentTime,
                DoctorId = DoctorId, // Lưu bác sĩ đã chọn
                Status = 0 // Chưa đến
            };
            
            _context.Appointments.Add(model);
            await _context.SaveChangesAsync();

            // Tính E-Ticket (số thứ tự điện tử trong ngày)
            var countToday = await _context.Appointments
                .Where(a => a.AppointmentTime.Date == model.AppointmentTime.Date)
                .CountAsync();
                
            TempData["PendingMessage"] = $"Yêu cầu đặt lịch đã được gửi! Số thứ tự điện tử (E-ticket): {countToday:D3}. Vui lòng chờ bác sĩ xác nhận.";
            return RedirectToAction(nameof(Booking));
        }

        public async Task<IActionResult> EHR()
        {
            int pid = await GetCurrentPatientId();
            // Lấy danh sách hồ sơ bệnh án của bệnh nhân
            var records = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Department)
                .Where(m => m.Appointment != null && m.Appointment.PatientId == pid)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.Prescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .Where(p => p.MedicalRecord != null && p.MedicalRecord.Appointment.PatientId == pid)
                .ToListAsync();
                
            ViewBag.LabTests = await _context.LabTests
                .Where(l => l.MedicalRecord != null && l.MedicalRecord.Appointment.PatientId == pid)
                .ToListAsync();

            return View(records);
        }

        public async Task<IActionResult> Payment()
        {
            int pid = await GetCurrentPatientId();
            // Lấy các đơn thuốc chưa thanh toán (Status = "Đã kê đơn")
            var unpaidPrescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m.Appointment)
                .Where(p => p.MedicalRecord != null && p.MedicalRecord.Appointment.PatientId == pid && p.Status == "Đã kê đơn")
                .ToListAsync();

            return View(unpaidPrescriptions);
        }
        
        [HttpPost]
        public async Task<IActionResult> PayPrescription(int prescriptionId)
        {
            var p = await _context.Prescriptions.FindAsync(prescriptionId);
            if (p != null)
            {
                p.Status = "Đã thanh toán";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thanh toán viện phí thành công!";
            }
            return RedirectToAction(nameof(Payment));
        }

        public async Task<IActionResult> Notifications()
        {
            int pid = await GetCurrentPatientId();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.PatientId == pid && !n.IsRead)
                .ToListAsync();
            foreach (var note in unreadNotifications)
            {
                note.IsRead = true;
            }
            if (unreadNotifications.Any())
            {
                await _context.SaveChangesAsync();
            }

            // 1. Nhắc lịch tái khám (Lấy các Appointment sắp tới của bệnh nhân)
            var upcomingAppointments = await _context.Appointments
                .Where(a => a.PatientId == pid && a.AppointmentTime > DateTime.Now)
                .OrderBy(a => a.AppointmentTime)
                .Take(5)
                .ToListAsync();

            // 2. Lấy đơn thuốc gần nhất để nhắc uống thuốc
            var recentPrescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .Where(p => p.MedicalRecord != null && p.MedicalRecord.Appointment.PatientId == pid)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            var notifications = await _context.Notifications
                .Include(n => n.Doctor)
                .Where(n => n.PatientId == pid)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.UpcomingAppointments = upcomingAppointments;
            ViewBag.RecentPrescription = recentPrescription;
            ViewBag.Notifications = notifications;

            return View();
        }

        [HttpGet]
        public IActionResult Rating()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rating(QualityReview model)
        {
            if (ModelState.IsValid)
            {
                int pid = await GetCurrentPatientId();
                var patient = await _context.Patients.FindAsync(pid);
                model.ReviewerName = patient?.FullName ?? "Bệnh nhân ẩn danh";
                model.Status = "Chờ xử lý";
                
                _context.QualityReviews.Add(model);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá dịch vụ. Chúng tôi sẽ ghi nhận và cải thiện tốt hơn!";
                return RedirectToAction(nameof(Rating));
            }
            return View(model);
        }

        public IActionResult HospitalMap()
        {
            return View();
        }
    }
}
