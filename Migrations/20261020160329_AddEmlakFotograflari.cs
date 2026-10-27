using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddEmlakFotograflari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmlakFotograflari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    DosyaYolu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    KapakMi = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakFotograflari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmlakFotograflari_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmlakFotograflari_EmlakId",
                table: "EmlakFotograflari",
                column: "EmlakId",
                unique: true,
                filter: "[KapakMi] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakFotograflari_EmlakId_Sira",
                table: "EmlakFotograflari",
                columns: new[] { "EmlakId", "Sira" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmlakFotograflari");
        }
    }
}
