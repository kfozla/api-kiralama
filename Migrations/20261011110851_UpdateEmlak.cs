using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmlak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bolgeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentBolgeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bolgeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bolgeler_Bolgeler_ParentBolgeId",
                        column: x => x.ParentBolgeId,
                        principalTable: "Bolgeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmlakOzellikleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OzellikAdi = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Kategori = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakOzellikleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmlakSahipleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakSahipleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emlaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmlakSahibiId = table.Column<int>(type: "int", nullable: false),
                    EmlakTipi = table.Column<int>(type: "int", nullable: false),
                    BolgeId = table.Column<int>(type: "int", nullable: false),
                    EmlakAdı = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Açıklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kapasite = table.Column<int>(type: "int", nullable: false),
                    YatakOdasıSayısı = table.Column<int>(type: "int", nullable: false),
                    BanyoSayısı = table.Column<int>(type: "int", nullable: false),
                    GenelBilgiler = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    EmlakSahibiId1 = table.Column<int>(type: "int", nullable: true),
                    HavuzBilgileri = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emlaklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emlaklar_Bolgeler_BolgeId",
                        column: x => x.BolgeId,
                        principalTable: "Bolgeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Emlaklar_EmlakSahipleri_EmlakSahibiId",
                        column: x => x.EmlakSahibiId,
                        principalTable: "EmlakSahipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Emlaklar_EmlakSahipleri_EmlakSahibiId1",
                        column: x => x.EmlakSahibiId1,
                        principalTable: "EmlakSahipleri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmlakOneCikanOzellikIliskileri",
                columns: table => new
                {
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    OneCıkanOzelliklerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakOneCikanOzellikIliskileri", x => new { x.EmlakId, x.OneCıkanOzelliklerId });
                    table.ForeignKey(
                        name: "FK_EmlakOneCikanOzellikIliskileri_EmlakOzellikleri_OneCıkanOzelliklerId",
                        column: x => x.OneCıkanOzelliklerId,
                        principalTable: "EmlakOzellikleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmlakOneCikanOzellikIliskileri_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmlakOzellikIliskileri",
                columns: table => new
                {
                    EmlakOzellikleriId = table.Column<int>(type: "int", nullable: false),
                    EmlaklarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakOzellikIliskileri", x => new { x.EmlakOzellikleriId, x.EmlaklarId });
                    table.ForeignKey(
                        name: "FK_EmlakOzellikIliskileri_EmlakOzellikleri_EmlakOzellikleriId",
                        column: x => x.EmlakOzellikleriId,
                        principalTable: "EmlakOzellikleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmlakOzellikIliskileri_Emlaklar_EmlaklarId",
                        column: x => x.EmlaklarId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bolgeler_ParentBolgeId",
                table: "Bolgeler",
                column: "ParentBolgeId");

            migrationBuilder.CreateIndex(
                name: "IX_Emlaklar_BolgeId",
                table: "Emlaklar",
                column: "BolgeId");

            migrationBuilder.CreateIndex(
                name: "IX_Emlaklar_EmlakSahibiId",
                table: "Emlaklar",
                column: "EmlakSahibiId");

            migrationBuilder.CreateIndex(
                name: "IX_Emlaklar_EmlakSahibiId1",
                table: "Emlaklar",
                column: "EmlakSahibiId1");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakOneCikanOzellikIliskileri_OneCıkanOzelliklerId",
                table: "EmlakOneCikanOzellikIliskileri",
                column: "OneCıkanOzelliklerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakOzellikIliskileri_EmlaklarId",
                table: "EmlakOzellikIliskileri",
                column: "EmlaklarId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakOzellikleri_OzellikAdi_Kategori",
                table: "EmlakOzellikleri",
                columns: new[] { "OzellikAdi", "Kategori" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmlakOneCikanOzellikIliskileri");

            migrationBuilder.DropTable(
                name: "EmlakOzellikIliskileri");

            migrationBuilder.DropTable(
                name: "EmlakOzellikleri");

            migrationBuilder.DropTable(
                name: "Emlaklar");

            migrationBuilder.DropTable(
                name: "Bolgeler");

            migrationBuilder.DropTable(
                name: "EmlakSahipleri");
        }
    }
}
