using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddEmlakYayinOtoparkBelge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BelgeNo",
                table: "Emlaklar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OtoparkSayisi",
                table: "Emlaklar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Yayinda",
                table: "Emlaklar",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BelgeNo",
                table: "Emlaklar");

            migrationBuilder.DropColumn(
                name: "OtoparkSayisi",
                table: "Emlaklar");

            migrationBuilder.DropColumn(
                name: "Yayinda",
                table: "Emlaklar");
        }
    }
}
