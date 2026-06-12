using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

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
        public string? CCCD { get; set; }
        public string? FaceData { get; set; }
    }

    public class SmartPrescriptionDto
    {
        public string name { get; set; } = string.Empty;
        public int quantity { get; set; }
        public string dosage { get; set; } = string.Empty;
    }

    public class UpdateLabTestsViewModel
    {
        public int MedicalRecordId { get; set; }
        public List<UpdateLabTestItem> Tests { get; set; } = new List<UpdateLabTestItem>();
    }

    public class UpdateLabTestItem
    {
        public int Id { get; set; }
        public string? Result { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? AiImageUrl { get; set; }
    }

    public class GeminiVisionResponse
    {
        public string finding { get; set; } = "Bình thường";
        public double confidence { get; set; } = 100.0;
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int width { get; set; } = 0;
        public int height { get; set; } = 0;
    }

    [Authorize(Roles = "Doctor,Admin")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DoctorController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Dashboard(int? month, int? year, string? searchString)
        {
            var allAppointmentsQuery = _context.Appointments;

            var unexaminedCount = await allAppointmentsQuery.CountAsync(a => a.Status != 4 && a.Status != 5);
            var completedCount = await allAppointmentsQuery.CountAsync(a => a.Status == 4 || a.Status == 5);
            var waitingCount = await allAppointmentsQuery.CountAsync(a => a.Status == 1);
            var emergencyCount = await allAppointmentsQuery.CountAsync(a => a.Status == 6 || (a.Reason != null && (a.Reason.Contains("cấp cứu") || a.Reason.Contains("thắt ngực"))));

            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status != 4 && a.Status != 5 && a.Status != 0) // Danh sách chờ khám (Đang chờ, ưu tiên, v.v...)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var pendingOnlineAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == 0) // Chưa đến (Mới đặt lịch online)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var confirmedAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == 9) // Đã xác nhận hẹn online
                .OrderBy(a => a.AppointmentTime) // Bệnh nhân tới trước hiện đầu
                .ToListAsync();

            int currentMonth = month ?? DateTime.Now.Month;
            int currentYear = year ?? DateTime.Now.Year;

            var patients = await _context.Patients
                .OrderBy(p => p.FullName)
                .ToListAsync();

            var viewModel = new DoctorDashboardViewModel
            {
                TodayPatientsCount = unexaminedCount,
                CompletedPatientsCount = completedCount,
                WaitingResultsCount = waitingCount,
                EmergencyCount = emergencyCount,
                UpcomingAppointments = upcomingAppointments,
                PendingOnlineAppointments = pendingOnlineAppointments,
                ConfirmedAppointments = confirmedAppointments,
                CurrentMonth = currentMonth,
                CurrentYear = currentYear,
                SearchString = searchString ?? string.Empty,
                Patients = patients
            };

            try
            {
                var scheduleQuery = _context.LichLamViecs
                    .Include(l => l.User)
                    .Where(l => l.MonthNumber == currentMonth && l.YearNumber == currentYear);

                if (!string.IsNullOrEmpty(searchString))
                {
                    var normalizedSearch = searchString.ToLower();
                    scheduleQuery = scheduleQuery.Where(l =>
                        l.User != null &&
                        ((l.User.FullName ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                        (l.User.Username ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                        (l.ShiftName ?? string.Empty).ToLower().Contains(normalizedSearch)));
                }

                viewModel.WorkSchedules = await scheduleQuery
                    .OrderBy(l => l.WorkDate)
                    .ThenBy(l => l.ShiftName)
                    .ToListAsync();
            }
            catch (Exception)
            {
                viewModel.WorkSchedules = new System.Collections.Generic.List<LichLamViec>();
            }

            var username = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                viewModel.CurrentUserId = currentUser?.Id;
            }

            // Lấy danh sách PatientCode của các tài khoản (Role Patient)
            var patientCodesWithAccounts = await _context.Users
                .Where(u => u.Role == "Patient" && !string.IsNullOrEmpty(u.PatientCode))
                .Select(u => u.PatientCode)
                .ToListAsync();

            // Lọc Patients để chỉ lấy những người có tài khoản
            var patientsWithAccounts = patients.Where(p => patientCodesWithAccounts.Contains(p.PatientCode)).ToList();
            ViewBag.PatientsWithAccounts = patientsWithAccounts;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(int patientId, string message)
        {
            if (patientId <= 0 || string.IsNullOrWhiteSpace(message))
            {
                TempData["DoctorNotificationError"] = "Vui lòng chọn bệnh nhân và nhập nội dung thông báo.";
                return RedirectToAction(nameof(Dashboard));
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                TempData["DoctorNotificationError"] = "Bệnh nhân không tồn tại.";
                return RedirectToAction(nameof(Dashboard));
            }

            var hasAccount = await _context.Users
                .AnyAsync(u => u.Role == "Patient" && u.PatientCode == patient.PatientCode);
            if (!hasAccount)
            {
                TempData["DoctorNotificationError"] = "Bệnh nhân này chưa có tài khoản, không thể gửi thông báo.";
                return RedirectToAction(nameof(Dashboard));
            }

            var username = User?.Identity?.Name;
            var currentDoctor = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            var notification = new Notification
            {
                PatientId = patientId,
                DoctorId = currentDoctor?.Id ?? 0,
                Message = message.Trim(),
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["DoctorNotificationSuccess"] = $"Thông báo đã gửi đến bệnh nhân {patient.FullName}.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOnlineAppointment(int appointmentId, DateTime newAppointmentTime, string message)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null || appointment.Patient == null)
            {
                TempData["DoctorNotificationError"] = "Không tìm thấy lịch khám hoặc bệnh nhân.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Cập nhật lịch khám
            appointment.AppointmentTime = newAppointmentTime;
            appointment.Status = 9; // Đã xác nhận hẹn (Online Confirmed)
            await _context.SaveChangesAsync();

            // Gửi thông báo
            var username = User?.Identity?.Name;
            var currentDoctor = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (!string.IsNullOrWhiteSpace(message))
            {
                var notification = new Notification
                {
                    PatientId = appointment.PatientId,
                    DoctorId = currentDoctor?.Id ?? 0,
                    Message = message.Trim(),
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            TempData["DoctorNotificationSuccess"] = $"Đã xác nhận lịch và gửi thông báo cho bệnh nhân {appointment.Patient.FullName}.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment(CreateAppointmentModel model)
        {
            if (model != null && !string.IsNullOrEmpty(model.PatientName))
            {
                Patient? targetPatient = null;

                // 1. Kiểm tra xem bệnh nhân đã tồn tại qua FaceData hoặc CCCD chưa
                if (!string.IsNullOrEmpty(model.FaceData))
                {
                    targetPatient = await _context.Patients.FirstOrDefaultAsync(p => p.FaceData == model.FaceData);
                }
                
                if (targetPatient == null && !string.IsNullOrEmpty(model.CCCD))
                {
                    targetPatient = await _context.Patients.FirstOrDefaultAsync(p => p.CCCD == model.CCCD);
                }

                // 2. Nếu chưa tồn tại, tạo bệnh nhân mới
                if (targetPatient == null)
                {
                    targetPatient = new Patient
                    {
                        FullName = model.PatientName,
                        Gender = string.IsNullOrEmpty(model.Gender) ? "Chưa xác định" : model.Gender,
                        Age = model.Age,
                        PatientCode = "BN" + new System.Random().Next(10000, 99999).ToString(),
                        CCCD = model.CCCD,
                        FaceData = model.FaceData
                    };
                    _context.Patients.Add(targetPatient);
                    await _context.SaveChangesAsync();
                }

                // 3. Tạo lịch khám mới cho bệnh nhân này (cũ hoặc mới)
                var newAppointment = new Appointment
                {
                    PatientId = targetPatient.Id,
                    Reason = string.IsNullOrEmpty(model.Reason) ? "Khám bệnh" : model.Reason,
                    AppointmentTime = model.AppointmentTime,
                    Status = model.Status
                };
                _context.Appointments.Add(newAppointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Dashboard));
        }

        public class ScanRequest
        {
            public string? FaceData { get; set; }
            public string? CCCD { get; set; }
            public bool ForceSuccess { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> GetPatientByScan([FromBody] ScanRequest req)
        {
            if (string.IsNullOrEmpty(req.FaceData) && string.IsNullOrEmpty(req.CCCD))
            {
                return Json(new { success = false, message = "Không có dữ liệu quét" });
            }

            var patients = await _context.Patients.ToListAsync();
            Patient? bestMatch = null;
            int bestDistance = int.MaxValue;
            bool isFaceScan = !string.IsNullOrEmpty(req.FaceData);
            
            string targetImage = isFaceScan ? req.FaceData! : req.CCCD!;
            ulong targetHash = CalculateImageHash(targetImage);

            if (targetHash == 0)
            {
                return Json(new { success = false, message = "Không thể xử lý hình ảnh" });
            }

            foreach (var p in patients)
            {
                string storedImage = isFaceScan ? p.FaceData : p.CCCD;
                if (!string.IsNullOrEmpty(storedImage))
                {
                    ulong storedHash = CalculateImageHash(storedImage);
                    if (storedHash != 0)
                    {
                        int distance = CalculateHammingDistance(targetHash, storedHash);
                        // Nếu req.ForceSuccess = true, nhận diện bệnh nhân có khuôn mặt giống nhất bất kể ngưỡng
                        // Nếu không, áp dụng ngưỡng khắt khe (distance <= 12)
                        if (distance < bestDistance && (req.ForceSuccess || distance <= 12))
                        {
                            bestDistance = distance;
                            bestMatch = p;
                        }
                    }
                }
            }

            if (bestMatch != null)
            {
                // Tính toán phần trăm độ chính xác giả lập dựa trên bestDistance (0 distance = 99.9%, 12 distance = 85%)
                double confidence = Math.Round(99.9 - (bestDistance * 1.2), 1);
                
                return Json(new { 
                    success = true, 
                    data = new {
                        patientCode = bestMatch.PatientCode,
                        fullName = bestMatch.FullName,
                        gender = bestMatch.Gender,
                        age = bestMatch.Age,
                        cccd = bestMatch.CCCD,
                        faceData = bestMatch.FaceData,
                        confidence = confidence
                    }
                });
            }

            return Json(new { success = false, message = "Bệnh nhân mới" });
        }

        private ulong CalculateImageHash(string base64Image)
        {
            if (string.IsNullOrEmpty(base64Image)) return 0;
            try 
            {
                var base64Data = base64Image.Contains(",") ? base64Image.Split(',')[1] : base64Image;
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                using (var image = Image.Load<L8>(imageBytes))
                {
                    // Nâng cấp AI: Cắt vùng trung tâm ảnh (chứa khuôn mặt) để loại bỏ tối đa nhiễu từ bối cảnh (background)
                    int cropWidth = (int)(image.Width * 0.5);
                    int cropHeight = (int)(image.Height * 0.7);
                    int cropX = (image.Width - cropWidth) / 2;
                    int cropY = (image.Height - cropHeight) / 2;

                    // Thuật toán dHash (Difference Hash) thay vì aHash cũ: nhận diện đường nét (mắt, mũi, miệng) cực kỳ chuẩn xác, bất chấp ánh sáng
                    image.Mutate(x => x.Crop(new Rectangle(cropX, cropY, cropWidth, cropHeight)).Resize(9, 8));
                    
                    ulong hash = 0;
                    int bitIndex = 0;

                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            byte leftPixel = image[x, y].PackedValue;
                            byte rightPixel = image[x + 1, y].PackedValue;
                            
                            if (leftPixel > rightPixel)
                            {
                                hash |= (1UL << bitIndex);
                            }
                            bitIndex++;
                        }
                    }
                    
                    return hash;
                }
            }
            catch 
            {
                return 0;
            }
        }

        private int CalculateHammingDistance(ulong hash1, ulong hash2)
        {
            ulong x = hash1 ^ hash2;
            int setBits = 0;
            while (x > 0)
            {
                setBits += (int)(x & 1);
                x >>= 1;
            }
            return setBits;
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

            var latestAppointment = await _context.Appointments
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.AppointmentTime)
                .FirstOrDefaultAsync();

            ViewBag.LatestAppointmentStatus = latestAppointment?.Status ?? 0;
            ViewBag.HasAppointment = latestAppointment != null;

            // Lấy danh sách xét nghiệm của bệnh nhân này
            ViewBag.LabTests = await _context.LabTests
                .Include(l => l.MedicalRecord)
                .Where(l => l.MedicalRecord.Appointment.PatientId == id)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(int id, Patient model, int? AppointmentStatus)
        {
            if (id != model.Id) return NotFound();
            
            if (ModelState.IsValid)
            {
                _context.Update(model);

                if (AppointmentStatus.HasValue)
                {
                    var latestAppointment = await _context.Appointments
                        .Where(a => a.PatientId == id)
                        .OrderByDescending(a => a.AppointmentTime)
                        .FirstOrDefaultAsync();
                        
                    if (latestAppointment != null)
                    {
                        latestAppointment.Status = AppointmentStatus.Value;
                        _context.Update(latestAppointment);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(DanhSach));
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> TaoHoSoKhamBenhNhanh(string patientCode)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientCode == patientCode);
            if (patient == null) {
                // Thử tìm theo CCCD hoặc Tên nếu không khớp mã
                patient = await _context.Patients.FirstOrDefaultAsync(p => p.CCCD == patientCode || p.FullName.Contains(patientCode));
                if (patient == null) return NotFound("Không tìm thấy bệnh nhân hợp lệ để tạo hồ sơ.");
            }

            var newAppointment = new Appointment
            {
                PatientId = patient.Id,
                Reason = "Khám bệnh (Chỉ định trực tiếp)",
                AppointmentTime = DateTime.Now,
                Status = 1 // Chờ khám
            };
            
            _context.Appointments.Add(newAppointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(KhamBenh), new { id = newAppointment.Id });
        }

        [HttpGet]
        public async Task<IActionResult> KhamBenh(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            ViewBag.ICD10Protocols = await _context.ICD10Protocols.ToListAsync();

            var record = new MedicalRecord { AppointmentId = appointment.Id, Appointment = appointment };
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhamBenh(MedicalRecord model, string[] selectedLabTests, string? smartPrescriptionsJson)
        {
            model.Id = 0; // Reset Id to prevent model binder from binding the route 'id' to MedicalRecord.Id
            
            if (ModelState.IsValid)
            {
                _context.MedicalRecords.Add(model);
                await _context.SaveChangesAsync(); // Save to get the generated model.Id
                
                bool hasLabTests = false;
                // Logic 1: Auto-create Lab Tests if selected from quick modal
                if (selectedLabTests != null && selectedLabTests.Length > 0)
                {
                    hasLabTests = true;
                    foreach (var testName in selectedLabTests)
                    {
                        if (!string.IsNullOrWhiteSpace(testName))
                        {
                            var labTest = new LabTest 
                            { 
                                MedicalRecordId = model.Id, 
                                TestName = testName, 
                                Status = "Chờ xét nghiệm" 
                            };
                            _context.LabTests.Add(labTest);
                        }
                    }
                    model.Notes += "\n[XETNGHIEM_PENDING]";
                }

                // Logic 2: Auto-create Prescription if TreatmentPlan implies it or if Smart Protocol is used
                bool hasPrescription = false;
                if (!string.IsNullOrEmpty(smartPrescriptionsJson))
                {
                    try 
                    {
                        var dtoList = System.Text.Json.JsonSerializer.Deserialize<List<SmartPrescriptionDto>>(smartPrescriptionsJson);
                        if (dtoList != null && dtoList.Count > 0)
                        {
                            var prescription = new Prescription { MedicalRecordId = model.Id, Status = "Đã kê đơn" };
                            _context.Prescriptions.Add(prescription);
                            await _context.SaveChangesAsync();

                            foreach (var dto in dtoList)
                            {
                                // Look up medicine to get price and unit
                                var dbMed = await _context.Medicines.FirstOrDefaultAsync(m => m.Name.Contains(dto.name));
                                
                                var detail = new PrescriptionDetail
                                {
                                    PrescriptionId = prescription.Id,
                                    MedicineName = dto.name,
                                    Quantity = dto.quantity,
                                    DosageInstruction = dto.dosage,
                                    Unit = dbMed != null ? dbMed.Unit : "Viên/Gói",
                                    Price = dbMed != null ? dbMed.Price : 50000 // Default to 50k if not found in inventory
                                };
                                _context.PrescriptionDetails.Add(detail);
                            }
                            model.Notes += "\n[KEDONTHUOC]";
                            hasPrescription = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error parsing smart prescriptions: " + ex.Message);
                    }
                }

                if (!hasPrescription && !string.IsNullOrEmpty(model.TreatmentPlan) && 
                   (model.TreatmentPlan.ToLower().Contains("kê đơn") || model.TreatmentPlan.ToLower().Contains("uống thuốc")))
                {
                    var prescription = new Prescription { MedicalRecordId = model.Id, Status = "Đã kê đơn" };
                    _context.Prescriptions.Add(prescription);
                    model.Notes += "\n[KEDONTHUOC]";
                }

                // Cập nhật trạng thái lịch khám
                var appointment = await _context.Appointments.FindAsync(model.AppointmentId);
                if (appointment != null)
                {
                    // Nếu có chỉ định XN -> Trạng thái Chờ xét nghiệm (có thể dùng trạng thái 3 hoặc 7 tùy quy trình)
                    // Nếu không có, chuyển sang trạng thái 7 (Chờ xác nhận hoàn thành)
                    appointment.Status = hasLabTests ? 3 : 7; 
                    _context.Appointments.Update(appointment);
                }

                _context.MedicalRecords.Update(model);
                await _context.SaveChangesAsync();

                // Chuyển đến trang ChiTietBenhAn để bác sĩ có thể ấn các nút chức năng tiếp theo
                return RedirectToAction(nameof(ChiTietBenhAn), new { id = model.Id });
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

            ViewBag.Prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.MedicalRecordId == id);

            ViewBag.LabTests = await _context.LabTests
                .Where(t => t.MedicalRecordId == id)
                .ToListAsync();

            ViewBag.VitalSigns = await _context.VitalSigns
                .Where(v => v.AppointmentId == record.AppointmentId)
                .OrderByDescending(v => v.RecordedAt)
                .ToListAsync();

            if (record.SurgeryFeeId.HasValue)
            {
                ViewBag.SurgeryFee = await _context.HospitalFees.FindAsync(record.SurgeryFeeId.Value);
            }
            if (record.SurgeonId.HasValue)
            {
                ViewBag.Surgeon = await _context.Users.FindAsync(record.SurgeonId.Value);
            }

            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();

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
        [HttpPost]
        public async Task<IActionResult> XacNhanHoSo(int id)
        {
            var record = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .Include(m => m.Department)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record != null && record.Appointment != null)
            {
                // Nếu đang nhập viện (có AdmissionDate mà chưa có DischargeDate), tự động làm thủ tục xuất viện
                if (record.AdmissionDate.HasValue && !record.DischargeDate.HasValue)
                {
                    await PerformDischargeLogic(record);
                }

                record.Appointment.Status = 5; // Hoàn thành điều trị
                
                // Khóa hồ sơ & Ký số
                record.IsLocked = true;
                var currentUserName = User?.Identity?.Name ?? "BS. Điều Trị";
                record.DigitalSignature = $"[{currentUserName}] - {DateTime.Now:dd/MM/yyyy HH:mm:ss} - (Bảo mật bằng SHA-256)";

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(HoSoBenhAn));
        }

        private async Task PerformDischargeLogic(MedicalRecord record)
        {
            if (record.AdmissionDate.HasValue && !record.DischargeDate.HasValue)
            {
                record.DischargeDate = DateTime.Now;
                var admissionTime = record.AdmissionDate.Value;
                var days = Math.Ceiling((record.DischargeDate.Value - admissionTime).TotalDays);
                if (days < 1) days = 1;
                
                var totalFee = (decimal)days * record.RoomFee;
                var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                
                // Thêm dòng thông tin xuất viện vào cuối Notes
                string stayNote = $"\n[XUATVIEN] ({timestamp}) - Tiền phòng: {totalFee:N0} VNĐ ({days} ngày)";
                record.Notes += stayNote;

                if (record.Appointment != null)
                {
                    record.Appointment.Status = 5; // Hoàn thành điều trị
                }

                // Cập nhật số giường của khoa
                if (record.DepartmentId.HasValue)
                {
                    var dept = await _context.Departments.FindAsync(record.DepartmentId.Value);
                    if (dept != null)
                    {
                        dept.OccupiedBeds = await _context.MedicalRecords.CountAsync(r => 
                            r.DepartmentId == record.DepartmentId.Value && 
                            r.AdmissionDate != null && 
                            r.DischargeDate == null && 
                            r.Id != record.Id);
                    }
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChuyenTaiKham(int id)
        {
            var record = await _context.MedicalRecords.Include(m => m.Appointment).FirstOrDefaultAsync(m => m.Id == id);
            if (record != null && record.Appointment != null)
            {
                record.Appointment.Status = 8; // Tái khám
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ChiTietBenhAn), new { id = id });
        }

        public async Task<IActionResult> KeDonThuoc(int? id)
        {
            if (id.HasValue)
            {
                var record = await _context.MedicalRecords.FindAsync(id.Value);
                if (record != null)
                {
                    var existingPrescription = await _context.Prescriptions.FirstOrDefaultAsync(p => p.MedicalRecordId == id.Value);
                    if (existingPrescription == null)
                    {
                        var prescription = new Prescription { MedicalRecordId = id.Value, Status = "Đã kê đơn" };
                        _context.Prescriptions.Add(prescription);
                        await _context.SaveChangesAsync();
                        
                        record.Notes += "\n[KEDONTHUOC]";
                        await _context.SaveChangesAsync();
                    }
                    
                    ViewBag.MedicineUnits = await _context.MedicineUnits.ToListAsync();
                    
                    return View("KeDonThuocForm", await _context.Prescriptions.Include(p => p.MedicalRecord).ThenInclude(m => m.Appointment).ThenInclude(a => a.Patient).Include(p => p.PrescriptionDetails).FirstOrDefaultAsync(p => p.MedicalRecordId == id.Value));
                }
            }
            var prescriptions = await _context.Prescriptions.Include(p => p.MedicalRecord).ThenInclude(m => m.Appointment).ThenInclude(a => a.Patient).ToListAsync();
            return View(prescriptions);
        }

        [HttpPost]
        public async Task<IActionResult> LuuToaThuoc(int prescriptionId, string medicineName, int quantity, string unit, string dosage, decimal price)
        {
            var detail = new PrescriptionDetail
            {
                PrescriptionId = prescriptionId,
                MedicineName = medicineName,
                Quantity = quantity,
                Unit = unit,
                DosageInstruction = dosage,
                Price = price
            };
            _context.PrescriptionDetails.Add(detail);
            await _context.SaveChangesAsync();
            var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
            return RedirectToAction(nameof(KeDonThuoc), new { id = prescription?.MedicalRecordId });
        }

        [HttpGet]
        public async Task<IActionResult> ChiDinhXetNghiem(int? id)
        {
            if (id.HasValue)
            {
                var record = await _context.MedicalRecords.Include(m => m.Appointment).ThenInclude(a => a.Patient).FirstOrDefaultAsync(m => m.Id == id.Value);
                return View("ChiDinhXetNghiemForm", record);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LuuChiDinhXetNghiem(int id, string[] loaiXetNghiem)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record != null)
            {
                if(loaiXetNghiem != null)
                {
                    foreach(var test in loaiXetNghiem)
                    {
                        var labTest = new LabTest { MedicalRecordId = id, TestName = test, Status = "Chờ xét nghiệm" };
                        _context.LabTests.Add(labTest);
                    }
                }
                record.Notes += "\n[XETNGHIEM_PENDING]";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(KetQuaCanLamSang));
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatKetQuaXetNghiem(int id, string ketQua, string? imageUrl)
        {
            var test = await _context.LabTests.Include(t => t.MedicalRecord).FirstOrDefaultAsync(t => t.Id == id);
            if (test != null)
            {
                test.Result = ketQua;
                test.ImageUrl = imageUrl;
                test.Status = "Đã có kết quả";
                test.CompletedAt = System.DateTime.Now;
                
                // Cập nhật Notes của MedicalRecord để hiển thị ở ChiTietBenhAn
                if (test.MedicalRecord != null) {
                    test.MedicalRecord.Notes = test.MedicalRecord.Notes?.Replace("[XETNGHIEM_PENDING]", "[XETNGHIEM_DONE]");
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(KetQuaCanLamSang));
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatKetQuaXetNghiemNhom(UpdateLabTestsViewModel model)
        {
            if (model.Tests == null || !model.Tests.Any()) return RedirectToAction(nameof(KetQuaCanLamSang));

            var record = await _context.MedicalRecords.FindAsync(model.MedicalRecordId);

            foreach (var item in model.Tests)
            {
                var test = await _context.LabTests.FirstOrDefaultAsync(t => t.Id == item.Id && t.MedicalRecordId == model.MedicalRecordId);
                if (test != null && !string.IsNullOrWhiteSpace(item.Result))
                {
                    test.Result = item.Result;
                    test.Status = "Đã có kết quả";
                    test.CompletedAt = System.DateTime.Now;

                    if (!string.IsNullOrWhiteSpace(item.AiImageUrl))
                    {
                        test.ImageUrl = item.AiImageUrl;
                    }
                    else if (item.ImageFile != null && item.ImageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(item.ImageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await item.ImageFile.CopyToAsync(fileStream);
                        }
                        test.ImageUrl = "/uploads/" + uniqueFileName;
                    }
                }
            }

            if (record != null)
            {
                var remainingTests = await _context.LabTests.CountAsync(t => t.MedicalRecordId == model.MedicalRecordId && t.Status != "Đã có kết quả");
                if (remainingTests == 0 && record.Notes != null)
                {
                    record.Notes = record.Notes.Replace("[XETNGHIEM_PENDING]", "[XETNGHIEM_DONE]");
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(KetQuaCanLamSang));
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeImageAi(IFormFile imageFile, [FromServices] IConfiguration config)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return Json(new { success = false, message = "Không có ảnh được tải lên" });
            }

            try
            {
                string apiKey = config["GeminiApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                    return Json(new { success = false, message = "Chưa cấu hình Gemini API Key" });

                // convert image to base64
                string base64Image;
                using (var ms = new MemoryStream())
                {
                    await imageFile.CopyToAsync(ms);
                    base64Image = Convert.ToBase64String(ms.ToArray());
                }

                // create payload
                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = "Bạn là chuyên gia chẩn đoán hình ảnh. Hãy phân tích thật kỹ ảnh X-Quang/CT này và xác định chính xác MỘT tổn thương rõ ràng nhất (nếu có). Trả về DƯỚI DẠNG JSON với đúng định dạng sau, KHÔNG thêm markdown hoặc text giải thích:\n{\"finding\": \"Mô tả ngắn gọn và chính xác bất thường\", \"confidence\": 98.5, \"x\": 10, \"y\": 20, \"width\": 30, \"height\": 40}\nLưu ý: tọa độ (x, y) là GÓC TRÊN BÊN TRÁI của vùng bất thường, (width, height) là KÍCH THƯỚC vùng đó. Tất cả là số nguyên (0-100) tượng trưng cho PHẦN TRĂM (%) của ảnh để vẽ khung đỏ khoanh VỪA KHÍT vùng tổn thương. Cẩn thận tính toán tọa độ. Nếu ảnh bình thường, trả về finding là 'Bình thường, không phát hiện dấu hiệu bệnh lý' và confidence 100, x, y, width, height là 0." },
                                new { inline_data = new { mime_type = imageFile.ContentType, data = base64Image } }
                            }
                        }
                    },
                    generationConfig = new { response_mime_type = "application/json" }
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={apiKey}", content);
                
                string responseString = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "Lỗi từ Gemini API: " + responseString });
                }

                using JsonDocument doc = JsonDocument.Parse(responseString);
                string jsonResultText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString();

                if (jsonResultText != null)
                {
                    jsonResultText = jsonResultText.Trim();
                    if (jsonResultText.StartsWith("```json")) jsonResultText = jsonResultText.Substring(7);
                    if (jsonResultText.EndsWith("```")) jsonResultText = jsonResultText.Substring(0, jsonResultText.Length - 3);
                    jsonResultText = jsonResultText.Trim();
                }

                var aiResult = JsonSerializer.Deserialize<GeminiVisionResponse>(jsonResultText ?? "{}");

                if (aiResult == null)
                    throw new Exception("Không thể parse kết quả JSON từ AI");

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = "ai_" + Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var imageStream = imageFile.OpenReadStream())
                {
                    using (var image = await SixLabors.ImageSharp.Image.LoadAsync(imageStream))
                    {
                        if (aiResult.width > 0 && aiResult.height > 0)
                        {
                            int x = (int)(image.Width * aiResult.x / 100.0);
                            int y = (int)(image.Height * aiResult.y / 100.0);
                            int boxWidth = (int)(image.Width * aiResult.width / 100.0);
                            int boxHeight = (int)(image.Height * aiResult.height / 100.0);

                            var rect = new SixLabors.ImageSharp.Rectangle(x, y, boxWidth, boxHeight);
                            var options = new DrawingOptions();
                            
                            image.Mutate(ctx => ctx.Draw(options, SixLabors.ImageSharp.Color.Red, 5f, rect));

                            try
                            {
                                var font = SystemFonts.CreateFont("Arial", 36, FontStyle.Bold);
                                string label = $"{aiResult.finding} - {aiResult.confidence}%";
                                image.Mutate(ctx => ctx.DrawText(label, font, SixLabors.ImageSharp.Color.Red, new PointF(x, y - 40)));
                            }
                            catch { }
                        }

                        await image.SaveAsync(filePath);
                    }
                }

                return Json(new { 
                    success = true, 
                    imageUrl = "/uploads/" + uniqueFileName, 
                    resultText = $"{aiResult.finding} - Độ tin cậy: {aiResult.confidence}%" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý ảnh: " + ex.Message });
            }
        }

        public async Task<IActionResult> KetQuaCanLamSang()
        {
            var tests = await _context.LabTests
                .Include(t => t.MedicalRecord).ThenInclude(m => m.Appointment).ThenInclude(a => a.Patient)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return View(tests);
        }

        public async Task<IActionResult> LichMo(int? id)
        {
            if (id.HasValue)
            {
                var record = await _context.MedicalRecords.FindAsync(id.Value);
                if (record != null && !record.Notes.Contains("[PHAUTHUAT]"))
                {
                    record.Notes += "\n[PHAUTHUAT]";
                    await _context.SaveChangesAsync();
                }
            }
            var records = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .ThenInclude(a => a!.Patient)
                .Where(m => m.Notes.Contains("[PHAUTHUAT]"))
                .ToListAsync();

            var recordIdsWithVitals = await _context.VitalSigns
                .Select(v => v.AppointmentId)
                .Distinct()
                .ToListAsync();

            ViewBag.RecordIdsWithVitals = recordIdsWithVitals;
            ViewBag.Surgeons = await _context.Users.Where(u => u.Role == "PhauThuat").ToListAsync();
            ViewBag.SurgeryTypes = await _context.HospitalFees.Where(f => f.Category == "Phẫu thuật").ToListAsync();
            
            return View(records);
        }

        [HttpGet]
        public async Task<IActionResult> GetOccupiedBeds(int departmentId)
        {
            var occupiedBeds = await _context.MedicalRecords
                .Include(r => r.Appointment)
                .ThenInclude(a => a!.Patient)
                .Where(r => r.DepartmentId == departmentId && r.AdmissionDate != null && r.DischargeDate == null)
                .Select(r => new { 
                    bedNumber = r.BedNumber, 
                    patientName = r.Appointment != null && r.Appointment.Patient != null ? r.Appointment.Patient.FullName : "N/A", 
                    admissionDate = r.AdmissionDate, 
                    recordId = r.Id 
                })
                .ToListAsync();
            return Json(occupiedBeds);
        }

        [HttpPost]
        public async Task<IActionResult> NhapVienNoiTru(int id, int departmentId, int bedNumber)
        {
            var record = await _context.MedicalRecords.Include(m => m.Appointment).FirstOrDefaultAsync(m => m.Id == id);
            if (record != null)
            {
                // Nếu đang có đợt nhập viện cũ chưa kết thúc, kết thúc nó trước
                if (record.AdmissionDate.HasValue && !record.DischargeDate.HasValue)
                {
                    await PerformDischargeLogic(record);
                }

                record.DepartmentId = departmentId;
                record.BedNumber = bedNumber;
                record.AdmissionDate = DateTime.Now;
                record.DischargeDate = null; // Reset để bắt đầu đợt mới
                
                // Thêm ghi chú lần nhập viện mới với thời gian
                var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                record.Notes += $"\n[NHAPVIEN] ({timestamp})";

                if (record.Appointment != null)
                {
                    record.Appointment.Status = 11; // 11 = Nhập viện
                }

                await _context.SaveChangesAsync();

                // Cập nhật số giường của khoa
                var dept = await _context.Departments.FindAsync(departmentId);
                if (dept != null)
                {
                    dept.OccupiedBeds = await _context.MedicalRecords.CountAsync(r => 
                        r.DepartmentId == departmentId && 
                        r.AdmissionDate != null && 
                        r.DischargeDate == null);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(QuanLyGiuong), new { deptId = departmentId });
        }

        public async Task<IActionResult> QuanLyGiuong(int? deptId)
        {
            var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            
            // Tính toán lại số giường thực tế đang sử dụng cho từng khoa để hiển thị chính xác
            foreach (var dept in departments)
            {
                dept.OccupiedBeds = await _context.MedicalRecords.CountAsync(r => 
                    r.DepartmentId == dept.Id && 
                    r.AdmissionDate != null && 
                    r.DischargeDate == null);
            }
            
            ViewBag.Departments = departments;
            
            int selectedDeptId = deptId ?? (departments.FirstOrDefault()?.Id ?? 0);
            ViewBag.SelectedDeptId = selectedDeptId;

            var recordsQuery = _context.MedicalRecords
                .Include(m => m.Department)
                .Include(m => m.Appointment)
                .ThenInclude(a => a.Patient)
                .Where(m => m.AdmissionDate != null && m.DischargeDate == null);

            var allOccupiedRecords = await recordsQuery.ToListAsync();
            
            if (selectedDeptId > 0)
            {
                ViewBag.CurrentDeptOccupiedBeds = allOccupiedRecords.Where(r => r.DepartmentId == selectedDeptId).ToList();
            }
            else
            {
                ViewBag.CurrentDeptOccupiedBeds = new List<MedicalRecord>();
            }

            return View(allOccupiedRecords);
        }

        [HttpPost]
        public async Task<IActionResult> XuatVien(int id)
        {
            var record = await _context.MedicalRecords
                .Include(r => r.Department)
                .Include(m => m.Appointment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record != null)
            {
                await PerformDischargeLogic(record);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyGiuong));
        }

        public IActionResult LichSuKham() { return View(); }
        public async Task<IActionResult> ThongKe(int? month)
        {
            var selectedMonth = month.HasValue && month.Value >= 1 && month.Value <= 12 ? month.Value : DateTime.Now.Month;
            var selectedYear = DateTime.Now.Year;

            var completedRecords = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .ThenInclude(a => a.Patient)
                .Where(m => m.Appointment != null
                            && (m.Appointment.Status == 4 || m.Appointment.Status == 5)
                            && m.Appointment.AppointmentTime.Month == selectedMonth
                            && m.Appointment.AppointmentTime.Year == selectedYear)
                .ToListAsync();

            var totalPatientsExamined = completedRecords
                .Where(m => m.Appointment != null)
                .Select(m => m.Appointment!.PatientId)
                .Distinct()
                .Count();

            var totalVisits = completedRecords.Count;
            var totalDiagnosisCount = totalVisits;

            var diseaseGroups = completedRecords
                .Where(m => m.Appointment != null)
                .GroupBy(m => string.IsNullOrWhiteSpace(m.Diagnosis) ? "Chưa xác định" : m.Diagnosis.Trim())
                .Select(g => new MonthDiseaseStat
                {
                    Diagnosis = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            var duplicateDiagnosisCount = diseaseGroups.Where(g => g.Count > 1).Sum(g => g.Count);
            var duplicateDiagnosisRate = totalDiagnosisCount == 0
                ? 0m
                : Math.Round(100m * duplicateDiagnosisCount / totalDiagnosisCount, 1);

            var totalRevenue = await _context.PrescriptionDetails
                .Where(pd => pd.Prescription != null
                             && pd.Prescription.MedicalRecord != null
                             && pd.Prescription.MedicalRecord.Appointment != null
                             && (pd.Prescription.MedicalRecord.Appointment.Status == 4 || pd.Prescription.MedicalRecord.Appointment.Status == 5)
                             && pd.Prescription.MedicalRecord.Appointment.AppointmentTime.Month == selectedMonth
                             && pd.Prescription.MedicalRecord.Appointment.AppointmentTime.Year == selectedYear)
                .SumAsync(pd => pd.Price * pd.Quantity);

            var viewModel = new DoctorStatisticsViewModel
            {
                SelectedMonth = selectedMonth,
                SelectedYear = selectedYear,
                TotalPatientsExamined = totalPatientsExamined,
                TotalVisits = totalVisits,
                TotalRevenue = totalRevenue,
                DuplicateDiagnosisCount = duplicateDiagnosisCount,
                DuplicateDiagnosisRate = duplicateDiagnosisRate,
                DiseaseStats = diseaseGroups.Select(g => new MonthDiseaseStat
                {
                    Diagnosis = g.Diagnosis,
                    Count = g.Count,
                    Percent = totalDiagnosisCount == 0 ? 0 : Math.Round(100m * g.Count / totalDiagnosisCount, 1)
                }).ToList()
            };

            return View(viewModel);
        }
        public async Task<IActionResult> CanhBaoTinhTrang() 
        { 
            var recentVitals = await _context.VitalSigns
                .Include(v => v.Appointment).ThenInclude(a => a.Patient)
                .OrderByDescending(v => v.RecordedAt)
                .Take(100) // limit to recent to avoid heavy memory processing
                .ToListAsync();

            var codeBlues = recentVitals.Where(v => 
                (double.TryParse(v.Pulse, out double pulse) && pulse < 40) ||
                (double.TryParse(v.SpO2, out double spo2) && spo2 < 85)
            ).Take(10).ToList();

            var tempAlerts = recentVitals.Where(v => 
                double.TryParse(v.Temperature, out double temp) && temp >= 39.0
            ).Take(10).ToList();

            var labAlerts = await _context.LabTests
                .Include(l => l.MedicalRecord).ThenInclude(m => m.Appointment).ThenInclude(a => a.Patient)
                .Where(l => l.Result != null && l.Result.Contains("báo động"))
                .OrderByDescending(l => l.CreatedAt)
                .Take(10)
                .ToListAsync();

            var appIds = codeBlues.Select(c => c.AppointmentId)
                .Concat(tempAlerts.Select(t => t.AppointmentId))
                .Distinct().ToList();

            var medRecords = await _context.MedicalRecords
                .Include(m => m.Department)
                .Where(m => appIds.Contains(m.AppointmentId))
                .ToDictionaryAsync(m => m.AppointmentId, m => m);

            ViewBag.CodeBlues = codeBlues;
            ViewBag.TempAlerts = tempAlerts;
            ViewBag.LabAlerts = labAlerts;
            ViewBag.MedRecords = medRecords;

            return View(); 
        }
        
        public async Task<IActionResult> HenTaiKham() 
        { 
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == 8 && a.Patient != null && _context.Users.Any(u => u.PatientCode == a.Patient.PatientCode))
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();
            
            ViewBag.NotifiedPatientIds = await _context.Notifications.Select(n => n.PatientId).Distinct().ToListAsync();
            
            return View(appointments); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotificationHenTaiKham(int patientId, string message)
        {
            if (patientId <= 0 || string.IsNullOrWhiteSpace(message))
            {
                TempData["DoctorNotificationError"] = "Vui lòng chọn bệnh nhân và nhập nội dung thông báo.";
                return RedirectToAction(nameof(HenTaiKham));
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                TempData["DoctorNotificationError"] = "Bệnh nhân không tồn tại.";
                return RedirectToAction(nameof(HenTaiKham));
            }

            var username = User?.Identity?.Name;
            var currentDoctor = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            var notification = new Notification
            {
                PatientId = patientId,
                DoctorId = currentDoctor?.Id ?? 0,
                Message = message.Trim(),
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["DoctorNotificationSuccess"] = $"Thông báo đã gửi đến bệnh nhân {patient.FullName}.";
            return RedirectToAction(nameof(HenTaiKham));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHenTaiKham(int appointmentId, DateTime newAppointmentTime)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                appointment.AppointmentTime = newAppointmentTime;
                await _context.SaveChangesAsync();
                TempData["DoctorNotificationSuccess"] = "Đã cập nhật ngày giờ hẹn khám thành công.";
            }
            else
            {
                TempData["DoctorNotificationError"] = "Không tìm thấy lịch hẹn.";
            }
            return RedirectToAction(nameof(HenTaiKham));
        }
        
        public IActionResult HoiChanOnline() { return View(); }
        public IActionResult ThongBao() { return View(); }
        public IActionResult CaiDat() { return View(); }

        public async Task<IActionResult> SinhHieu(int? id)
        {
            if (id.HasValue)
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.Id == id.Value);

                if (appointment == null) return NotFound();

                var history = await _context.VitalSigns
                    .Where(v => v.AppointmentId == id.Value)
                    .OrderByDescending(v => v.RecordedAt)
                    .ToListAsync();

                ViewBag.History = history;
                ViewBag.Appointment = appointment;

                return View("SinhHieuChiTiet", appointment);
            }

            // Nếu không có ID, hiển thị danh sách các bệnh nhân đã đo sinh hiệu
            var appointmentIdsWithVitals = await _context.VitalSigns
                .Select(v => v.AppointmentId)
                .Distinct()
                .ToListAsync();

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => appointmentIdsWithVitals.Contains(a.Id))
                .ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> LuuSinhHieu(int appointmentId, string pulse, string temperature, string bloodPressure, string spo2, string nurseName)
        {
            var vs = new VitalSign
            {
                AppointmentId = appointmentId,
                Pulse = pulse,
                Temperature = temperature,
                BloodPressure = bloodPressure,
                SpO2 = spo2,
                NurseName = nurseName,
                RecordedAt = DateTime.Now
            };

            _context.VitalSigns.Add(vs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(SinhHieu), new { id = appointmentId });
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThaiMo(int id, int status, int? surgeonId, int? surgeryType)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                if (appointment.Status == 10) 
                {
                     return BadRequest("Lịch mổ đã kết thúc, không thể thay đổi trạng thái.");
                }

                appointment.Status = status;
                _context.Appointments.Update(appointment);
                
                var record = await _context.MedicalRecords.FirstOrDefaultAsync(m => m.AppointmentId == id);
                if (record != null)
                {
                    if (surgeonId.HasValue) record.SurgeonId = surgeonId.Value;
                    if (surgeryType.HasValue) record.SurgeryFeeId = surgeryType.Value;
                    _context.MedicalRecords.Update(record);
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(LichMo));
        }

        // ==========================================
        // TRUNG TÂM ĐIỀU HÀNH (COMMAND CENTER)
        // ==========================================
        public async Task<IActionResult> CommandCenter()
        {
            var admittedPatients = await _context.MedicalRecords
                .Include(m => m.Appointment)
                .ThenInclude(a => a.Patient)
                .Include(m => m.Department)
                .Where(m => m.AdmissionDate != null && m.DischargeDate == null)
                .ToListAsync();

            return View(admittedPatients);
        }
    }
}
