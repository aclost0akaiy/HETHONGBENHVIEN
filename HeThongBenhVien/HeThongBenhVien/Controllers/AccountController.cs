using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeThongBenhVien.Data;
using HeThongBenhVien.Models;
using System.Security.Claims;

namespace HeThongBenhVien.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
                if (User.IsInRole("Doctor")) return RedirectToAction("Dashboard", "Doctor");
                if (User.IsInRole("BenhNhan") || User.IsInRole("Patient")) return RedirectToAction("Portal", "Patient");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var username = model.Username?.Trim() ?? string.Empty;
                var password = model.Password?.Trim() ?? string.Empty;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

                if (user != null)
                {
                    if (user.Role == "BenhNhan" || user.Role == "Patient")
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản Bệnh nhân không được phép đăng nhập tại cổng Nhân viên y tế. Vui lòng đăng nhập tại Cổng Bệnh Nhân.");
                        return View(model);
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("FullName", user.FullName ?? "")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    TempData["NewLogin"] = "true";
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    if (user.Role == "Admin")
                        return RedirectToAction("Dashboard", "Admin");
                    else if (user.Role == "Doctor")
                    {
                        if (user.Username == "bs_kedon")
                        {
                            return RedirectToAction("KeDonDashboard", "Doctor");
                        }
                        return RedirectToAction("Dashboard", "Doctor");
                    }
                    else if (user.Role == "BenhNhan" || user.Role == "Patient")
                        return RedirectToAction("Portal", "Patient");
                    
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult PatientLogin(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("BenhNhan") || User.IsInRole("Patient"))
                {
                    return RedirectToAction("Portal", "Patient");
                }
                else
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PatientLogin(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var username = model.Username?.Trim() ?? string.Empty;
                var password = model.Password?.Trim() ?? string.Empty;

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

                if (user != null)
                {
                    if (user.Role != "BenhNhan" && user.Role != "Patient")
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản Nhân viên y tế không được phép đăng nhập tại Cổng Bệnh Nhân. Vui lòng đăng nhập tại cổng Nhân viên y tế.");
                        return View(model);
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("FullName", user.FullName ?? "")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    TempData["NewLogin"] = "true";
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Portal", "Patient");
                }

                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterPatient()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPatient(RegisterPatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.AnyAsync(u => u.Username == model.Username.Trim());
                if (existingUser)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                string patientCode = "BN" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                while (await _context.Patients.AnyAsync(p => p.PatientCode == patientCode))
                {
                    patientCode = "BN" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                }

                var patient = new Patient
                {
                    FullName = model.FullName.Trim(),
                    Gender = model.Gender,
                    Age = model.Age,
                    PatientCode = patientCode
                };

                var user = new User
                {
                    Username = model.Username.Trim(),
                    Password = model.Password.Trim(),
                    FullName = model.FullName.Trim(),
                    Role = "BenhNhan",
                    SDT = model.SDT.Trim(),
                    Email = model.Email?.Trim(),
                    PatientCode = patientCode
                };

                _context.Patients.Add(patient);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("FullName", user.FullName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["NewLogin"] = "true";
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Portal", "Patient");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
