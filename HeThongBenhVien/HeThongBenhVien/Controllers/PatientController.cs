using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace HeThongBenhVien.Controllers
{
    public class AiSuggestRequest
    {
        public string symptoms { get; set; }
    }

    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public PatientController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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
                
                // Nếu người dùng ĐÃ ĐĂNG NHẬP, tuyệt đối không dùng Cookie của khách
                // để tránh tình trạng xem lẫn lịch của tài khoản khác
                return 0;
            }

            // Hỗ trợ hiển thị lịch khám cho khách (không đăng nhập) bằng Cookie
            if (Request.Cookies.TryGetValue("GuestPatientId", out string? guestIdStr) && int.TryParse(guestIdStr, out int guestId))
            {
                return guestId;
            }

            return 0;
        }

        [Authorize]
        public async Task<IActionResult> Portal()
        {
            int pid = await GetCurrentPatientId();
            if (pid == 0)
            {
                // Thay vì redirect gây lặp, hiển thị thông báo lỗi hoặc yêu cầu liên hệ admin
                ViewBag.ErrorMessage = "Tài khoản của bạn chưa được liên kết với hồ sơ bệnh nhân. Vui lòng liên hệ quản trị viên.";
                return View("ErrorAuth"); 
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == pid);
            ViewBag.PatientName = patient?.FullName ?? "Bệnh nhân";
            ViewBag.PatientCode = patient?.PatientCode ?? "";
            ViewBag.UnreadNotificationCount = await _context.Notifications
                .CountAsync(n => n.PatientId == pid && !n.IsRead && n.IsForPatient);

            // Đếm số lượng hồ sơ bệnh án thực tế
            var recordCount = await _context.MedicalRecords
                .CountAsync(m => m.Appointment != null && m.Appointment.PatientId == pid);
            ViewBag.RecordCount = recordCount;

            // Tìm lịch hẹn sắp tới (AppointmentTime >= Hiện tại, Trạng thái: Chưa đến (0) hoặc Đã xác nhận hẹn online (9))
            var upcomingAppointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Where(a => a.PatientId == pid && a.AppointmentTime >= DateTime.Now && (a.Status == 0 || a.Status == 9 || a.Status == 8))
                .OrderBy(a => a.AppointmentTime)
                .FirstOrDefaultAsync();

            ViewBag.UpcomingAppointment = upcomingAppointment;
            if (upcomingAppointment != null)
            {
                ViewBag.AppointmentTimeStr = upcomingAppointment.AppointmentTime.ToString("HH:mm");
                ViewBag.AppointmentDateStr = upcomingAppointment.AppointmentTime.ToString("dd/MM/yyyy");
                ViewBag.DoctorName = upcomingAppointment.Doctor?.FullName ?? "Bác sĩ khám";
                ViewBag.DepartmentName = upcomingAppointment.Doctor?.Department?.Name ?? "Khoa khám bệnh";
            }
            else
            {
                ViewBag.AppointmentTimeStr = "Không có";
                ViewBag.AppointmentDateStr = "Không có";
                ViewBag.DoctorName = null;
                ViewBag.DepartmentName = null;
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> UserGuide()
        {
            int pid = await GetCurrentPatientId();
            if (pid == 0)
            {
                ViewBag.ErrorMessage = "Tài khoản của bạn chưa được liên kết với hồ sơ bệnh nhân. Vui lòng liên hệ quản trị viên.";
                return View("ErrorAuth"); 
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == pid);
            ViewBag.PatientName = patient?.FullName ?? "Bệnh nhân";
            ViewBag.PatientCode = patient?.PatientCode ?? "";
            ViewBag.UnreadNotificationCount = await _context.Notifications
                .CountAsync(n => n.PatientId == pid && !n.IsRead && n.IsForPatient);
            return View();
        }

        private async Task<List<PatientBookingStatusItem>> GetPatientBookingStatuses(int patientId)
        {
            var bookings = await _context.Appointments
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentTime)
                .Take(10)
                .ToListAsync();

            var notifications = await _context.Notifications
                .Include(n => n.Doctor)
                .Where(n => n.PatientId == patientId && n.IsForPatient)
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
            // Lấy bác sĩ có liên kết với khoa đó
            var doctors = await _context.Users
                .Where(u => u.Role == "Doctor" && _context.DoctorDepartments.Any(dd => dd.DoctorId == u.Id && dd.DepartmentId == departmentId))
                .Select(u => new { id = u.Id, fullName = u.FullName })
                .ToListAsync();

            // Nếu không có bác sĩ liên kết với khoa, trả về tất cả bác sĩ
            if (!doctors.Any())
            {
                doctors = await _context.Users
                    .Where(u => u.Role == "Doctor")
                    .Select(u => new { id = u.Id, fullName = u.FullName })
                    .ToListAsync();
            }

            return Json(doctors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Booking(string FullName, string PhoneNumber, string Reason, DateTime AppointmentTime, int? DoctorId)
        {
            Patient? patient = null;

            // 1. Ưu tiên: Nếu bệnh nhân đang đăng nhập, lấy đúng Patient record của họ
            var loggedInUsername = User?.Identity?.Name;
            User? currentUser = null;
            if (!string.IsNullOrEmpty(loggedInUsername))
            {
                currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == loggedInUsername);
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

            // Create notification for Doctor
            if (model.DoctorId.HasValue)
            {
                var notification = new Notification
                {
                    DoctorId = model.DoctorId.Value,
                    PatientId = patient.Id,
                    Message = $"Bệnh nhân {patient.FullName} đã đặt lịch khám vào lúc {model.AppointmentTime:dd/MM/yyyy HH:mm} (Gửi lúc: {DateTime.Now:dd/MM/yyyy HH:mm}).",
                    Type = NotificationType.Appointment,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    IsForPatient = false
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            // Tính E-Ticket (số thứ tự điện tử trong ngày)
            var countToday = await _context.Appointments
                .Where(a => a.AppointmentTime.Date == model.AppointmentTime.Date)
                .CountAsync();

            // Liên kết PatientCode cho User nếu họ đang đăng nhập nhưng chưa có mã bệnh nhân
            if (currentUser != null && string.IsNullOrEmpty(currentUser.PatientCode))
            {
                currentUser.PatientCode = patient.PatientCode;
                _context.Users.Update(currentUser);
                await _context.SaveChangesAsync();
            }

            // Lưu Cookie để bệnh nhân (kể cả khách không đăng nhập) xem lại được lịch khám khi quay lại trang
            Response.Cookies.Append("GuestPatientId", patient.Id.ToString(), new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30) });
                
            TempData["PendingMessage"] = $"Yêu cầu đặt lịch đã được gửi! Số thứ tự điện tử (E-ticket): {countToday:D3}. Vui lòng chờ bác sĩ xác nhận.";
            return RedirectToAction(nameof(Booking));
        }

        [Authorize]
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

        [Authorize]
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
        [Authorize]
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

        [Authorize]
        public async Task<IActionResult> Notifications()
        {
            int pid = await GetCurrentPatientId();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.PatientId == pid && !n.IsRead && n.IsForPatient)
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
                .Where(n => n.PatientId == pid && n.IsForPatient)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.UpcomingAppointments = upcomingAppointments;
            ViewBag.RecentPrescription = recentPrescription;
            ViewBag.Notifications = notifications;

            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Rating()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
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
        [HttpPost]
        public async Task<IActionResult> SuggestDepartmentAI([FromBody] AiSuggestRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.symptoms))
                return Json(new { success = false, message = "Vui lòng nhập triệu chứng." });

            try
            {
                var departments = await _context.Departments.Select(d => new { d.Id, d.DepartmentName }).ToListAsync();
                string deptsJson = JsonSerializer.Serialize(departments);

                string apiKey = _config["OpenAiApiKey"];

                string prompt = $"Một bệnh nhân có triệu chứng sau: '{req.symptoms}'. " +
                                $"Bệnh viện có các chuyên khoa sau (định dạng JSON): {deptsJson}. " +
                                "Dựa vào triệu chứng, hãy chọn 1 chuyên khoa phù hợp nhất để khám. " +
                                "CHỈ TRẢ VỀ ID (số nguyên) CỦA CHUYÊN KHOA ĐÓ, không thêm bất kỳ văn bản, giải thích hay markdown nào khác.";

                if (!string.IsNullOrEmpty(apiKey))
                {
                    try
                    {
                        var payload = new
                        {
                            model = "gpt-4o-mini",
                            messages = new[]
                            {
                                new { role = "system", content = "Bạn là một bác sĩ hỗ trợ chọn chuyên khoa." },
                                new { role = "user", content = prompt }
                            }
                        };
                        using var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            using JsonDocument doc = JsonDocument.Parse(responseString);
                            string resultText = doc.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content").GetString();

                            if (!string.IsNullOrEmpty(resultText))
                            {
                                resultText = resultText.Trim();
                                string onlyDigits = new string(resultText.Where(char.IsDigit).ToArray());

                                if (!string.IsNullOrEmpty(onlyDigits) && int.TryParse(onlyDigits, out int deptId))
                                {
                                    return Json(new { success = true, departmentId = deptId, isAi = true, engine = "OpenAI" });
                                }
                            }
                        }
                        // Nếu API lỗi (hết quota, sai key...) sẽ tự động rơi xuống chế độ dự phòng bên dưới
                    }
                    catch
                    {
                        // Bỏ qua lỗi, chạy tiếp xuống AI Dự Phòng
                    }
                }

                // --- CHẾ ĐỘ DỰ PHÒNG (FALLBACK) ---
                // Được kích hoạt nếu API Key (cả OpenAI lẫn Gemini) bị lỗi hết tiền, hết quota hoặc cấu hình sai
                string s = req.symptoms.ToLower();
                int fallbackDeptId = departments.FirstOrDefault()?.Id ?? 0;

                if (s.Contains("mỏi tay") || s.Contains("đau tay") || s.Contains("mỏi vai") || s.Contains("vai gáy") || s.Contains("đau lưng") || s.Contains("xương") || s.Contains("khớp") || s.Contains("gãy")) {
                    var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("xương") || x.DepartmentName.ToLower().Contains("khớp") || x.DepartmentName.ToLower().Contains("ngoại"));
                    if (d != null) fallbackDeptId = d.Id;
                }
                else if (s.Contains("ho") || s.Contains("sổ mũi") || s.Contains("đau họng") || s.Contains("tai ") || s.Contains("mũi ")) {
                    var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("tai") || x.DepartmentName.ToLower().Contains("họng") || x.DepartmentName.ToLower().Contains("nội"));
                    if (d != null) fallbackDeptId = d.Id;
                }
                else if (s.Contains("răng") || s.Contains("nướu") || s.Contains("hàm")) {
                    var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("răng") || x.DepartmentName.ToLower().Contains("hàm"));
                    if (d != null) fallbackDeptId = d.Id;
                }
                else if (s.Contains("mắt") || s.Contains("mờ") || s.Contains("cận")) {
                    var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("mắt"));
                    if (d != null) fallbackDeptId = d.Id;
                }
                else if (s.Contains("đau đầu") || s.Contains("chóng mặt") || s.Contains("đau bụng") || s.Contains("buồn nôn") || s.Contains("sốt") || s.Contains("tim") || s.Contains("huyết áp")) {
                    var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("nội") || x.DepartmentName.ToLower().Contains("tổng hợp") || x.DepartmentName.ToLower().Contains("tim"));
                    if (d != null) fallbackDeptId = d.Id;
                }

                if (fallbackDeptId > 0)
                {
                    return Json(new { success = true, departmentId = fallbackDeptId, isAi = false, message = "Cảnh báo: API Key (OpenAI/Gemini) của bạn đã hết tiền/Quota. Hệ thống tự động kích hoạt AI Dự phòng để không làm gián đoạn." });
                }

                return Json(new { success = false, message = "Hệ thống AI từ xa hết hạn mức và không thể tự phân tích." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi kết nối AI: " + ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> ChatAI()
        {
            var username = User?.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            ViewBag.PatientName = user?.FullName ?? "Bệnh nhân";
            ViewBag.PatientCode = user?.PatientCode ?? "";
            
            // Get unread notification count
            int pid = await GetCurrentPatientId();
            ViewBag.UnreadNotificationCount = await _context.Notifications
                .CountAsync(n => n.PatientId == pid && !n.IsRead && n.IsForPatient);

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendChatMessage([FromBody] ChatAiRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.message))
                return Json(new { success = false, message = "Tin nhắn không được để trống." });

            try
            {
                string apiKey = _config["GeminiApiKey"] ?? "AQ.Ab8RN6J4SMQmPbxcj4SVJiawQciJYbwlAcwgfj4oZxwqBNDjNQ";
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

                var contentsList = new List<object>();

                if (req.history != null)
                {
                    foreach (var h in req.history)
                    {
                        contentsList.Add(new
                        {
                            role = h.role == "user" ? "user" : "model",
                            parts = new object[] { new { text = h.content } }
                        });
                    }
                }

                contentsList.Add(new
                {
                    role = "user",
                    parts = new object[] { new { text = req.message } }
                });

                var payload = new
                {
                    systemInstruction = new
                    {
                        parts = new object[]
                        {
                            new { text = "Bạn là trợ lý AI Y tế Chuyên sâu (Medical AI Advisor) của hệ thống QL KCB. Nhiệm vụ của bạn là tư vấn sức khỏe cho bệnh nhân. Hãy trả lời cực kỳ ân cần, khoa học, chính xác bằng tiếng Việt. Gợi ý các chuyên khoa hoặc lối sống lành mạnh nếu phù hợp. Luôn nhắc nhở thân thiện rằng lời khuyên của bạn chỉ mang tính tham khảo và khuyên họ nên đặt lịch khám với bác sĩ chuyên khoa nếu triệu chứng kéo dài hoặc nghiêm trọng." }
                        }
                    },
                    contents = contentsList
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(responseString);
                    string reply = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text").GetString();

                    return Json(new { success = true, reply = reply });
                }

                string errorMsg = await response.Content.ReadAsStringAsync();
                return Json(new { success = false, message = "Hệ thống AI không phản hồi: " + errorMsg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi kết nối AI: " + ex.Message });
            }
        }
    }

    public class ChatAiRequest
    {
        public string message { get; set; }
        public List<ChatMessage> history { get; set; }
    }

    public class ChatMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}
