using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PdfToExcel.Migrations
{
    /// <inheritdoc />
    public partial class Fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Average",
                table: "OfficialCurrencies");

            migrationBuilder.DropColumn(
                name: "Margin",
                table: "OfficialCurrencies");

            migrationBuilder.DropColumn(
                name: "Average",
                table: "ForexCurrencies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Average",
                table: "OfficialCurrencies",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Margin",
                table: "OfficialCurrencies",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Average",
                table: "ForexCurrencies",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
