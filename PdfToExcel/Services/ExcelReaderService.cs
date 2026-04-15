using ClosedXML.Excel;
using PdfToExcel.Models;

public class ExcelReaderService
{
    // ===================== VALIDATE FOREX =====================
    public (bool isValid, string error) ValidateForex(string path)
    {
        try
        {
            using var workbook = new XLWorkbook(path);
            var ws = workbook.Worksheet(1);

            var headers = ws.Row(1).Cells(1, 4)
                .Select(x => x.GetString().ToLower())
                .ToList();

            if (headers.Count < 4 ||
                headers[0] != "code" ||
                headers[1] != "country" ||
                headers[2] != "buy" ||
                headers[3] != "sell")
            {
                return (false, "Invalid Excel format. Required: Code, Country, Buy, Sell");
            }

            if (ws.RowsUsed().Count() <= 1)
                return (false, "Excel file is empty");

            return (true, "");
        }
        catch
        {
            return (false, "Invalid or corrupted Excel file");
        }
    }

    // ===================== VALIDATE OFFICIAL =====================
    public (bool isValid, string error) ValidateOfficial(string path)
    {
        try
        {
            using var workbook = new XLWorkbook(path);
            var ws = workbook.Worksheet(1);

            var headers = ws.Row(1).Cells(1, 4)
                .Select(x => x.GetString().ToLower())
                .ToList();

            if (headers.Count < 4 ||
                headers[0] != "code" ||
                headers[1] != "country" ||
                headers[2] != "buy" ||
                headers[3] != "sell")
            {
                return (false, "Invalid Excel format. Required: Code, Country, Buy, Sell");
            }

            if (ws.RowsUsed().Count() <= 1)
                return (false, "Excel file is empty");

            return (true, "");
        }
        catch
        {
            return (false, "Invalid or corrupted Excel file");
        }
    }

    // ===================== READ FOREX =====================
    public List<ForexCurrency> ReadForex(string path)
    {
        var list = new List<ForexCurrency>();

        using var workbook = new XLWorkbook(path);
        var ws = workbook.Worksheet(1);

        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var buy = row.Cell(3).GetDouble();
            var sell = row.Cell(4).GetDouble();

            list.Add(new ForexCurrency
            {
                Code = row.Cell(1).GetString(),
                Country = row.Cell(2).GetString(),
                Buy = buy,
                Sell = sell,
                Flag = GetFlag(row.Cell(1).GetString())
                // ✅ Average auto computed
            });
        }

        return list;
    }

    // ===================== READ OFFICIAL =====================
    public List<OfficialCurrency> ReadOfficial(string path)
    {
        var list = new List<OfficialCurrency>();

        using var workbook = new XLWorkbook(path);
        var ws = workbook.Worksheet(1);

        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var buy = row.Cell(3).GetDouble();
            var sell = row.Cell(4).GetDouble();

            list.Add(new OfficialCurrency
            {
                Code = row.Cell(1).GetString(),
                Country = row.Cell(2).GetString(),
                Buy = buy,
                Sell = sell,
                Flag = GetFlag(row.Cell(1).GetString())
                // ✅ Average + Margin auto computed
            });
        }

        return list;
    }

    // ===================== FLAG =====================
    private string GetFlag(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
            return "";

        return $"https://flagcdn.com/w40/{code.ToLower().Substring(0, 2)}.png";
    }
}