using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PdfToExcel.Models;
using PdfToExcel.Data;

namespace PdfToExcel.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("user") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var files = _context.ConvertedFiles
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return View(files);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}