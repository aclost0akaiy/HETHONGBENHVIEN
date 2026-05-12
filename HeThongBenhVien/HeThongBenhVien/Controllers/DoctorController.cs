using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;
using System;
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
            var allAppointmentsQuery = _context.Appointments;

            var unexaminedCount = await allAppointmentsQuery.CountAsync(a => a.Status != 4 && a.Status != 5);
            var completedCount = await allAppointmentsQuery.CountAsync(a => a.Status == 4 || a.Status == 5);
            var waitingCount = await allAppointmentsQuery.CountAsync(a => a.Status == 1);
            var emergencyCount = await allAppointmentsQuery.CountAsync(a => a.Status == 6 || (a.Reason != null && (a.Reason.Contains("cấp cứu") || a.Reason.Contains("thắt ngực"))));

            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status != 4 && a.Status != 5)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            var viewModel = new DoctorDashboardViewModel
            {
                TodayPatientsCount = unexaminedCount,
                CompletedPatientsCount = completedCount,
                WaitingResultsCount = waitingCount,
                EmergencyCount = emergencyCount,
                UpcomingAppointments = upcomingAppointments
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

            var latestAppointment = await _context.Appointments
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.AppointmentTime)
                .FirstOrDefaultAsync();

            ViewBag.LatestAppointmentStatus = latestAppointment?.Status ?? 0;
            ViewBag.HasAppointment = latestAppointment != null;

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
                
                // Cập nhật trạng thái lịch khám thành Chờ xác nhận (7) thay vì 4
                var appointment = await _context.Appointments.FindAsync(model.AppointmentId);
                if (appointment != null)
                {
                    appointment.Status = 7;
                    _context.Appointments.Update(appointment);
                }

                await _context.SaveChangesAsync();
                // Sửa redirect: chuyển đến trang ChiTietBenhAn để bác sĩ có thể ấn các nút chức năng (Kê đơn, Xét nghiệm, Xác nhận)
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
            var record = await _context.MedicalRecords.Include(m => m.Appointment).FirstOrDefaultAsync(m => m.Id == id);
            if (record != null && record.Appointment != null)
            {
                record.Appointment.Status = 5; // Hoàn thành điều trị
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(HoSoBenhAn));
        }

        public async Task<IActionResult> KeDonThuoc(int? id)
        {
            if (id.HasValue)
            {
                // Kiểm tra nếu id là MedicalRecord.Id
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
                    ViewBag.Medicines = await _context.Medicines.ToListAsync();
                    ViewBag.AllPrescriptionDetails = await _context.PrescriptionDetails
                        .Include(pd => pd.Prescription)
                        .ThenInclude(p => p.MedicalRecord)
                        .ThenInclude(m => m.Appointment)
                        .ThenInclude(a => a.Patient)
                        .ToListAsync();

                    return View("KeDonThuocForm", await _context.Prescriptions.Include(p => p.MedicalRecord).ThenInclude(m => m.Appointment).ThenInclude(a => a.Patient).Include(p => p.PrescriptionDetails).FirstOrDefaultAsync(p => p.MedicalRecordId == id.Value));
                }
                else
                {
                    // Nếu không phải MedicalRecord, coi là User.Id
                    var user = await _context.Users.FindAsync(id.Value);
                    if (user != null && user.Role == "BenhNhan")
                    {
                        // Lấy hoặc tạo đối tượng Patient tương ứng với User bệnh nhân
                        var patient = await GetOrCreatePatientFromUser(user);
                        if (patient != null)
                        {
                            // Kiểm tra nếu patient đã có MedicalRecord
                            var existingMedicalRecord = await _context.MedicalRecords
                                .Include(m => m.Appointment)
                                .FirstOrDefaultAsync(m => m.Appointment != null && m.Appointment.PatientId == patient.Id);
                            if (existingMedicalRecord != null)
                            {
                                // Nếu đã có, load existing Prescription
                                var existingPrescription = await _context.Prescriptions.FirstOrDefaultAsync(p => p.MedicalRecordId == existingMedicalRecord.Id);
                                if (existingPrescription == null)
                                {
                                    existingPrescription = new Prescription { MedicalRecordId = existingMedicalRecord.Id, Status = "Đã kê đơn" };
                                    _context.Prescriptions.Add(existingPrescription);
                                    await _context.SaveChangesAsync();
                                }

                                ViewBag.MedicineUnits = await _context.MedicineUnits.ToListAsync();
                                ViewBag.Medicines = await _context.Medicines.ToListAsync();
                                ViewBag.AllPrescriptionDetails = await _context.PrescriptionDetails
                                    .Include(pd => pd.Prescription)
                                    .ThenInclude(p => p.MedicalRecord)
                                    .ThenInclude(m => m.Appointment)
                                    .ThenInclude(a => a.Patient)
                                    .ToListAsync();

                                return View("KeDonThuocForm", await _context.Prescriptions.Include(p => p.MedicalRecord).ThenInclude(m => m.Appointment).ThenInclude(a => a.Patient).Include(p => p.PrescriptionDetails).FirstOrDefaultAsync(p => p.MedicalRecordId == existingMedicalRecord.Id));
                            }
                            else
                            {
                                // Tạo Appointment mới
                                var appointment = new Appointment
                                {
                                    PatientId = patient.Id,
                                    Status = 7, // Chờ kê đơn
                                    AppointmentTime = DateTime.Now,
                                    Reason = "Kê đơn thuốc"
                                };
                                _context.Appointments.Add(appointment);
                                await _context.SaveChangesAsync();

                                // Tạo MedicalRecord
                                var medicalRecord = new MedicalRecord
                                {
                                    AppointmentId = appointment.Id,
                                    Diagnosis = "Chưa chẩn đoán",
                                    Symptoms = "Không có triệu chứng",
                                    TreatmentPlan = "Kê đơn thuốc",
                                    Vitals = "Không có sinh hiệu"
                                };
                                _context.MedicalRecords.Add(medicalRecord);
                                await _context.SaveChangesAsync();

                                // Tạo Prescription
                                var prescription = new Prescription { MedicalRecordId = medicalRecord.Id, Status = "Đã kê đơn" };
                                _context.Prescriptions.Add(prescription);
                                await _context.SaveChangesAsync();

                                medicalRecord.Notes += "\n[KEDONTHUOC]";
                                await _context.SaveChangesAsync();

                                ViewBag.MedicineUnits = await _context.MedicineUnits.ToListAsync();
                                ViewBag.Medicines = await _context.Medicines.ToListAsync();
                                ViewBag.AllPrescriptionDetails = await _context.PrescriptionDetails
                                    .Include(pd => pd.Prescription)
                                    .ThenInclude(p => p.MedicalRecord)
                                    .ThenInclude(m => m.Appointment)
                                    .ThenInclude(a => a.Patient)
                                    .ToListAsync();

                                return View("KeDonThuocForm", await _context.Prescriptions.Include(p => p.MedicalRecord).ThenInclude(m => m.Appointment).ThenInclude(a => a.Patient).Include(p => p.PrescriptionDetails).FirstOrDefaultAsync(p => p.MedicalRecordId == medicalRecord.Id));
                            }
                        }
                    }
                }
            }

            var patients = await _context.Users
                .Where(u => u.Role == "BenhNhan")
                .ToListAsync();
            return View(patients);
        }

        [HttpPost]
        public async Task<IActionResult> LuuToaThuoc(int prescriptionId, int medicineId, int quantity, string unit, string dosage)
        {
            var medicine = await _context.Medicines.FindAsync(medicineId);
            if (medicine == null)
            {
                TempData["ErrorMessage"] = "Thuốc không tồn tại.";
                return RedirectToAction(nameof(KeDonThuoc), new { id = prescriptionId });
            }

            var detail = new PrescriptionDetail
            {
                PrescriptionId = prescriptionId,
                MedicineName = medicine.Name,
                Quantity = quantity,
                Unit = unit,
                DosageInstruction = dosage,
                Price = medicine.Price
            };
            _context.PrescriptionDetails.Add(detail);
            await _context.SaveChangesAsync();
            var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
            return RedirectToAction(nameof(KeDonThuoc), new { id = prescription?.MedicalRecordId });
        }

        private async Task<Patient?> GetOrCreatePatientFromUser(User user)
        {
            if (user == null) return null;

            if (string.IsNullOrWhiteSpace(user.PatientCode))
            {
                user.PatientCode = "BN" + user.Id.ToString("D4");
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientCode == user.PatientCode);
            if (patient != null) return patient;

            patient = new Patient
            {
                FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName,
                PatientCode = user.PatientCode,
                Gender = "Nam",
                Age = 0
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
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
        public async Task<IActionResult> CapNhatKetQuaXetNghiem(int id, string ketQua)
        {
            var test = await _context.LabTests.Include(t => t.MedicalRecord).FirstOrDefaultAsync(t => t.Id == id);
            if (test != null)
            {
                test.Result = ketQua;
                test.Status = "Đã có kết quả";
                test.CompletedAt = System.DateTime.Now;
                
                // Cập nhật Notes của MedicalRecord để hiển thị ở ChiTietBenhAn
                if (test.MedicalRecord != null) {
                    test.MedicalRecord.Notes = test.MedicalRecord.Notes.Replace("[XETNGHIEM_PENDING]", "[XETNGHIEM_DONE]");
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(KetQuaCanLamSang));
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
            var records = await _context.MedicalRecords.Include(m => m.Appointment).ThenInclude(a => a.Patient).Where(m => m.Notes.Contains("[PHAUTHUAT]")).ToListAsync();
            return View(records);
        }

        public async Task<IActionResult> QuanLyGiuong(int? id)
        {
            if (id.HasValue)
            {
                var record = await _context.MedicalRecords.FindAsync(id.Value);
                if (record != null && !record.Notes.Contains("[NHAPVIEN]"))
                {
                    record.Notes += "\n[NHAPVIEN]";
                    await _context.SaveChangesAsync();
                }
            }
            var records = await _context.MedicalRecords.Include(m => m.Appointment).ThenInclude(a => a.Patient).Where(m => m.Notes.Contains("[NHAPVIEN]")).ToListAsync();
            return View(records);
        }

        public IActionResult LichHen() { return View(); }
        public IActionResult SinhHieu() { return View(); }
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
        public IActionResult CanhBaoTinhTrang() { return View(); }
        
        public async Task<IActionResult> HenTaiKham() 
        { 
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Status == 8)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();
            return View(appointments); 
        }
        
        public IActionResult HoiChanOnline() { return View(); }
        public IActionResult ThongBao() { return View(); }
        public IActionResult CaiDat() { return View(); }
    }
}
