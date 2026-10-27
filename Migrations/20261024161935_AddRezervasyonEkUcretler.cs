using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddRezervasyonEkUcretler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HasarDepozitosu",
                table: "Rezervasyonlar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TemizlikUcreti",
                table: "Rezervasyonlar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasarDepozitosu",
                table: "Rezervasyonlar");

            migrationBuilder.DropColumn(
                name: "TemizlikUcreti",
                table: "Rezervasyonlar");
        }
    }
}
