using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace dug.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DnsServers",
                columns: table => new
                {
                    DnsServerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", nullable: false),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    DNSSEC = table.Column<bool>(type: "INTEGER", nullable: false),
                    Reliability = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnsServers", x => x.DnsServerId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DnsServers_CountryCode",
                table: "DnsServers",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DnsServers_IPAddress",
                table: "DnsServers",
                column: "IPAddress",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DnsServers");
        }
    }
}
