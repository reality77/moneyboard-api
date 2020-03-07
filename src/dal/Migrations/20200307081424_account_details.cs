using Microsoft.EntityFrameworkCore.Migrations;

namespace dal.Migrations
{
    public partial class account_details : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Accounts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "Accounts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Iban",
                table: "Accounts",
                column: "Iban",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Number",
                table: "Accounts",
                column: "Number",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Iban",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Number",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Accounts");
        }
    }
}
