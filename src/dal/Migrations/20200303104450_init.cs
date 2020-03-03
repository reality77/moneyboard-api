using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace dal.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Balance = table.Column<decimal>(nullable: false, defaultValue: 0m),
                    Currency = table.Column<int>(nullable: false, defaultValue: 1),
                    InitialBalance = table.Column<decimal>(nullable: false, defaultValue: 0m),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    FileName = table.Column<string>(nullable: false),
                    ImportDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagTypes",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    Caption = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTypes", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "TransactionRecognitionRules",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UseOrConditions = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRecognitionRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Amount = table.Column<decimal>(nullable: false, defaultValue: 0m),
                    Caption = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false, defaultValue: 0),
                    UserDate = table.Column<DateTime>(nullable: true),
                    AccountId = table.Column<int>(nullable: false),
                    transaction_type = table.Column<int>(nullable: false),
                    ImportFileId = table.Column<int>(nullable: true),
                    ImportNumber = table.Column<string>(nullable: true),
                    ImportCaption = table.Column<string>(nullable: true),
                    ImportComment = table.Column<string>(nullable: true),
                    ImportHash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_ImportedFiles_ImportFileId",
                        column: x => x.ImportFileId,
                        principalTable: "ImportedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Key = table.Column<string>(nullable: false),
                    Caption = table.Column<string>(nullable: true),
                    TagTypeKey = table.Column<string>(nullable: true),
                    ParentTagId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Tags_ParentTagId",
                        column: x => x.ParentTagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tags_TagTypes_TagTypeKey",
                        column: x => x.TagTypeKey,
                        principalTable: "TagTypes",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionRecognitionRuleAction",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TransactionRecognitionRuleId = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Field = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRecognitionRuleAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionRecognitionRuleAction_TransactionRecognitionRule~",
                        column: x => x.TransactionRecognitionRuleId,
                        principalTable: "TransactionRecognitionRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionRecognitionRuleCondition",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TransactionRecognitionRuleId = table.Column<int>(nullable: false),
                    FieldType = table.Column<int>(nullable: false),
                    FieldName = table.Column<string>(nullable: true),
                    ValueOperator = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRecognitionRuleCondition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionRecognitionRuleCondition_TransactionRecognitionR~",
                        column: x => x.TransactionRecognitionRuleId,
                        principalTable: "TransactionRecognitionRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTag",
                columns: table => new
                {
                    TagId = table.Column<int>(nullable: false),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTag", x => new { x.TransactionId, x.TagId });
                    table.ForeignKey(
                        name: "FK_TransactionTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionTag_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportedFiles_FileName",
                table: "ImportedFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ParentTagId",
                table: "Tags",
                column: "ParentTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagTypeKey_Key",
                table: "Tags",
                columns: new[] { "TagTypeKey", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecognitionRuleAction_TransactionRecognitionRule~",
                table: "TransactionRecognitionRuleAction",
                column: "TransactionRecognitionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecognitionRuleCondition_TransactionRecognitionR~",
                table: "TransactionRecognitionRuleCondition",
                column: "TransactionRecognitionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ImportFileId",
                table: "Transactions",
                column: "ImportFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ImportHash",
                table: "Transactions",
                column: "ImportHash");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTag_TagId",
                table: "TransactionTag",
                column: "TagId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionRecognitionRuleAction");

            migrationBuilder.DropTable(
                name: "TransactionRecognitionRuleCondition");

            migrationBuilder.DropTable(
                name: "TransactionTag");

            migrationBuilder.DropTable(
                name: "TransactionRecognitionRules");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "TagTypes");

            migrationBuilder.DropTable(
                name: "ImportedFiles");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
