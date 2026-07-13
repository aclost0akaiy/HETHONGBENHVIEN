using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                var redirectUri = context.RedirectUri;
                if (context.Request.Path.Value != null && context.Request.Path.Value.StartsWith("/Patient", StringComparison.OrdinalIgnoreCase))
                {
                    var patientRedirectUri = redirectUri.Replace("/Account/Login", "/Account/PatientLogin");
                    context.Response.Redirect(patientRedirectUri);
                }
                else
                {
                    context.Response.Redirect(redirectUri);
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<ReexaminationNotificationService>();

var app = builder.Build();

// Seed default application users so login works on first run.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        // Auto-fix schema for Email, SDT, PatientCode in Users table in case they recreated from old BenhVien.sql
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Email' AND Object_ID = Object_ID(N'Users'))
            BEGIN
                ALTER TABLE Users ADD Email NVARCHAR(100) NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SDT' AND Object_ID = Object_ID(N'Users'))
            BEGIN
                ALTER TABLE Users ADD SDT NVARCHAR(20) NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'PatientCode' AND Object_ID = Object_ID(N'Users'))
            BEGIN
                ALTER TABLE Users ADD PatientCode NVARCHAR(20) NULL;
            END

            -- Auto-fix QualityReviews schema
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ReviewerPhone' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD ReviewerPhone NVARCHAR(20) NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ReplyComment' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD ReplyComment NVARCHAR(1000) NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'RepliedBy' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD RepliedBy NVARCHAR(100) NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'RepliedAt' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD RepliedAt DATETIME2 NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'AttachmentPath' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD AttachmentPath NVARCHAR(500) NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ResponseAttachmentPath' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD ResponseAttachmentPath NVARCHAR(500) NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'OverallScore' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD OverallScore INT NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsAnonymous' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD IsAnonymous BIT NOT NULL DEFAULT 0;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'UserId' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD UserId INT NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'DepartmentId' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD DepartmentId INT NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'RatingReason' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD RatingReason NVARCHAR(200) NULL;
            END
            IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'VisitDate' AND Object_ID = Object_ID(N'QualityReviews'))
            BEGIN
                ALTER TABLE QualityReviews ADD VisitDate DATETIME2 NULL;
            END

            -- Create ReviewImages table if not exists
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReviewImages')
            BEGIN
                CREATE TABLE ReviewImages (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    QualityReviewId INT NOT NULL,
                    ImageUrl NVARCHAR(500) NOT NULL,
                    ImagePath NVARCHAR(500) NULL,
                    FOREIGN KEY (QualityReviewId) REFERENCES QualityReviews(Id) ON DELETE CASCADE
                );
            END
            ELSE
            BEGIN
                IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ImagePath' AND Object_ID = Object_ID(N'ReviewImages'))
                BEGIN
                    ALTER TABLE ReviewImages ADD ImagePath NVARCHAR(500) NULL;
                END
            END


            -- Create ReviewThreads table if not exists
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReviewThreads')
            BEGIN
                CREATE TABLE ReviewThreads (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    QualityReviewId INT NOT NULL,
                    SenderType NVARCHAR(50) NOT NULL, -- 'Patient' or 'Admin'
                    MessageContent NVARCHAR(MAX) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL,
                    IsAdminReply INT NOT NULL DEFAULT 0,
                    FOREIGN KEY (QualityReviewId) REFERENCES QualityReviews(Id) ON DELETE CASCADE
                );
            END
            ELSE
            BEGIN
                IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsAdminReply' AND Object_ID = Object_ID(N'ReviewThreads'))
                BEGIN
                    ALTER TABLE ReviewThreads ADD IsAdminReply INT NOT NULL DEFAULT 0;
                END
            END

            -- Normalize status terminology to 'Chưa phản hồi' and 'Đã phản hồi'
            UPDATE QualityReviews SET Status = N'Chưa phản hồi' WHERE Status IN (N'Chờ xử lý', N'Đang xử lý', N'Chưa trả lời', N'Chưa phản hồi');

            -- Create Feedbacks table if not exists
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Feedbacks')
            BEGIN
                CREATE TABLE Feedbacks (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    UserId INT NULL,
                    DepartmentId INT NOT NULL,
                    RatingOverall INT NOT NULL,
                    ThaiDo INT NOT NULL,
                    VeSinh INT NOT NULL,
                    ChuyenMon NVARCHAR(50) NULL,
                    CSVC NVARCHAR(50) NULL,
                    ThoiGianCho NVARCHAR(50) NULL,
                    Content NVARCHAR(2000) NULL,
                    ImageUrl NVARCHAR(2000) NULL,
                    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
                    AllowContact BIT NOT NULL DEFAULT 0,
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
                    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE CASCADE
                );
            END

            -- Create Responses table if not exists
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Responses')
            BEGIN
                CREATE TABLE Responses (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    FeedbackId INT NOT NULL,
                    Sender NVARCHAR(20) NOT NULL,
                    Content NVARCHAR(2000) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                    FOREIGN KEY (FeedbackId) REFERENCES Feedbacks(Id) ON DELETE CASCADE
                );

            -- Auto-fix corrupted testName and status in LabTests table from double-encoded seed data
            IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LabTests')
            BEGIN
                UPDATE LabTests SET TestName = N'Xét nghiệm sinh hóa máu' WHERE TestName LIKE '%nghi%m%sinh%' OR TestName LIKE '%X%t%nghi%m%sinh%';
                UPDATE LabTests SET TestName = N'Siêu âm ổ bụng' WHERE TestName LIKE '%S%u%m%b%' OR TestName LIKE '%Sieu%am%';
                UPDATE LabTests SET TestName = N'Chụp X-Quang ngực' WHERE (TestName LIKE '%Ch%p%X%Quang%' OR TestName LIKE '%Chup%X%Quang%' OR TestName LIKE '%Ch%p%ng%c%') AND TestName NOT IN ('XQuang');
                UPDATE LabTests SET TestName = N'Xét nghiệm nước tiểu' WHERE TestName LIKE '%n%c%ti%u%' OR TestName LIKE '%NuocTieu%';

                UPDATE LabTests SET Status = N'Đã có kết quả' WHERE Status LIKE '%qu%' OR Status LIKE '%k%t%' OR Status LIKE '%hoan%thanh%';
                UPDATE LabTests SET Status = N'Chờ xét nghiệm' WHERE Status LIKE '%nghi%m%';
                UPDATE LabTests SET Status = N'Chờ kết quả' WHERE Status LIKE 'Ch%' AND Status NOT LIKE '%nghi%m%';
            END

            -- Auto-fix corrupted medical records
            IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MedicalRecords')
            BEGIN
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Vim h?ng c?p', N'Viêm họng cấp');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Vim h?ng', N'Viêm họng');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'C?m cm', N'Cảm cúm');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Vim gan c?p', N'Viêm gan cấp');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Vim ru?t', N'Viêm ruột');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Vim ph? qu?n', N'Viêm phế quản');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Suy h h?p c?p', N'Suy hô hấp cấp');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Suy nhu?c co th?', N'Suy nhược cơ thể');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Ch?n don', N'Chẩn đoán');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Chua c ch?n don c? th?', N'Chưa có chẩn đoán cụ thể');
                UPDATE MedicalRecords SET Diagnosis = REPLACE(Diagnosis, 'Nh?p vi?n', N'Nhập viện');

                UPDATE MedicalRecords SET TreatmentPlan = REPLACE(TreatmentPlan, 'Di?u tr?', N'Điều trị');
                UPDATE MedicalRecords SET TreatmentPlan = REPLACE(TreatmentPlan, 'Theo doi lm sng', N'Theo dõi lâm sàng');
                UPDATE MedicalRecords SET TreatmentPlan = REPLACE(TreatmentPlan, 'Theo doi', N'Theo dõi');
                UPDATE MedicalRecords SET TreatmentPlan = REPLACE(TreatmentPlan, 'K don thu?c', N'Kê đơn thuốc');
                UPDATE MedicalRecords SET TreatmentPlan = REPLACE(TreatmentPlan, 'nghi? ngoi', N'nghỉ ngơi');
                UPDATE MedicalRecords SET TreatmentPlan = REPLACE(TreatmentPlan, 'U?ng thu?c', N'Uống thuốc');
                UPDATE MedicalRecords SET TreatmentPlan = REPLACE(TreatmentPlan, 'Ch? d?nh xt nghi?m mu', N'Chỉ định xét nghiệm máu');
                UPDATE MedicalRecords SET TreatmentPlan = REPLACE(TreatmentPlan, 'Nh?p vi?n theo doi.', N'Nhập viện theo dõi.');
            END
        ");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Could not auto-fix table schemas: " + ex.Message);
    }

    try
    {
        var existingAdmin = db.Users.FirstOrDefault(u => u.Username == "admin");
        if (existingAdmin != null)
        {
            if (existingAdmin.Password != "admin123")
            {
                existingAdmin.Password = "admin123";
                db.Users.Update(existingAdmin);
                db.SaveChanges();
            }
        }
        else
        {
            db.Users.Add(new User
            {
                Username = "admin",
                Password = "admin123",
                Role = "Admin",
                FullName = "Administrator",
                Email = "admin@benhvien.com",
                SDT = "0901111222"
            });
            db.SaveChanges();
        }

        if (!db.Users.Any(u => u.Username == "doctor"))
        {
            db.Users.Add(new User
            {
                Username = "doctor",
                Password = "123",
                Role = "Doctor",
                FullName = "Bác sĩ",
                Email = "doctor@benhvien.com",
                SDT = "0987654321"
            });
            db.SaveChanges();
        }
    }
    catch (Exception)
    {
        // Users already exist in database (seeded via BenhVien.sql) — skip silently.
    }

    try
    {
        if (!db.QualityReviews.Any())
        {
            var reviews = new QualityReview[]
            {
                new QualityReview {
                    Department = "Nội khoa",
                    ReviewerName = "Nguyễn Văn An",
                    ReviewerPhone = "0912345678",
                    ServiceScore = 8,
                    CleanlinessScore = 9,
                    StaffScore = 9,
                    FacilityScore = 8,
                    WaitTimeScore = 7,
                    Comment = "Bác sĩ tư vấn rất nhiệt tình, chu đáo. Khoa phòng sạch sẽ, ngăn nắp. Thời gian chờ khám hơi lâu một chút.",
                    Status = "Chưa phản hồi",
                    OverallScore = 4,
                    CreatedAt = DateTime.Now.AddDays(-2)
                },
                new QualityReview {
                    Department = "Sản phụ khoa",
                    ReviewerName = "Lê Văn C",
                    ReviewerPhone = "0987654321",
                    ServiceScore = 10,
                    CleanlinessScore = 10,
                    StaffScore = 10,
                    FacilityScore = 10,
                    WaitTimeScore = 10,
                    Comment = "Dịch vụ tuyệt vời, phòng ốc hiện đại và bác sĩ rất tâm lý. Tôi rất hài lòng khi sinh em bé ở đây.",
                    Status = "Đã phản hồi",
                    ReplyComment = "Cảm ơn gia đình đã tin tưởng lựa chọn bệnh viện. Chúc bé và mẹ luôn khỏe mạnh!",
                    RepliedBy = "Administrator",
                    RepliedAt = DateTime.Now.AddDays(-1),
                    OverallScore = 5,
                    CreatedAt = DateTime.Now.AddDays(-3)
                },
                new QualityReview {
                    Department = "Cấp cứu",
                    ReviewerName = "Trần Thị Bích",
                    ReviewerPhone = "0905556677",
                    ServiceScore = 4,
                    CleanlinessScore = 5,
                    StaffScore = 4,
                    FacilityScore = 4,
                    WaitTimeScore = 3,
                    Comment = "Thái độ nhân viên trực cổng chưa tốt, cáu gắt với người nhà bệnh nhân. Thời gian tiếp nhận cấp cứu cần nhanh hơn.",
                    Status = "Chưa phản hồi",
                    OverallScore = 2,
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new QualityReview {
                    Department = "Khám bệnh",
                    ReviewerName = "Phạm Minh Hoàng",
                    ReviewerPhone = "0944332211",
                    ServiceScore = 8,
                    CleanlinessScore = 7,
                    StaffScore = 8,
                    FacilityScore = 7,
                    WaitTimeScore = 6,
                    Comment = "Bác sĩ khám nhanh, chẩn đoán rõ ràng. Tuy nhiên khu vực chờ khám hơi nóng vào buổi trưa.",
                    Status = "Chưa phản hồi",
                    OverallScore = 4,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                new QualityReview {
                    Department = "Ngoại khoa",
                    ReviewerName = "Đỗ Thị Dung",
                    ReviewerPhone = "0933221100",
                    ServiceScore = 9,
                    CleanlinessScore = 9,
                    StaffScore = 9,
                    FacilityScore = 8,
                    WaitTimeScore = 8,
                    Comment = "Các điều dưỡng viên chăm sóc rất chu đáo sau phẫu thuật. Rất biết ơn đội ngũ y bác sĩ ngoại khoa.",
                    Status = "Đã phản hồi",
                    ReplyComment = "Bệnh viện xin chân thành cảm ơn ý kiến đóng góp của chị. Chúc chị nhanh chóng bình phục hoàn toàn!",
                    RepliedBy = "Administrator",
                    RepliedAt = DateTime.Now.AddHours(-2),
                    OverallScore = 5,
                    CreatedAt = DateTime.Now.AddDays(-4)
                }
            };
            db.QualityReviews.AddRange(reviews);
            db.SaveChanges();

            foreach (var r in reviews)
            {
                db.ReviewThreads.Add(new ReviewThread {
                    QualityReviewId = r.Id,
                    SenderType = "Patient",
                    MessageContent = r.Comment ?? "",
                    CreatedAt = r.CreatedAt
                });

                if (r.Status == "Đã phản hồi")
                {
                    db.ReviewThreads.Add(new ReviewThread {
                        QualityReviewId = r.Id,
                        SenderType = "Admin",
                        MessageContent = r.ReplyComment ?? "",
                        CreatedAt = r.RepliedAt ?? r.CreatedAt.AddHours(2)
                    });
                }
            }
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Could not seed QualityReviews data: " + ex.Message);
    }

    try
    {
        if (!db.Departments.Any())
        {
            db.Departments.AddRange(new Department[] {
                new Department { DepartmentCode = "KKB", DepartmentName = "Khoa Khám bệnh", Description = "Khám bệnh ngoại trú", HeadDoctor = "Nguyễn Văn A", Phone = "0243123456" },
                new Department { DepartmentCode = "KNC", DepartmentName = "Khoa Nội tổng hợp", Description = "Điều trị nội trú nội khoa", HeadDoctor = "Lê Văn B", Phone = "0243123457" },
                new Department { DepartmentCode = "KNG", DepartmentName = "Khoa Ngoại tổng hợp", Description = "Phẫu thuật ngoại khoa", HeadDoctor = "Trần Thị C", Phone = "0243123458" },
                new Department { DepartmentCode = "KCC", DepartmentName = "Khoa Cấp cứu", Description = "Cấp cứu và hồi sức", HeadDoctor = "Phạm Văn D", Phone = "0243123459" },
                new Department { DepartmentCode = "KSN", DepartmentName = "Khoa Sản phụ khoa", Description = "Chăm sóc sức khỏe sinh sản", HeadDoctor = "Hoàng Thị E", Phone = "0243123460" }
            });
            db.SaveChanges();
        }

        if (!db.Feedbacks.Any())
        {
            var depIds = db.Departments.Select(d => new { d.Id, d.DepartmentName }).ToList();
            var depNoi = depIds.FirstOrDefault(d => d.DepartmentName.Contains("Nội"))?.Id ?? depIds.FirstOrDefault()?.Id ?? 1;
            var depSan = depIds.FirstOrDefault(d => d.DepartmentName.Contains("Sản"))?.Id ?? depIds.FirstOrDefault()?.Id ?? 1;
            var depCapCuu = depIds.FirstOrDefault(d => d.DepartmentName.Contains("Cấp cứu"))?.Id ?? depIds.FirstOrDefault()?.Id ?? 1;
            var depKhamBenh = depIds.FirstOrDefault(d => d.DepartmentName.Contains("Khám"))?.Id ?? depIds.FirstOrDefault()?.Id ?? 1;
            var depNgoai = depIds.FirstOrDefault(d => d.DepartmentName.Contains("Ngoại"))?.Id ?? depIds.FirstOrDefault()?.Id ?? 1;

            var f1 = new Feedback {
                DepartmentId = depNoi,
                RatingOverall = 4,
                ThaiDo = 4,
                VeSinh = 5,
                ChuyenMon = "Tốt",
                CSVC = "Tốt",
                ThoiGianCho = "30-60p",
                Content = "Bác sĩ tư vấn rất nhiệt tình, chu đáo. Khoa phòng sạch sẽ, ngăn nắp. Thời gian chờ khám hơi lâu một chút.",
                Status = "Pending",
                AllowContact = true,
                CreatedAt = DateTime.Now.AddDays(-2)
            };

            var f2 = new Feedback {
                DepartmentId = depSan,
                RatingOverall = 5,
                ThaiDo = 5,
                VeSinh = 5,
                ChuyenMon = "Xuất sắc",
                CSVC = "Tốt",
                ThoiGianCho = "< 15p",
                Content = "Dịch vụ tuyệt vời, phòng ốc hiện đại và bác sĩ rất tâm lý. Tôi rất hài lòng khi sinh em bé ở đây.",
                Status = "Responded",
                AllowContact = true,
                CreatedAt = DateTime.Now.AddDays(-3)
            };

            var f3 = new Feedback {
                DepartmentId = depCapCuu,
                RatingOverall = 2,
                ThaiDo = 2,
                VeSinh = 3,
                ChuyenMon = "Trung bình",
                CSVC = "Trung bình",
                ThoiGianCho = "> 2h",
                Content = "Thái độ nhân viên trực cổng chưa tốt, cáu gắt với người nhà bệnh nhân. Thời gian tiếp nhận cấp cứu cần nhanh hơn.",
                Status = "Pending",
                AllowContact = false,
                CreatedAt = DateTime.Now.AddDays(-5)
            };

            var f4 = new Feedback {
                DepartmentId = depKhamBenh,
                RatingOverall = 4,
                ThaiDo = 4,
                VeSinh = 4,
                ChuyenMon = "Tốt",
                CSVC = "Trung bình",
                ThoiGianCho = "30-60p",
                Content = "Bác sĩ khám nhanh, chẩn đoán rõ ràng. Tuy nhiên khu vực chờ khám hơi nóng vào buổi trưa.",
                Status = "Pending",
                AllowContact = true,
                CreatedAt = DateTime.Now.AddDays(-1)
            };

            var f5 = new Feedback {
                DepartmentId = depNgoai,
                RatingOverall = 5,
                ThaiDo = 5,
                VeSinh = 5,
                ChuyenMon = "Xuất sắc",
                CSVC = "Tốt",
                ThoiGianCho = "15-30p",
                Content = "Các điều dưỡng viên chăm sóc rất chu đáo sau phẫu thuật. Rất biết ơn đội ngũ y bác sĩ ngoại khoa.",
                Status = "Responded",
                AllowContact = true,
                CreatedAt = DateTime.Now.AddDays(-4)
            };

            db.Feedbacks.AddRange(new Feedback[] { f1, f2, f3, f4, f5 });
            db.SaveChanges();

            db.Responses.AddRange(new Response[] {
                new Response { FeedbackId = f1.Id, Sender = "User", Content = f1.Content, CreatedAt = f1.CreatedAt },
                
                new Response { FeedbackId = f2.Id, Sender = "User", Content = f2.Content, CreatedAt = f2.CreatedAt },
                new Response { FeedbackId = f2.Id, Sender = "Admin", Content = "Cảm ơn gia đình đã tin tưởng lựa chọn bệnh viện. Chúc bé và mẹ luôn khỏe mạnh!", CreatedAt = f2.CreatedAt.AddHours(2) },

                new Response { FeedbackId = f3.Id, Sender = "User", Content = f3.Content, CreatedAt = f3.CreatedAt },

                new Response { FeedbackId = f4.Id, Sender = "User", Content = f4.Content, CreatedAt = f4.CreatedAt },

                new Response { FeedbackId = f5.Id, Sender = "User", Content = f5.Content, CreatedAt = f5.CreatedAt },
                new Response { FeedbackId = f5.Id, Sender = "Admin", Content = "Bệnh viện xin chân thành cảm ơn ý kiến đóng góp của chị. Chúc chị nhanh chóng bình phục hoàn toàn!", CreatedAt = f5.CreatedAt.AddHours(2) }
            });
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Could not seed Feedbacks data: " + ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute( 
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public static class NotificationBackgroundWorker
{
    public static System.Collections.Generic.List<string> Logs { get; } = new System.Collections.Generic.List<string>
    {
        $"[{System.DateTime.Now:HH:mm:ss}] [SYSTEM] Khởi động hệ thống gửi thông báo thông minh thành công.",
        $"[{System.DateTime.Now:HH:mm:ss}] [SYSTEM] Trình quét nền liên tục đang chạy..."
    };

    public static void AddLog(string log)
    {
        lock (Logs)
        {
            Logs.Add($"[{System.DateTime.Now:HH:mm:ss}] {log}");
            if (Logs.Count > 50)
            {
                Logs.RemoveAt(2); // Keep the first two logs
            }
        }
    }
}

public class ReexaminationNotificationService : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly System.IServiceProvider _serviceProvider;

    public ReexaminationNotificationService(System.IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(System.Threading.CancellationToken stoppingToken)
    {
        NotificationBackgroundWorker.AddLog("[SYSTEM] Bắt đầu quét định kỳ lịch hẹn tái khám.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<HeThongBenhVien.Data.ApplicationDbContext>();
                    
                    var tomorrow = System.DateTime.Today.AddDays(1);
                    var appointments = await db.Appointments
                        .Include(a => a.Patient)
                        .Where(a => a.Status == 8 && a.Patient != null && a.AppointmentTime.Date == tomorrow)
                        .ToListAsync(stoppingToken);

                    NotificationBackgroundWorker.AddLog($"[SYSTEM] Tiến hành quét. Tìm thấy {appointments.Count} lịch hẹn tái khám vào ngày mai ({tomorrow:dd/MM/yyyy}).");

                    int sentCount = 0;
                    foreach (var appt in appointments)
                    {
                        bool alreadySent = await db.Notifications
                            .AnyAsync(n => n.PatientId == appt.PatientId && 
                                           n.DoctorId == appt.DoctorId && 
                                           n.IsForPatient && 
                                           n.Message.Contains("nhắc nhở tái khám"), stoppingToken);

                        if (!alreadySent)
                        {
                            string message = $"[TỰ ĐỘNG NHẮC HẸN] Xin chào {appt.Patient.FullName}, bạn có lịch hẹn tái khám với bác sĩ vào ngày {appt.AppointmentTime:dd/MM/yyyy} lúc {appt.AppointmentTime:HH:mm}. Vui lòng đến đúng giờ.";
                            
                            var notification = new Notification
                            {
                                PatientId = appt.PatientId,
                                DoctorId = appt.DoctorId ?? 1,
                                Message = message,
                                CreatedAt = System.DateTime.Now,
                                IsRead = false,
                                IsForPatient = true
                            };
                            db.Notifications.Add(notification);
                            sentCount++;
                            NotificationBackgroundWorker.AddLog($"[AUTO-SMS] Đã tự động gửi thông báo cho bệnh nhân {appt.Patient.FullName} (Mã: {appt.Patient.PatientCode}).");
                        }
                    }
                    if (sentCount > 0)
                    {
                        await db.SaveChangesAsync(stoppingToken);
                        NotificationBackgroundWorker.AddLog($"[SYSTEM] Đã lưu {sentCount} thông báo nhắc hẹn mới vào cơ sở dữ liệu.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                NotificationBackgroundWorker.AddLog($"[ERROR] Lỗi khi quét lịch hẹn: {ex.Message}");
            }

            // Sleep 15 seconds to simulate fast updates
            await System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}     
