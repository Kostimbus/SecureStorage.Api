using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureStorage.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActorUsername = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Details = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    FileId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RemoteIp = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_ActorUsername",
                table: "AuditEntries",
                column: "ActorUsername");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_EventType",
                table: "AuditEntries",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_OccurredAtUtc",
                table: "AuditEntries",
                column: "OccurredAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEntries");
        }
    }
}
