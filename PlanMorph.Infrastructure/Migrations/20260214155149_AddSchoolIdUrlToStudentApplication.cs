using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanMorph.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolIdUrlToStudentApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SchoolIdUrl",
                table: "StudentApplications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchoolIdUrl",
                table: "StudentApplications");
        }
    }
}
