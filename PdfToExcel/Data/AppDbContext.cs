using Microsoft.EntityFrameworkCore;
using PdfToExcel.Models;

namespace PdfToExcel.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ForexCurrency> ForexCurrencies { get; set; }
        public DbSet<OfficialCurrency> OfficialCurrencies { get; set; }
        public DbSet<ConvertedFile> ConvertedFiles { get; set; } = null!;
    }
}
