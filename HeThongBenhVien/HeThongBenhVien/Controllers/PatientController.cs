using Microsoft.AspNetCore.Mvc;

namespace HeThongBenhVien.Controllers
{
    public class PatientController : Controller
    {
        public IActionResult Portal()
        {
            return View();
        }

        public IActionResult Booking()
        {
            return View();
        }

        public IActionResult EHR()
        {
            return View();
        }

        public IActionResult Payment()
        {
            return View();
        }

        public IActionResult Notifications()
        {
            return View();
        }

        public IActionResult Rating()
        {
            return View();
        }
    }
}
