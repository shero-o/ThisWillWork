namespace PdfToExcel.Models
{
    public class ForexCurrency
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Country { get; set; }
        public double Buy { get; set; }
        public double Sell { get; set; }
        public double Average => (Buy + Sell) / 2;
        public string Flag { get; set; }

    }
}
