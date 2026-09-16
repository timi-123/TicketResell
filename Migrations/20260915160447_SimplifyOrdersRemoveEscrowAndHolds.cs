using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketResell.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyOrdersRemoveEscrowAndHolds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerConfirmedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DisputeReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DisputedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PayoutReleasedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReservedUntil",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BuyerConfirmedAt",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisputeReason",
                table: "Orders",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisputedAt",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PayoutReleasedAt",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservedUntil",
                table: "Orders",
                type: "TEXT",
                nullable: true);
        }
    }
}
