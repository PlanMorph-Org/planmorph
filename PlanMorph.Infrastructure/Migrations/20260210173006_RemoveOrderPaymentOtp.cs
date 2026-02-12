using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanMorph.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrderPaymentOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentOtpAttempts",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentOtpExpiresAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentOtpHash",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentOtpRequestedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentOtpVerifiedAt",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentOtpAttempts",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentOtpExpiresAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentOtpHash",
                table: "Orders",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentOtpRequestedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentOtpVerifiedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
