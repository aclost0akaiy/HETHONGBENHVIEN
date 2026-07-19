using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;

namespace HeThongBenhVien.Controllers
{
    /// <summary>
    /// Nhà thuốc: Chỉ có quyền xem BN chờ thuốc và xác nhận phát thuốc
    /// </summary>
    [Authorize(Roles = "Pharmacy,Admin")]
    public class PharmacyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PharmacyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Danh sách BN chờ lấy thuốc (Status = 7)
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.Status == AppointmentStatus.ChoLayThuoc)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var apptIds = appointments.Select(a => a.Id).ToList();

            var medRecords = await _context.MedicalRecords
                .Where(mr => apptIds.Contains(mr.AppointmentId))
                .ToListAsync();

            var medRecordIds = medRecords.Select(mr => mr.Id).ToList();

            var prescriptions = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .Where(p => medRecordIds.Contains(p.MedicalRecordId))
                .ToListAsync();

            ViewBag.MedicalRecords = medRecords.ToDictionary(mr => mr.AppointmentId, mr => mr);
            ViewBag.Prescriptions = prescriptions.ToDictionary(
                p => p.MedicalRecordId,
                p => p
            );

            return View(appointments);
        }

        // POST: Phát thuốc -> Status = 10 (Hoàn thành) hoặc 8 (Hẹn tái khám)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanPhatThuoc(int appointmentId, bool hasFollowUp)
        {
            var appt = await _context.Appointments.FindAsync(appointmentId);
            if (appt != null && appt.Status == AppointmentStatus.ChoLayThuoc)
            {
                appt.Status = hasFollowUp
                    ? AppointmentStatus.HenTaiKham
                    : AppointmentStatus.HoanThanh;

                await _context.SaveChangesAsync();

                TempData["PharmacySuccess"] = hasFollowUp
                    ? "Đã phát thuốc. Bệnh nhân có lịch tái khám."
                    : "Đã phát thuốc. Bệnh nhân hoàn thành điều trị, ra về.";
            }
            else
            {
                TempData["PharmacyError"] = "Không thể xử lý. Trạng thái không hợp lệ.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
