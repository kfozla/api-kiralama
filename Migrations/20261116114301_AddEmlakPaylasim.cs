using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddEmlakPaylasim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmlakPaylasimlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TokenHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Baslik = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GirisTarihi = table.Column<DateOnly>(type: "date", nullable: true),
                    CikisTarihi = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsRevoked = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakPaylasimlar", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmlakPaylasimItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmlakPaylasimId = table.Column<int>(type: "int", nullable: false),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakPaylasimItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmlakPaylasimItems_EmlakPaylasimlar_EmlakPaylasimId",
                        column: x => x.EmlakPaylasimId,
                        principalTable: "EmlakPaylasimlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmlakPaylasimItems_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakPaylasimItems_EmlakId",
                table: "EmlakPaylasimItems",
                column: "EmlakId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakPaylasimItems_EmlakPaylasimId_EmlakId",
                table: "EmlakPaylasimItems",
                columns: new[] { "EmlakPaylasimId", "EmlakId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmlakPaylasimlar_TokenHash",
                table: "EmlakPaylasimlar",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmlakPaylasimItems");

            migrationBuilder.DropTable(
                name: "EmlakPaylasimlar");
        }
    }
}
