using Microsoft.EntityFrameworkCore.Migrations;

namespace Market.Migrations
{
    public partial class newMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "account",
                table: "users",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "account",
                table: "users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                schema: "account",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "account",
                table: "users");
        }
    }
}
