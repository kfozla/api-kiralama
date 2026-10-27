using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddEmlakAccommodationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "CikisSaati",
                table: "Emlaklar",
                type: "time(0)",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "GirisSaati",
                table: "Emlaklar",
                type: "time(0)",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<decimal>(
                name: "HasarDepozitosu",
                table: "Emlaklar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinimumKonaklamaSuresi",
                table: "Emlaklar",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "TemizlikUcreti",
                table: "Emlaklar",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CikisSaati",
                table: "Emlaklar");

            migrationBuilder.DropColumn(
                name: "GirisSaati",
                table: "Emlaklar");

            migrationBuilder.DropColumn(
                name: "HasarDepozitosu",
                table: "Emlaklar");

            migrationBuilder.DropColumn(
                name: "MinimumKonaklamaSuresi",
                table: "Emlaklar");

            migrationBuilder.DropColumn(
                name: "TemizlikUcreti",
                table: "Emlaklar");
        }
    }
}
