using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddContentFaqAndBankAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankaHesaplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankaAdi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HesapSahibi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Iban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    SubeAdi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ParaBirimi = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankaHesaplari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SayfaIcerikleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IcerikHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SayfaIcerikleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SikcaSorulanSorular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Soru = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CevapHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SikcaSorulanSorular", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankaHesaplari_Sira",
                table: "BankaHesaplari",
                column: "Sira");

            migrationBuilder.CreateIndex(
                name: "IX_SayfaIcerikleri_Slug",
                table: "SayfaIcerikleri",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SikcaSorulanSorular_Sira",
                table: "SikcaSorulanSorular",
                column: "Sira");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankaHesaplari");

            migrationBuilder.DropTable(
                name: "SayfaIcerikleri");

            migrationBuilder.DropTable(
                name: "SikcaSorulanSorular");
        }
    }
}
