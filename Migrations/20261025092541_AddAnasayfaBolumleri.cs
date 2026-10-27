using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddAnasayfaBolumleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnasayfaBolumler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    GosterimTipi = table.Column<int>(type: "int", nullable: false),
                    SecimTipi = table.Column<int>(type: "int", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    MaksimumVillaSayisi = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnasayfaBolumler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnasayfaBolumEmlaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnasayfaBolumId = table.Column<int>(type: "int", nullable: false),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnasayfaBolumEmlaklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnasayfaBolumEmlaklar_AnasayfaBolumler_AnasayfaBolumId",
                        column: x => x.AnasayfaBolumId,
                        principalTable: "AnasayfaBolumler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnasayfaBolumEmlaklar_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnasayfaBolumEmlaklar_AnasayfaBolumId_EmlakId",
                table: "AnasayfaBolumEmlaklar",
                columns: new[] { "AnasayfaBolumId", "EmlakId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnasayfaBolumEmlaklar_EmlakId",
                table: "AnasayfaBolumEmlaklar",
                column: "EmlakId");

            migrationBuilder.CreateIndex(
                name: "IX_AnasayfaBolumler_Slug",
                table: "AnasayfaBolumler",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnasayfaBolumEmlaklar");

            migrationBuilder.DropTable(
                name: "AnasayfaBolumler");
        }
    }
}
