using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanMorph.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalDocumentsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverLetterUrl",
                table: "AspNetUsers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvUrl",
                table: "AspNetUsers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkExperienceUrl",
                table: "AspNetUsers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverLetterUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CvUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkExperienceUrl",
                table: "AspNetUsers");
        }
    }
}
