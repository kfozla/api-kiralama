using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddMuhasebeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "KomisyonTutari",
                table: "Rezervasyonlar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OnOdemeTutari",
                table: "Rezervasyonlar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OnOdemeOrani",
                table: "IsletmeAyarlari",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "KomisyonOrani",
                table: "Emlaklar",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KomisyonTutari",
                table: "Rezervasyonlar");

            migrationBuilder.DropColumn(
                name: "OnOdemeTutari",
                table: "Rezervasyonlar");

            migrationBuilder.DropColumn(
                name: "OnOdemeOrani",
                table: "IsletmeAyarlari");

            migrationBuilder.DropColumn(
                name: "KomisyonOrani",
                table: "Emlaklar");
        }
    }
}
