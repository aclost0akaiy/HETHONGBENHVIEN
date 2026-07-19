using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HeThongBenhVien.Controllers.PatientRating
{
    [Route("Patient/Rating")]
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.PatientName = user.FullName ?? "";
            ViewBag.PatientPhone = user.SDT ?? "";
            ViewBag.CurrentUserId = user.Id;
            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();

            return View("~/Views/Patient/Rating.cshtml");
        }

        [HttpPost]
        [Route("SubmitFeedback")]
        public async Task<IActionResult> SubmitFeedback(
            int DepartmentId,
            int RatingOverall,
            int ThaiDo,
            int VeSinh,
            string? ChuyenMon,
            string? CSVC,
            string? ThoiGianCho,
            string? Content,
            bool AllowContact,
            List<IFormFile>? reviewerFiles)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            var dept = await _context.Departments.FindAsync(DepartmentId);
            if (dept == null)
            {
                return Json(new { success = false, message = "Khoa điều trị không hợp lệ." });
            }

            if (RatingOverall < 1 || RatingOverall > 5 || ThaiDo < 1 || ThaiDo > 5 || VeSinh < 1 || VeSinh > 5)
            {
                return Json(new { success = false, message = "Đánh giá sao phải từ 1 đến 5." });
            }

            if (string.IsNullOrEmpty(Content) || string.IsNullOrWhiteSpace(Content))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung đánh giá." });
            }

            if (reviewerFiles != null && reviewerFiles.Count > 10)
            {
                return Json(new { success = false, message = "Chỉ được phép đính kèm tối đa 10 hình ảnh." });
            }

            // Upload files
            var imageUrlsList = new List<string>();
            if (reviewerFiles != null && reviewerFiles.Count > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                foreach (var file in reviewerFiles)
                {
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = $"Hình ảnh '{file.FileName}' vượt quá dung lượng cho phép (tối đa 5MB)." });
                    }
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(ext))
                    {
                        return Json(new { success = false, message = $"Định dạng tệp của '{file.FileName}' không hợp lệ. Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .gif" });
                    }
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reviews");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                foreach (var file in reviewerFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        imageUrlsList.Add("/uploads/reviews/" + fileName);
                    }
                }
            }

            var feedback = new Feedback
            {
                UserId = user.Id,
                DepartmentId = DepartmentId,
                RatingOverall = RatingOverall,
                ThaiDo = ThaiDo,
                VeSinh = VeSinh,
                ChuyenMon = ChuyenMon,
                CSVC = CSVC,
                ThoiGianCho = ThoiGianCho,
                Content = Content.Trim(),
                ImageUrl = imageUrlsList.Any() ? string.Join(";", imageUrlsList) : null,
                Status = "Pending",
                AllowContact = AllowContact,
                CreatedAt = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            // Create initial response containing the user's comment
            var initialResponse = new Response
            {
                FeedbackId = feedback.Id,
                Sender = "User",
                Content = feedback.Content,
                CreatedAt = feedback.CreatedAt
            };
            _context.Responses.Add(initialResponse);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Gửi đánh giá thành công!" });
        }

        [HttpGet]
        [Route("GetMyFeedback")]
        public async Task<IActionResult> GetMyFeedback(int page = 1, int pageSize = 5)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            var query = _context.Feedbacks
                .Include(f => f.Department)
                .Where(f => f.UserId == user.Id);

            var totalCount = await query.CountAsync();
            var feedbacks = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<object>();
            foreach (var f in feedbacks)
            {
                var responses = await _context.Responses
                    .Where(r => r.FeedbackId == f.Id)
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => new
                    {
                        id = r.Id,
                        sender = r.Sender,
                        content = r.Content,
                        createdAt = r.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                    })
                    .ToListAsync();

                var images = string.IsNullOrEmpty(f.ImageUrl) 
                    ? new List<string>() 
                    : f.ImageUrl.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                result.Add(new
                {
                    id = f.Id,
                    userId = f.UserId,
                    departmentId = f.DepartmentId,
                    departmentName = f.Department?.DepartmentName ?? "Khoa phòng",
                    ratingOverall = f.RatingOverall,
                    thaiDo = f.ThaiDo,
                    veSinh = f.VeSinh,
                    chuyenMon = f.ChuyenMon ?? "",
                    csvc = f.CSVC ?? "",
                    thoiGianCho = f.ThoiGianCho ?? "",
                    content = f.Content ?? "",
                    images = images,
                    status = f.Status,
                    allowContact = f.AllowContact,
                    createdAt = f.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    responses = responses
                });
            }

            return Json(new { success = true, data = result, totalCount = totalCount });
        }

        [HttpPost]
        [Route("SendReply")]
        public async Task<IActionResult> SendReply(int feedbackId, string content)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung phản hồi không được để trống." });
            }

            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null || feedback.UserId != user.Id)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá tương ứng." });
            }

            var response = new Response
            {
                FeedbackId = feedbackId,
                Sender = "User",
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Responses.Add(response);
            feedback.Status = "Pending";
            _context.Feedbacks.Update(feedback);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Gửi phản hồi thành công!",
                data = new
                {
                    id = response.Id,
                    sender = response.Sender,
                    content = response.Content,
                    createdAt = response.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        [HttpPost]
        [Route("DeleteReview")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null || feedback.UserId != user.Id)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá." });
            }

            if (feedback.Status == "Responded" || feedback.Status == "Closed")
            {
                return Json(new { success = false, message = "Đánh giá đã được phản hồi hoặc đóng, không thể xóa." });
            }

            if (!string.IsNullOrEmpty(feedback.ImageUrl))
            {
                var images = feedback.ImageUrl.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var img in images)
                {
                    try
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                    catch { }
                }
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa đánh giá thành công!" });
        }

        [HttpPost]
        [Route("UpdateReview")]
        public async Task<IActionResult> UpdateReview(
            int Id,
            int DepartmentId,
            int RatingOverall,
            int ThaiDo,
            int VeSinh,
            string? ChuyenMon,
            string? CSVC,
            string? ThoiGianCho,
            string? Content,
            bool AllowContact,
            List<IFormFile>? reviewerFiles,
            List<string>? keepImages)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            var feedback = await _context.Feedbacks.FindAsync(Id);
            if (feedback == null || feedback.UserId != user.Id)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá." });
            }

            if (feedback.Status == "Responded" || feedback.Status == "Closed")
            {
                return Json(new { success = false, message = "Đánh giá đã được phản hồi hoặc đóng, không thể chỉnh sửa." });
            }

            var dept = await _context.Departments.FindAsync(DepartmentId);
            if (dept == null)
            {
                return Json(new { success = false, message = "Khoa điều trị không hợp lệ." });
            }

            if (RatingOverall < 1 || RatingOverall > 5 || ThaiDo < 1 || ThaiDo > 5 || VeSinh < 1 || VeSinh > 5)
            {
                return Json(new { success = false, message = "Đánh giá sao phải từ 1 đến 5." });
            }

            if (string.IsNullOrEmpty(Content) || string.IsNullOrWhiteSpace(Content))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung đánh giá." });
            }

            var keptImagesList = new List<string>();
            if (keepImages != null && !string.IsNullOrEmpty(feedback.ImageUrl))
            {
                var currentImages = feedback.ImageUrl.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var img in currentImages)
                {
                    if (keepImages.Contains(img))
                    {
                        keptImagesList.Add(img);
                    }
                    else
                    {
                        try
                        {
                            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.TrimStart('/'));
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                        }
                        catch { }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(feedback.ImageUrl))
            {
                var currentImages = feedback.ImageUrl.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var img in currentImages)
                {
                    try
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                    catch { }
                }
            }

            int newFilesCount = reviewerFiles?.Count ?? 0;
            if (keptImagesList.Count + newFilesCount > 10)
            {
                return Json(new { success = false, message = "Tổng số ảnh đính kèm không được vượt quá 10 ảnh." });
            }

            if (reviewerFiles != null && reviewerFiles.Count > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                foreach (var file in reviewerFiles)
                {
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = $"Hình ảnh '{file.FileName}' vượt quá dung lượng cho phép (tối đa 5MB)." });
                    }
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(ext))
                    {
                        return Json(new { success = false, message = $"Định dạng tệp của '{file.FileName}' không hợp lệ. Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .gif" });
                    }
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reviews");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                foreach (var file in reviewerFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        keptImagesList.Add("/uploads/reviews/" + fileName);
                    }
                }
            }

            feedback.DepartmentId = DepartmentId;
            feedback.RatingOverall = RatingOverall;
            feedback.ThaiDo = ThaiDo;
            feedback.VeSinh = VeSinh;
            feedback.ChuyenMon = ChuyenMon;
            feedback.CSVC = CSVC;
            feedback.ThoiGianCho = ThoiGianCho;
            feedback.Content = Content.Trim();
            feedback.ImageUrl = keptImagesList.Any() ? string.Join(";", keptImagesList) : null;
            feedback.AllowContact = AllowContact;

            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();

            var initialResponse = await _context.Responses
                .Where(r => r.FeedbackId == feedback.Id && r.Sender == "User")
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (initialResponse != null)
            {
                initialResponse.Content = feedback.Content;
                _context.Responses.Update(initialResponse);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Cập nhật đánh giá thành công!" });
        }
    }
}
