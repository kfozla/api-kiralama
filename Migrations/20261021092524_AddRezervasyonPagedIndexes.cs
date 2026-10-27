using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddRezervasyonPagedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_CreatedAt_Id",
                table: "Rezervasyonlar",
                columns: new[] { "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_Durum_GirisTarihi",
                table: "Rezervasyonlar",
                columns: new[] { "Durum", "GirisTarihi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rezervasyonlar_CreatedAt_Id",
                table: "Rezervasyonlar");

            migrationBuilder.DropIndex(
                name: "IX_Rezervasyonlar_Durum_GirisTarihi",
                table: "Rezervasyonlar");
        }
    }
}
