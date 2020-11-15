using Microsoft.EntityFrameworkCore.Migrations;

namespace dal.Migrations
{
    public partial class one_tag_only : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OneTagOnly",
                table: "TagTypes",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OneTagOnly",
                table: "TagTypes");
        }
    }
}
