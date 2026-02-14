using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanMorph.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketEntitiesForSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_AspNetUsers_SenderId",
                table: "TicketMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedToId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Tickets",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "AssignedToId",
                table: "Tickets",
                newName: "AssignedToAdminId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_AssignedToId",
                table: "Tickets",
                newName: "IX_Tickets_AssignedToAdminId");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "TicketMessages",
                newName: "AuthorId");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "TicketMessages",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "IsStaffMessage",
                table: "TicketMessages",
                newName: "IsReadByClient");

            migrationBuilder.RenameIndex(
                name: "IX_TicketMessages_SenderId",
                table: "TicketMessages",
                newName: "IX_TicketMessages_AuthorId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "TicketMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFromAdmin",
                table: "TicketMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_AspNetUsers_AuthorId",
                table: "TicketMessages",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedToAdminId",
                table: "Tickets",
                column: "AssignedToAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_AspNetUsers_AuthorId",
                table: "TicketMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedToAdminId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "TicketMessages");

            migrationBuilder.DropColumn(
                name: "IsFromAdmin",
                table: "TicketMessages");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "Tickets",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "AssignedToAdminId",
                table: "Tickets",
                newName: "AssignedToId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_AssignedToAdminId",
                table: "Tickets",
                newName: "IX_Tickets_AssignedToId");

            migrationBuilder.RenameColumn(
                name: "IsReadByClient",
                table: "TicketMessages",
                newName: "IsStaffMessage");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "TicketMessages",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "TicketMessages",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketMessages_AuthorId",
                table: "TicketMessages",
                newName: "IX_TicketMessages_SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_AspNetUsers_SenderId",
                table: "TicketMessages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedToId",
                table: "Tickets",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
