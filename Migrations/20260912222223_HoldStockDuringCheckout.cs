using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketResell.Migrations
{
    /// <inheritdoc />
    public partial class HoldStockDuringCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReservedUntil",
                table: "Orders",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservedUntil",
                table: "Orders");
        }
    }
}
