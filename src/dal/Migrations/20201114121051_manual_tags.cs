using Microsoft.EntityFrameworkCore.Migrations;

namespace dal.Migrations
{
    public partial class manual_tags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManual",
                table: "TransactionTag",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManual",
                table: "TransactionTag");
        }
    }
}
