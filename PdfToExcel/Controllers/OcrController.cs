using Microsoft.AspNetCore.Mvc;
using PdfToExcel.Services;

namespace PdfToExcel.Controllers
{
    [ApiController]
    [Route("api/ocr")]
    public class OcrController : ControllerBase
    {
        private readonly PdfToImageService _pdf;
        private readonly OcrService _ocr;
        private readonly ExcelService _excel;

        public OcrController(
            PdfToImageService pdf,
            OcrService ocr,
            ExcelService excel)
        {
            _pdf = pdf;
            _ocr = ocr;
            _excel = excel;
        }

        [HttpPost("convert")]
        public IActionResult Convert(IFormFile file)
        {
            if (file == null)
                return BadRequest("No file");

            // save PDF
            var pdfPath = Path.Combine("wwwroot/temp", Guid.NewGuid() + ".pdf");
            Directory.CreateDirectory("wwwroot/temp");

            using (var stream = new FileStream(pdfPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // PDF → images
            var images = _pdf.Convert(pdfPath);

            var fullText = "";

            // OCR
            foreach (var img in images)
            {
                fullText += _ocr.ExtractText(img) + "\n";
            }

            // SIMPLE table parsing (basic version)
            var table = fullText
                .Split('\n')
                .Select(line =>
                    line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .ToList())
                .Where(x => x.Count > 0)
                .ToList();

            // Excel
            var excelFile = _excel.CreateExcel(table);

            var fullPath = Path.Combine("wwwroot/files", excelFile);

            var bytes = System.IO.File.ReadAllBytes(fullPath);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                excelFile
            );
        }
    }
}