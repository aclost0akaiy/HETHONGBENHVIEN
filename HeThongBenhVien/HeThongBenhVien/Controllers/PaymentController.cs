using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;

namespace HeThongBenhVien.Controllers
{
    /// <summary>
    /// Bộ phận thanh toán: Chỉ có quyền xem BN chờ TT và xác nhận thu tiền
    /// </summary>
    [Authorize(Roles = "Payment,Admin")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Danh sách BN chờ thanh toán (Status = 6)
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.Status == AppointmentStatus.ChoThanhToan)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var apptIds = appointments.Select(a => a.Id).ToList();

            // Lấy thông tin đơn thuốc để hiển thị tổng tiền
            var medRecords = await _context.MedicalRecords
                .Where(mr => apptIds.Contains(mr.AppointmentId))
                .ToListAsync();

            var medRecordIds = medRecords.Select(mr => mr.Id).ToList();

            var prescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .Where(p => medRecordIds.Contains(p.MedicalRecordId))
                .ToListAsync();

            var labTests = await _context.LabTests
                .Where(lt => medRecordIds.Contains(lt.MedicalRecordId))
                .ToListAsync();

            ViewBag.MedicalRecords = medRecords;
            ViewBag.Prescriptions = prescriptions;
            ViewBag.LabTests = labTests;

            return View(appointments);
        }

        // POST: Thu ngân xác nhận đã thu tiền -> Status = 7 (Chờ lấy thuốc)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanThanhToan(int appointmentId)
        {
            var appt = await _context.Appointments.FindAsync(appointmentId);
            if (appt != null && appt.Status == AppointmentStatus.ChoThanhToan)
            {
                appt.Status = AppointmentStatus.ChoLayThuoc;
                await _context.SaveChangesAsync();
                TempData["PaySuccess"] = "Xác nhận thanh toán thành công. Bệnh nhân đã chuyển sang chờ lấy thuốc.";
            }
            else
            {
                TempData["PayError"] = "Không thể xử lý. Trạng thái bệnh án không hợp lệ.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
