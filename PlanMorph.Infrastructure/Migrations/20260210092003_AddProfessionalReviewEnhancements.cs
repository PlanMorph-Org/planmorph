using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanMorph.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalReviewEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverLetterFileName",
                table: "AspNetUsers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CoverLetterFileSizeBytes",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CoverLetterUploadedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvFileName",
                table: "AspNetUsers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CvFileSizeBytes",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CvUploadedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentsVerified",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExperienceVerified",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReviewedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastReviewedById",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastReviewedByName",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LicenseVerified",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedById",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "AspNetUsers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNotes",
                table: "AspNetUsers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkExperienceFileName",
                table: "AspNetUsers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WorkExperienceFileSizeBytes",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkExperienceUploadedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProfessionalReviewLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LicenseVerified = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ExperienceVerified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalReviewLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfessionalReviewLogs_AspNetUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfessionalReviewLogs_AspNetUsers_ProfessionalUserId",
                        column: x => x.ProfessionalUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalReviewLogs_AdminUserId",
                table: "ProfessionalReviewLogs",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalReviewLogs_ProfessionalUserId",
                table: "ProfessionalReviewLogs",
                column: "ProfessionalUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfessionalReviewLogs");

            migrationBuilder.DropColumn(
                name: "CoverLetterFileName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CoverLetterFileSizeBytes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CoverLetterUploadedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CvFileName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CvFileSizeBytes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CvUploadedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DocumentsVerified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExperienceVerified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastReviewedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastReviewedById",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastReviewedByName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LicenseVerified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RejectedById",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VerificationNotes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkExperienceFileName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkExperienceFileSizeBytes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkExperienceUploadedAt",
                table: "AspNetUsers");
        }
    }
}
