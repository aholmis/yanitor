using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Yanitor.Web.Migrations
{
    public partial class AddEmailAuthToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old unique index on Name
            migrationBuilder.DropIndex(
                name: "IX_Users_Name",
                table: "Users");

            // Drop the one-to-one relationship index
            migrationBuilder.DropIndex(
                name: "IX_Houses_OwnerId",
                table: "Houses");

            // Drop the HouseId column from Users (no longer needed)
            migrationBuilder.DropColumn(
                name: "HouseId",
                table: "Users");

            // Drop the Name column from Users (replaced by Email)
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");

            // Add Email column
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "TEXT",
                maxLength: 320,
                nullable: false,
                defaultValue: "");

            // Add DisplayName column
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            // Add CreatedAt column
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            // Add LastLoginAt column
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "TEXT",
                nullable: true);

            // Add EmailVerified column
            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Create unique index on Email
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            // Create non-unique index on OwnerId (one-to-many)
            migrationBuilder.CreateIndex(
                name: "IX_Houses_OwnerId",
                table: "Houses",
                column: "OwnerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Houses_OwnerId",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "HouseId",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Houses_OwnerId",
                table: "Houses",
                column: "OwnerId",
                unique: true);
        }
    }
}
