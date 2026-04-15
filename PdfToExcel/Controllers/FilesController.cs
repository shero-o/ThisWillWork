using Microsoft.AspNetCore.Mvc;
using PdfToExcel.Data;
using PdfToExcel.Models;
using PdfToExcel.Services;

namespace PdfToExcel.Controllers
{
    public class FilesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly PdfToImageService _pdfToImage;
        private readonly OcrService _ocr;
        private readonly ExcelService _excel;

        public FilesController(
            AppDbContext context,
            IWebHostEnvironment env,
            PdfToImageService pdfToImage,
            OcrService ocr,
            ExcelService excel)
        {
            _context = context;
            _env = env;
            _pdfToImage = pdfToImage;
            _ocr = ocr;
            _excel = excel;
        }

        public IActionResult Index(int page = 1)
        {
            if (HttpContext.Session.GetString("user") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            const int pageSize = 4;

            var filesQuery = _context.ConvertedFiles
                .OrderByDescending(x => x.CreatedAt);

            var totalFiles = filesQuery.Count();
            var pagedFiles = filesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = Math.Ceiling(totalFiles / (double)pageSize);
            ViewBag.TotalFiles = totalFiles;

            return View(pagedFiles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please choose a PDF file.";
                return RedirectToAction("Index");
            }

            if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Only PDF files are allowed.";
                return RedirectToAction("Index");
            }

            var tempRoot = Path.Combine(_env.WebRootPath, "temp");
            Directory.CreateDirectory(tempRoot);

            var tempPdfPath = Path.Combine(tempRoot, $"{Guid.NewGuid()}.pdf");
            var generatedImages = new List<string>();

            try
            {
                await using (var stream = new FileStream(tempPdfPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                generatedImages = _pdfToImage.Convert(tempPdfPath);

                var extractedLines = new List<List<string>>();

                foreach (var imagePath in generatedImages)
                {
                    var pageText = _ocr.ExtractText(imagePath);
                    var rows = pageText
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .ToList())
                        .Where(tokens => tokens.Count > 0)
                        .ToList();

                    extractedLines.AddRange(rows);
                }

                if (!extractedLines.Any())
                {
                    TempData["Error"] = "OCR completed, but no readable text was found in this PDF.";
                    return RedirectToAction("Index");
                }

                var excelFileName = _excel.CreateExcel(extractedLines);

                var originalBaseName = Path.GetFileNameWithoutExtension(file.FileName);
                var safeName = string.Concat(originalBaseName.Split(Path.GetInvalidFileNameChars()));
                if (string.IsNullOrWhiteSpace(safeName))
                {
                    safeName = "converted-file";
                }

                var downloadName = $"{safeName}.xlsx";

                var entity = new ConvertedFile
                {
                    Id = Guid.NewGuid(),
                    FileName = downloadName,
                    FilePath = excelFileName,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ConvertedFiles.Add(entity);
                await _context.SaveChangesAsync();

                TempData["Success"] = "File converted with OCR and saved successfully.";
            }
            catch
            {
                TempData["Error"] = "Something went wrong while processing your PDF.";
            }
            finally
            {
                if (System.IO.File.Exists(tempPdfPath))
                {
                    System.IO.File.Delete(tempPdfPath);
                }

                foreach (var imagePath in generatedImages)
                {
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Download(Guid id)
        {
            var file = _context.ConvertedFiles.FirstOrDefault(x => x.Id == id);
            if (file == null) return NotFound();

            var path = Path.Combine(_env.WebRootPath, "files", file.FilePath);

            if (!System.IO.File.Exists(path))
            {
                return NotFound("Converted file not found.");
            }

            return PhysicalFile(path,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                file.FileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            var file = _context.ConvertedFiles.FirstOrDefault(x => x.Id == id);
            if (file == null) return RedirectToAction("Index");

            var path = Path.Combine(_env.WebRootPath, "files", file.FilePath);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            _context.ConvertedFiles.Remove(file);
            _context.SaveChanges();

            TempData["Success"] = "Converted file deleted.";
            return RedirectToAction("Index");
        }
    }
}
