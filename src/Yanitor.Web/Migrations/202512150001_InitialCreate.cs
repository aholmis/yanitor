using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Yanitor.Web.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    HouseId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Houses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Houses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Houses_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelectedItemTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HouseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedItemTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelectedItemTypes_Houses_HouseId",
                        column: x => x.HouseId,
                        principalTable: "Houses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HouseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TaskName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TaskType = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RoomType = table.Column<int>(type: "INTEGER", nullable: false),
                    LastCompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IntervalDays = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveTasks_Houses_HouseId",
                        column: x => x.HouseId,
                        principalTable: "Houses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_SelectedItemTypes_HouseId_Type",
                table: "SelectedItemTypes",
                columns: new[] { "HouseId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActiveTasks_NextDueDate",
                table: "ActiveTasks",
                column: "NextDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveTasks_HouseId",
                table: "ActiveTasks",
                column: "HouseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ActiveTasks");
            migrationBuilder.DropTable(name: "SelectedItemTypes");
            migrationBuilder.DropTable(name: "Houses");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
