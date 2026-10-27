using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddRezervasyonIdToEmlakBlokeTarih : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RezervasyonId",
                table: "EmlakBlokeTarihler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmlakBlokeTarihler_RezervasyonId",
                table: "EmlakBlokeTarihler",
                column: "RezervasyonId",
                unique: true,
                filter: "[RezervasyonId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_EmlakBlokeTarihler_Rezervasyonlar_RezervasyonId",
                table: "EmlakBlokeTarihler",
                column: "RezervasyonId",
                principalTable: "Rezervasyonlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmlakBlokeTarihler_Rezervasyonlar_RezervasyonId",
                table: "EmlakBlokeTarihler");

            migrationBuilder.DropIndex(
                name: "IX_EmlakBlokeTarihler_RezervasyonId",
                table: "EmlakBlokeTarihler");

            migrationBuilder.DropColumn(
                name: "RezervasyonId",
                table: "EmlakBlokeTarihler");
        }
    }
}
