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

        public async Task<IActionResult> Portal()
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == currentPatientId);
            ViewBag.PatientName = patient?.FullName ?? "Bệnh nhân Mock";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Booking()
        {
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Booking(string FullName, string PhoneNumber, string Reason, int? DepartmentId, DateTime AppointmentTime)
        {
            // Find patient by Phone (stored in CCCD for now, or just create new)
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.CCCD == PhoneNumber);
            if (patient == null)
            {
                patient = new Patient
                {
                    FullName = FullName,
                    CCCD = PhoneNumber, // temporary use CCCD to store Phone
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
                Status = 0 // Chưa đến
            };
            
            _context.Appointments.Add(model);
            await _context.SaveChangesAsync();

            // Tính E-Ticket (số thứ tự điện tử trong ngày)
            var countToday = await _context.Appointments
                .Where(a => a.AppointmentTime.Date == model.AppointmentTime.Date)
                .CountAsync();
                
            TempData["SuccessMessage"] = $"Đặt lịch thành công! Số thứ tự điện tử (E-ticket) của bạn là: {countToday:D3}";
            return RedirectToAction(nameof(Booking));
        }

        public async Task<IActionResult> EHR()
        {
            // Lấy danh sách hồ sơ bệnh án của bệnh nhân
            var records = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Department)
                .Where(m => m.Appointment != null && m.Appointment.PatientId == currentPatientId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.Prescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .Where(p => p.MedicalRecord != null && p.MedicalRecord.Appointment.PatientId == currentPatientId)
                .ToListAsync();
                
            ViewBag.LabTests = await _context.LabTests
                .Where(l => l.MedicalRecord != null && l.MedicalRecord.Appointment.PatientId == currentPatientId)
                .ToListAsync();

            return View(records);
        }

        public async Task<IActionResult> Payment()
        {
            // Lấy các đơn thuốc chưa thanh toán (Status = "Đã kê đơn")
            var unpaidPrescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(m => m.Appointment)
                .Where(p => p.MedicalRecord != null && p.MedicalRecord.Appointment.PatientId == currentPatientId && p.Status == "Đã kê đơn")
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
            // 1. Nhắc lịch tái khám (Lấy các Appointment sắp tới của bệnh nhân)
            var upcomingAppointments = await _context.Appointments
                .Where(a => a.PatientId == currentPatientId && a.AppointmentTime > DateTime.Now)
                .OrderBy(a => a.AppointmentTime)
                .Take(5)
                .ToListAsync();

            // 2. Lấy đơn thuốc gần nhất để nhắc uống thuốc
            var recentPrescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .Where(p => p.MedicalRecord != null && p.MedicalRecord.Appointment.PatientId == currentPatientId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            var notifications = await _context.Notifications
                .Include(n => n.Doctor)
                .Where(n => n.PatientId == currentPatientId)
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
                var patient = await _context.Patients.FindAsync(currentPatientId);
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
