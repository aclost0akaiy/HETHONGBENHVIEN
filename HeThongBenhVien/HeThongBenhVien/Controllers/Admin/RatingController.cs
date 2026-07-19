using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HeThongBenhVien.Controllers.AdminRating
{
    [Route("Admin/Rating")]
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
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            return View("~/Views/Admin/DanhGiaChatLuong.cshtml");
        }

        [HttpGet]
        [Route("GetFeedbackDashboardStats")]
        public async Task<IActionResult> GetFeedbackDashboardStats()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            var feedbacks = await _context.Feedbacks.ToListAsync();
            var total = feedbacks.Count;
            var unreplied = feedbacks.Count(f => f.Status == "Pending");
            var replied = feedbacks.Count(f => f.Status == "Responded" || f.Status == "Closed");

            double sumOverall = 0.0; int countOverall = 0;
            double sumStaff = 0.0; int countStaff = 0;
            double sumClean = 0.0; int countClean = 0;
            double sumService = 0.0; int countService = 0;
            double sumFacility = 0.0; int countFacility = 0;
            double sumWait = 0.0; int countWait = 0;

            foreach (var f in feedbacks)
            {
                sumOverall += f.RatingOverall; countOverall++;
                sumStaff += f.ThaiDo * 2.0; countStaff++;
                sumClean += f.VeSinh * 2.0; countClean++;

                // Chuyên môn bác sĩ: "Kém"->2.5, "Trung bình"->5.0, "Tốt"->7.5, "Xuất sắc"->10.0
                if (!string.IsNullOrEmpty(f.ChuyenMon))
                {
                    double val = f.ChuyenMon switch
                    {
                        "Kém" => 2.5,
                        "Trung bình" => 5.0,
                        "Tốt" => 7.5,
                        "Xuất sắc" => 10.0,
                        _ => 7.5
                    };
                    sumService += val;
                    countService++;
                }

                // Cơ sở vật chất: "Kém"->3.3, "Trung bình"->6.6, "Tốt"->10.0
                if (!string.IsNullOrEmpty(f.CSVC))
                {
                    double val = f.CSVC switch
                    {
                        "Kém" => 3.3,
                        "Trung bình" => 6.6,
                        "Tốt" => 10.0,
                        _ => 8.0
                    };
                    sumFacility += val;
                    countFacility++;
                }

                // Thời gian chờ đợi: "> 2h"->2.0, "1h-2h"->4.0, "30-60p"->6.0, "15-30p"->8.0, "< 15p"->10.0
                if (!string.IsNullOrEmpty(f.ThoiGianCho))
                {
                    double val = f.ThoiGianCho switch
                    {
                        "> 2h" => 2.0,
                        "1h-2h" => 4.0,
                        "30-60p" => 6.0,
                        "15-30p" => 8.0,
                        "< 15p" => 10.0,
                        _ => 7.0
                    };
                    sumWait += val;
                    countWait++;
                }
            }

            double avgOverall = countOverall > 0 ? (sumOverall / countOverall) : 0.0;
            double avgStaff = countStaff > 0 ? (sumStaff / countStaff) : 0.0;
            double avgClean = countClean > 0 ? (sumClean / countClean) : 0.0;
            double avgService = countService > 0 ? (sumService / countService) : 0.0;
            double avgFacility = countFacility > 0 ? (sumFacility / countFacility) : 0.0;
            double avgWait = countWait > 0 ? (sumWait / countWait) : 0.0;

            int unrepliedPercent = total > 0 ? (int)Math.Round((double)unreplied * 100 / total) : 0;

            return Json(new
            {
                success = true,
                avgOverall = Math.Round(avgOverall, 1),
                avgStaff = Math.Round(avgStaff, 1),
                avgService = Math.Round(avgService, 1),
                avgCleanliness = Math.Round(avgClean, 1),
                avgFacility = Math.Round(avgFacility, 1),
                avgWaitTime = Math.Round(avgWait, 1),
                totalCount = total,
                unrepliedCount = unreplied,
                unrepliedPercent = unrepliedPercent
            });
        }

        [HttpGet]
        [Route("GetListFeedback")]
        public async Task<IActionResult> GetListFeedback(string? khoa, int? soSao, string? trangThai, string? keyword, int page = 1, int pageSize = 10)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            var query = _context.Feedbacks.Include(f => f.Department).Include(f => f.User).AsQueryable();

            if (!string.IsNullOrEmpty(khoa) && khoa != "All")
            {
                query = query.Where(f => f.Department != null && f.Department.DepartmentName == khoa);
            }

            if (!string.IsNullOrEmpty(trangThai) && trangThai != "All")
            {
                if (trangThai == "Chưa phản hồi")
                {
                    query = query.Where(f => f.Status == "Pending");
                }
                else if (trangThai == "Đã phản hồi")
                {
                    query = query.Where(f => f.Status == "Responded" || f.Status == "Closed");
                }
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                var kw = keyword.ToLower();
                query = query.Where(f => (f.User != null && f.User.FullName.ToLower().Contains(kw))
                                      || (f.Department != null && f.Department.DepartmentName.ToLower().Contains(kw))
                                      || (f.Content != null && f.Content.ToLower().Contains(kw)));
            }

            var allItems = await query.ToListAsync();

            var processedItems = allItems.Select(f =>
            {
                var lastMessageTime = _context.Responses
                    .Where(r => r.FeedbackId == f.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.CreatedAt)
                    .FirstOrDefault();

                if (lastMessageTime == default) lastMessageTime = f.CreatedAt;

                return new { f, lastMessageTime };
            }).ToList();

            if (soSao.HasValue && soSao > 0)
            {
                processedItems = processedItems.Where(x => x.f.RatingOverall == soSao.Value).ToList();
            }

            processedItems = processedItems.OrderByDescending(x => x.lastMessageTime).ToList();

            var totalCount = processedItems.Count;
            var paginatedItems = processedItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x =>
                {
                    var f = x.f;
                    var adminReply = _context.Responses
                        .Where(r => r.FeedbackId == f.Id && r.Sender == "Admin")
                        .OrderByDescending(r => r.CreatedAt)
                        .FirstOrDefault();

                    return new
                    {
                        id = f.Id,
                        reviewerName = f.User?.FullName ?? "Bệnh nhân ẩn danh",
                        reviewerPhone = f.User?.SDT ?? "",
                        department = f.Department?.DepartmentName ?? "Khoa phòng",
                        overallScore = f.RatingOverall,
                        stars = f.RatingOverall,
                        displayScore = (double)f.RatingOverall,
                        comment = f.Content ?? "",
                        ratingReason = $"Chuyên môn: {f.ChuyenMon ?? "N/A"}, CSVC: {f.CSVC ?? "N/A"}, Chờ: {f.ThoiGianCho ?? "N/A"}",
                        visitDate = f.CreatedAt.ToString("yyyy-MM-dd"),
                        status = f.Status == "Responded" || f.Status == "Closed" ? "Đã phản hồi" : "Chưa phản hồi",
                        createdAt = f.CreatedAt.ToString("dd/MM/yyyy"),
                        repliedBy = adminReply != null ? "Administrator" : "",
                        repliedAt = adminReply?.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "",
                        replyComment = adminReply?.Content ?? "",
                        attachmentPath = string.IsNullOrEmpty(f.ImageUrl) ? "" : f.ImageUrl.Split(';').FirstOrDefault() ?? "",
                        responseAttachmentPath = ""
                    };
                })
                .ToList();

            return Json(new
            {
                success = true,
                data = paginatedItems,
                totalCount = totalCount,
                page = page,
                pageSize = pageSize
            });
        }

        [HttpGet]
        [Route("GetFeedbackDetail/{id}")]
        public async Task<IActionResult> GetFeedbackDetail(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            var f = await _context.Feedbacks
                .Include(x => x.User)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (f == null) return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            var images = string.IsNullOrEmpty(f.ImageUrl)
                ? new List<string>()
                : f.ImageUrl.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var threads = await _context.Responses
                .Where(r => r.FeedbackId == f.Id)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new
                {
                    id = r.Id,
                    senderType = r.Sender == "Admin" ? "Admin" : "Patient",
                    messageContent = r.Content,
                    createdAt = r.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            var adminReply = await _context.Responses
                .Where(r => r.FeedbackId == f.Id && r.Sender == "Admin")
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            return Json(new
            {
                success = true,
                data = new
                {
                    id = f.Id,
                    reviewerName = f.User?.FullName ?? "Bệnh nhân ẩn danh",
                    reviewerPhone = f.User?.SDT ?? "",
                    department = f.Department?.DepartmentName ?? "Khoa phòng",
                    overallScore = f.RatingOverall,
                    displayScore = (double)f.RatingOverall,
                    ratingReason = $"Thái độ: {f.ThaiDo}★, Vệ sinh: {f.VeSinh}★, Chuyên môn: {f.ChuyenMon}, CSVC: {f.CSVC}, Chờ: {f.ThoiGianCho}",
                    visitDate = f.CreatedAt.ToString("yyyy-MM-dd"),
                    comment = f.Content ?? "",
                    status = f.Status == "Responded" || f.Status == "Closed" ? "Đã phản hồi" : "Chưa phản hồi",
                    createdAt = f.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    repliedBy = "Administrator",
                    repliedAt = adminReply?.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "",
                    replyComment = adminReply?.Content ?? "",
                    images = images,
                    threads = threads
                }
            });
        }

        [HttpPost]
        [Route("SubmitResponse")]
        public async Task<IActionResult> SubmitResponse(int feedbackId, string content, bool isUpdate)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Không có quyền thực hiện hành động." });
            }

            var feedback = await _context.Feedbacks.FindAsync(feedbackId);
            if (feedback == null) return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            if (isUpdate)
            {
                var lastAdminResponse = await _context.Responses
                    .Where(r => r.FeedbackId == feedbackId && r.Sender == "Admin")
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (lastAdminResponse != null)
                {
                    lastAdminResponse.Content = content.Trim();
                    lastAdminResponse.CreatedAt = DateTime.Now;
                    _context.Responses.Update(lastAdminResponse);
                }
                else
                {
                    var response = new Response
                    {
                        FeedbackId = feedbackId,
                        Sender = "Admin",
                        Content = content.Trim(),
                        CreatedAt = DateTime.Now
                    };
                    _context.Responses.Add(response);
                }
            }
            else
            {
                var response = new Response
                {
                    FeedbackId = feedbackId,
                    Sender = "Admin",
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now
                };
                _context.Responses.Add(response);
            }

            feedback.Status = "Responded";
            _context.Feedbacks.Update(feedback);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Gửi phản hồi thành công" });
        }

        [HttpPost]
        [Route("DeleteFeedback")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return Json(new { success = false, message = "Không có quyền thực hiện hành động." });
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            if (!string.IsNullOrEmpty(feedback.ImageUrl))
            {
                var files = feedback.ImageUrl.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var file in files)
                {
                    try
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.TrimStart('/'));
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

            return Json(new { success = true, message = "Xóa đánh giá thành công" });
        }
    }
}
