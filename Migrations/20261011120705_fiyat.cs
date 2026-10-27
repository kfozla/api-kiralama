using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class fiyat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmlakBlokeTarihler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    BaslangicTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    BitisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakBlokeTarihler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmlakBlokeTarihler_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmlakFiyatlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    BaslangicTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    BitisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    GecelikFiyat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakFiyatlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmlakFiyatlari_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rezervasyonlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    MusteriAd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MusteriSoyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GirisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    CikisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    KisiSayisi = table.Column<int>(type: "int", nullable: false),
                    ToplamFiyat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezervasyonlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rezervasyonlar_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmlakBlokeTarihler_EmlakId",
                table: "EmlakBlokeTarihler",
                column: "EmlakId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakFiyatlari_EmlakId",
                table: "EmlakFiyatlari",
                column: "EmlakId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_EmlakId",
                table: "Rezervasyonlar",
                column: "EmlakId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmlakBlokeTarihler");

            migrationBuilder.DropTable(
                name: "EmlakFiyatlari");

            migrationBuilder.DropTable(
                name: "Rezervasyonlar");
        }
    }
}
