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

        public DbSet<ConvertedFile> ConvertedFiles { get; set; } = null!;
    }
}
