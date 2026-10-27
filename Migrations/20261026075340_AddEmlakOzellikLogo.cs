using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class AddEmlakOzellikLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "EmlakOzellikleri",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "EmlakOzellikleri");
        }
    }
}
