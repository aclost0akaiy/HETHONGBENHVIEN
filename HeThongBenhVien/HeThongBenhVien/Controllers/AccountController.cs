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

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    if (user.Role == "Admin")
                        return RedirectToAction("Dashboard", "Admin");
                    else if (user.Role == "Doctor")
                        return RedirectToAction("Dashboard", "Doctor");
                    
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
