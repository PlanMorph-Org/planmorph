using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanMorph.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DesignVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DesignId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifierUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerificationType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignVerifications_AspNetUsers_VerifierUserId",
                        column: x => x.VerifierUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DesignVerifications_Designs_DesignId",
                        column: x => x.DesignId,
                        principalTable: "Designs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DesignVerifications_DesignId",
                table: "DesignVerifications",
                column: "DesignId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignVerifications_VerifierUserId",
                table: "DesignVerifications",
                column: "VerifierUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DesignVerifications");
        }
    }
}
