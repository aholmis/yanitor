using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yanitor.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OneTimePasswords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimePasswords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePasswords_Code",
                table: "OneTimePasswords",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePasswords_Email_CreatedAt",
                table: "OneTimePasswords",
                columns: new[] { "Email", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePasswords_ExpiresAt",
                table: "OneTimePasswords",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OneTimePasswords");
        }
    }
}
