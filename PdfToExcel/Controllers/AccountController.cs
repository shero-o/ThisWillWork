using Microsoft.AspNetCore.Mvc;

namespace PdfToExcel.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "1234")
            {
                
                HttpContext.Session.SetString("user", "admin");
                return RedirectToAction("Index", "Files");
            }

            ViewBag.Error = "Invalid credentials";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
