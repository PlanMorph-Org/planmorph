using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanMorph.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMentorshipSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MentorProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxConcurrentStudents = table.Column<int>(type: "integer", nullable: false),
                    MaxConcurrentProjects = table.Column<int>(type: "integer", nullable: false),
                    Bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Specializations = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AcceptingStudents = table.Column<bool>(type: "boolean", nullable: false),
                    TotalProjectsCompleted = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    TotalStudentsMentored = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MentorshipProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Requirements = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ProjectType = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    EstimatedDeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    ClientFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MentorFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StudentFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    DesignId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaxRevisions = table.Column<int>(type: "integer", nullable: false),
                    CurrentRevisionCount = table.Column<int>(type: "integer", nullable: false),
                    MentorDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StudentDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorshipProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorshipProjects_AspNetUsers_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentorshipProjects_AspNetUsers_MentorId",
                        column: x => x.MentorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentorshipProjects_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentorshipProjects_Designs_DesignId",
                        column: x => x.DesignId,
                        principalTable: "Designs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentorshipProjects_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MentorStudentRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MentorId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MentorRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    StudentRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    ProjectsCompleted = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorStudentRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentorStudentRelationships_AspNetUsers_MentorId",
                        column: x => x.MentorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentorStudentRelationships_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationType = table.Column<int>(type: "integer", nullable: false),
                    InvitedByMentorId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentType = table.Column<int>(type: "integer", nullable: false),
                    UniversityName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    StudentIdNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TranscriptUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PortfolioUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CoverLetterUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentApplications_AspNetUsers_InvitedByMentorId",
                        column: x => x.InvitedByMentorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentApplications_AspNetUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentApplications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentType = table.Column<int>(type: "integer", nullable: false),
                    UniversityName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EnrollmentStatus = table.Column<int>(type: "integer", nullable: false),
                    ExpectedGraduation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StudentIdNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TranscriptUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MentorId = table.Column<Guid>(type: "uuid", nullable: true),
                    MentorshipStatus = table.Column<int>(type: "integer", nullable: false),
                    TotalProjectsCompleted = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    TotalEarnings = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_AspNetUsers_MentorId",
                        column: x => x.MentorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAuditLogs_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectAuditLogs_MentorshipProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "MentorshipProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDisputes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RaisedById = table.Column<Guid>(type: "uuid", nullable: false),
                    RaisedByRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Resolution = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ResolvedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDisputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDisputes_AspNetUsers_RaisedById",
                        column: x => x.RaisedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectDisputes_AspNetUsers_ResolvedById",
                        column: x => x.ResolvedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectDisputes_MentorshipProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "MentorshipProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectIterations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IterationNumber = table.Column<int>(type: "integer", nullable: false),
                    SubmittedById = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedByRole = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectIterations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectIterations_AspNetUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectIterations_AspNetUsers_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectIterations_MentorshipProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "MentorshipProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderRole = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    IsSystemMessage = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMessages_MentorshipProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "MentorshipProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientDeliverables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IterationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveryNumber = table.Column<int>(type: "integer", nullable: false),
                    DeliveredByMentorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClientNotes = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    MentorNotes = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientDeliverables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientDeliverables_AspNetUsers_DeliveredByMentorId",
                        column: x => x.DeliveredByMentorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientDeliverables_MentorshipProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "MentorshipProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientDeliverables_ProjectIterations_IterationId",
                        column: x => x.IterationId,
                        principalTable: "ProjectIterations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IterationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    StorageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectFiles_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectFiles_MentorshipProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "MentorshipProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectFiles_ProjectIterations_IterationId",
                        column: x => x.IterationId,
                        principalTable: "ProjectIterations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientDeliverables_DeliveredByMentorId",
                table: "ClientDeliverables",
                column: "DeliveredByMentorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDeliverables_IterationId",
                table: "ClientDeliverables",
                column: "IterationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDeliverables_ProjectId",
                table: "ClientDeliverables",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorProfiles_UserId",
                table: "MentorProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipProjects_ClientId",
                table: "MentorshipProjects",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipProjects_DesignId",
                table: "MentorshipProjects",
                column: "DesignId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipProjects_MentorId",
                table: "MentorshipProjects",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipProjects_OrderId",
                table: "MentorshipProjects",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipProjects_ProjectNumber",
                table: "MentorshipProjects",
                column: "ProjectNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MentorshipProjects_StudentId",
                table: "MentorshipProjects",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorStudentRelationships_MentorId",
                table: "MentorStudentRelationships",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_MentorStudentRelationships_StudentId",
                table: "MentorStudentRelationships",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAuditLogs_ActorId",
                table: "ProjectAuditLogs",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAuditLogs_ProjectId",
                table: "ProjectAuditLogs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDisputes_ProjectId",
                table: "ProjectDisputes",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDisputes_RaisedById",
                table: "ProjectDisputes",
                column: "RaisedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDisputes_ResolvedById",
                table: "ProjectDisputes",
                column: "ResolvedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFiles_IterationId",
                table: "ProjectFiles",
                column: "IterationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFiles_ProjectId",
                table: "ProjectFiles",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFiles_UploadedById",
                table: "ProjectFiles",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIterations_ProjectId",
                table: "ProjectIterations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIterations_ReviewedById",
                table: "ProjectIterations",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIterations_SubmittedById",
                table: "ProjectIterations",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMessages_ProjectId",
                table: "ProjectMessages",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMessages_SenderId",
                table: "ProjectMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_InvitedByMentorId",
                table: "StudentApplications",
                column: "InvitedByMentorId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_ReviewedById",
                table: "StudentApplications",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_StudentApplications_UserId",
                table: "StudentApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_MentorId",
                table: "StudentProfiles",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientDeliverables");

            migrationBuilder.DropTable(
                name: "MentorProfiles");

            migrationBuilder.DropTable(
                name: "MentorStudentRelationships");

            migrationBuilder.DropTable(
                name: "ProjectAuditLogs");

            migrationBuilder.DropTable(
                name: "ProjectDisputes");

            migrationBuilder.DropTable(
                name: "ProjectFiles");

            migrationBuilder.DropTable(
                name: "ProjectMessages");

            migrationBuilder.DropTable(
                name: "StudentApplications");

            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "ProjectIterations");

            migrationBuilder.DropTable(
                name: "MentorshipProjects");
        }
    }
}
