using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;

namespace HeThongBenhVien.Controllers
{
    /// <summary>
    /// Phòng xét nghiệm: Chỉ có quyền xem BN chờ XN và cập nhật kết quả
    /// </summary>
    [Authorize(Roles = "Lab,Admin")]
    public class LabController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public LabController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Danh sách BN cần làm xét nghiệm (Status = 3 hoặc 4)
        public async Task<IActionResult> Index()
        {
            // Lấy cả BN đang chờ XN (3) và đang chờ KQ (4)
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.Status == AppointmentStatus.ChoXetNghiem
                         || a.Status == AppointmentStatus.ChoKetQua)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var apptIds = appointments.Select(a => a.Id).ToList();

            var medRecords = await _context.MedicalRecords
                .Where(mr => apptIds.Contains(mr.AppointmentId))
                .ToListAsync();

            var medRecordIds = medRecords.Select(mr => mr.Id).ToList();

            var labTests = await _context.LabTests
                .Where(lt => medRecordIds.Contains(lt.MedicalRecordId))
                .ToListAsync();

            ViewBag.MedicalRecords = medRecords;
            ViewBag.LabTests = labTests;

            return View(appointments);
        }

        // POST: Xác nhận bắt đầu xét nghiệm -> Status = 4 (Chờ kết quả)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatDauXetNghiem(int appointmentId)
        {
            var appt = await _context.Appointments.FindAsync(appointmentId);
            if (appt != null && appt.Status == AppointmentStatus.ChoXetNghiem)
            {
                appt.Status = AppointmentStatus.ChoKetQua;
                await _context.SaveChangesAsync();
                TempData["LabSuccess"] = "Đã tiếp nhận mẫu, bệnh nhân đang chờ kết quả.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Nhập kết quả xét nghiệm + tự động chuyển Status = 5 khi xong tất cả
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhapKetQua(int labTestId, string ketQua,
                                                     IFormFile? imageFile, int appointmentId)
        {
            var labTest = await _context.LabTests.FindAsync(labTestId);
            if (labTest != null)
            {
                labTest.Result = ketQua;
                labTest.Status = "Hoàn thành";
                labTest.CompletedAt = DateTime.Now;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"lab_{labTestId}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    var uploadDir = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadDir);
                    var filePath = Path.Combine(uploadDir, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await imageFile.CopyToAsync(stream);
                    labTest.ImageUrl = "/uploads/" + fileName;
                }

                await _context.SaveChangesAsync();
            }

            // Kiểm tra tất cả lab tests của appointment đã xong chưa
            var medRecord = await _context.MedicalRecords
                .FirstOrDefaultAsync(mr => mr.AppointmentId == appointmentId);

            if (medRecord != null)
            {
                var pendingTests = await _context.LabTests
                    .Where(lt => lt.MedicalRecordId == medRecord.Id
                              && lt.Status != "Hoàn thành"
                              && lt.Status != "Đã có kết quả")
                    .CountAsync();

                if (pendingTests == 0)
                {
                    // Tất cả XN xong -> chuyển "Chờ toa thuốc" (Status = 5)
                    var appt = await _context.Appointments.FindAsync(appointmentId);
                    if (appt != null &&
                        (appt.Status == AppointmentStatus.ChoKetQua ||
                         appt.Status == AppointmentStatus.ChoXetNghiem ||
                         appt.Status == AppointmentStatus.DangKham ||
                         appt.Status == AppointmentStatus.ChoKham))
                    {
                        appt.Status = AppointmentStatus.ChoToaThuoc;
                        await _context.SaveChangesAsync();
                        TempData["LabSuccess"] = "Đã cập nhật kết quả! Tất cả xét nghiệm hoàn thành. Bệnh nhân đã chuyển sang Chờ toa thuốc.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            TempData["LabSuccess"] = "Đã cập nhật kết quả xét nghiệm.";
            return RedirectToAction(nameof(Index));
        }
    }
}
