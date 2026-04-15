using Microsoft.AspNetCore.Mvc;
using PdfToExcel.Data;
using PdfToExcel.Models;

[ApiController]
[Route("api/currency")]
public class CurrencyApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public CurrencyApiController(AppDbContext context)
    {
        _context = context;
    }

    // ================= FOREX =================
    [HttpGet("forex")]
    public IActionResult GetForex()
    {
        var data = _context.ForexCurrencies
            .Select(x => new
            {
                id = x.Code,
                code = x.Code,
                country = x.Country,
                buy = x.Buy,
                sell = x.Sell,
                average = (x.Buy + x.Sell) / 2,
                flag = x.Flag
            })
            .ToList();

        return Ok(data);
    }

    // ================= OFFICIAL =================
    [HttpGet("official")]
    public IActionResult GetOfficial()
    {
        var data = _context.OfficialCurrencies
            .Select(x => new OfficialCurrency
            {
                Code = x.Code,
                Country = x.Country,
                Buy = x.Buy,
                Sell = x.Sell,
                Flag = x.Flag
                // Average + Margin auto computed ✅
            })
            .ToList();

        return Ok(data);
    }

}