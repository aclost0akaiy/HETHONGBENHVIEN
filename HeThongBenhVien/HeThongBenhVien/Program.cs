using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        ");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Could not auto-fix Users table schema: " + ex.Message);
    }

    try
    {
        if (!db.Users.Any(u => u.Username == "admin"))
        {
            db.Users.Add(new User
            {
                Username = "admin",
                Password = "123",
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
