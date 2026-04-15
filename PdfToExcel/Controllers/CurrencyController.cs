using Microsoft.AspNetCore.Mvc;
using PdfToExcel.Models;
using PdfToExcel.Data;

public class CurrencyController : Controller
{
    private readonly ExcelReaderService _excel;
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _context;

    public CurrencyController(
        ExcelReaderService excel,
        IWebHostEnvironment env,
        AppDbContext context)
    {
        _excel = excel;
        _env = env;
        _context = context;
    }

    // ===================== UPLOAD =====================
    [HttpPost]
    public async Task<IActionResult> UploadExcel(IFormFile file, string type)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file.";
            return RedirectToAction(type == "forex" ? "Forex" : "Official");
        }

        var folder = Path.Combine(_env.WebRootPath, "files");
        Directory.CreateDirectory(folder);

        var fileName = type == "forex" ? "forex.xlsx" : "official.xlsx";
        var path = Path.Combine(folder, fileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 🔥 VALIDATE
        var result = type == "forex"
            ? _excel.ValidateForex(path)
            : _excel.ValidateOfficial(path);

        if (!result.isValid)
        {
            System.IO.File.Delete(path);
            TempData["Error"] = result.error;
            return RedirectToAction(type == "forex" ? "Forex" : "Official");
        }

        TempData["Success"] = "File uploaded and validated ✅";
        TempData["ValidFile"] = "true";

        return RedirectToAction(type == "forex" ? "Forex" : "Official");
    }

    // ===================== PUBLISH FOREX =====================
    [HttpPost]
    public IActionResult PublishForex()
    {
        var path = Path.Combine(_env.WebRootPath, "files", "forex.xlsx");

        if (!System.IO.File.Exists(path))
            return Content("Forex file not found");

        var data = _excel.ReadForex(path);

        _context.ForexCurrencies.RemoveRange(_context.ForexCurrencies);

        var entities = data.Select(x => new ForexCurrency
        {
            Code = x.Code,
            Country = x.Country,
            Buy = x.Buy,
            Sell = x.Sell,
            Flag = x.Flag
        }).ToList();

        _context.ForexCurrencies.AddRange(entities);
        _context.SaveChanges();

        TempData["Success"] = "Forex data published 🚀";

        return RedirectToAction("Forex");
    }

    // ===================== PUBLISH OFFICIAL =====================
    [HttpPost]
    public IActionResult PublishOfficial()
    {
        var path = Path.Combine(_env.WebRootPath, "files", "official.xlsx");

        if (!System.IO.File.Exists(path))
            return Content("Official file not found");

        var data = _excel.ReadOfficial(path);

        _context.OfficialCurrencies.RemoveRange(_context.OfficialCurrencies);

        var entities = data.Select(x => new OfficialCurrency
        {
            Code = x.Code,
            Country = x.Country,
            Buy = x.Buy,
            Sell = x.Sell,
            Flag = x.Flag
        }).ToList();

        _context.OfficialCurrencies.AddRange(entities);
        _context.SaveChanges();

        TempData["Success"] = "Official data published 🚀";

        return RedirectToAction("Official");
    }

    // ===================== VIEW FOREX =====================
    public IActionResult Forex()
    {
        var data = _context.ForexCurrencies
            .Select(x => new ForexCurrency
            {
                Code = x.Code,
                Country = x.Country,
                Buy = x.Buy,
                Sell = x.Sell,
                Flag = x.Flag
                // Average auto computed ✅
            })
            .ToList();

        return View(data);
    }

    // ===================== VIEW OFFICIAL =====================
    public IActionResult Official()
    {
        var data = _context.OfficialCurrencies
            .Select(x => new OfficialCurrency
            {
                Code = x.Code,
                Country = x.Country,
                Buy = x.Buy,
                Sell = x.Sell,
                Flag = x.Flag
                // Average & Margin auto computed ✅
            })
            .ToList();

        return View(data);
    }
}