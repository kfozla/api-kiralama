using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddIsletmeAyarlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IsletmeAyarlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsletmeAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    YetkiliKisi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebSitesi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TursabUyeligiVar = table.Column<bool>(type: "bit", nullable: false),
                    TursabBelgeNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TursabBelgeUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsletmeAyarlari", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IsletmeAyarlari");
        }
    }
}
