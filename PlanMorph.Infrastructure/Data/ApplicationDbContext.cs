using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Design> Designs { get; set; }
    public DbSet<DesignFile> DesignFiles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ConstructionContract> ConstructionContracts { get; set; }
    public DbSet<ModificationRequest> ModificationRequests { get; set; }
    public DbSet<DesignVerification> DesignVerifications { get; set; }
    public DbSet<ProfessionalReviewLog> ProfessionalReviewLogs { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketMessage> TicketMessages { get; set; }

    // Mentorship
    public DbSet<MentorProfile> MentorProfiles { get; set; }
    public DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<MentorshipProject> MentorshipProjects { get; set; }
    public DbSet<ProjectIteration> ProjectIterations { get; set; }
    public DbSet<ProjectFile> ProjectFiles { get; set; }
    public DbSet<ProjectMessage> ProjectMessages { get; set; }
    public DbSet<ClientDeliverable> ClientDeliverables { get; set; }
    public DbSet<StudentApplication> StudentApplications { get; set; }
    public DbSet<ProjectDispute> ProjectDisputes { get; set; }
    public DbSet<MentorStudentRelationship> MentorStudentRelationships { get; set; }
    public DbSet<ProjectAuditLog> ProjectAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Design Configuration
        builder.Entity<Design>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.EstimatedConstructionCost).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Architect)
                .WithMany(u => u.Designs)
                .HasForeignKey(e => e.ArchitectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // DesignFile Configuration
        builder.Entity<DesignFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.StorageUrl).IsRequired().HasMaxLength(1000);
            
            entity.HasOne(e => e.Design)
                .WithMany(d => d.Files)
                .HasForeignKey(e => e.DesignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Order Configuration
        builder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Client)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Design)
                .WithMany(d => d.Orders)
                .HasForeignKey(e => e.DesignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Contractor)
                .WithMany()
                .HasForeignKey(e => e.ContractorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ConstructionContract Configuration
        builder.Entity<ConstructionContract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EstimatedCost).HasPrecision(18, 2);
            entity.Property(e => e.CommissionAmount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Order)
                .WithOne(o => o.ConstructionContract)
                .HasForeignKey<ConstructionContract>(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Contractor)
                .WithMany()
                .HasForeignKey(e => e.ContractorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ModificationRequest Configuration
        builder.Entity<ModificationRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.QuotedPrice).HasPrecision(18, 2);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Design)
                .WithMany(d => d.ModificationRequests)
                .HasForeignKey(e => e.DesignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // DesignVerification Configuration
        builder.Entity<DesignVerification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comments).HasMaxLength(2000);

            entity.HasOne(e => e.Design)
                .WithMany(d => d.Verifications)
                .HasForeignKey(e => e.DesignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.VerifierUser)
                .WithMany(u => u.Verifications)
                .HasForeignKey(e => e.VerifierUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // User Configuration
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProfessionalLicense).HasMaxLength(200);
            entity.Property(e => e.PortfolioUrl).HasMaxLength(500);
            entity.Property(e => e.Specialization).HasMaxLength(200);
            entity.Property(e => e.CvUrl).HasMaxLength(1000);
            entity.Property(e => e.CoverLetterUrl).HasMaxLength(1000);
            entity.Property(e => e.WorkExperienceUrl).HasMaxLength(1000);
            entity.Property(e => e.CvFileName).HasMaxLength(255);
            entity.Property(e => e.CoverLetterFileName).HasMaxLength(255);
            entity.Property(e => e.WorkExperienceFileName).HasMaxLength(255);
            entity.Property(e => e.VerificationNotes).HasMaxLength(2000);
            entity.Property(e => e.LastReviewedByName).HasMaxLength(200);
            entity.Property(e => e.RejectionReason).HasMaxLength(2000);
        });

        builder.Entity<ProfessionalReviewLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne(e => e.ProfessionalUser)
                .WithMany(u => u.ProfessionalReviewLogs)
                .HasForeignKey(e => e.ProfessionalUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AdminUser)
                .WithMany()
                .HasForeignKey(e => e.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Ticket Configuration
        builder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TicketNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200); // Changed from Title
            entity.Property(e => e.Description).IsRequired().HasMaxLength(5000);

            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedTo)
                .WithMany()
                .HasForeignKey(e => e.AssignedToAdminId) // Changed from AssignedToId
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Design)
                .WithMany()
                .HasForeignKey(e => e.DesignId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // TicketMessage Configuration
        builder.Entity<TicketMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(5000); // Changed from Message
            entity.Property(e => e.AuthorName).IsRequired().HasMaxLength(100); // Added

            entity.HasOne(e => e.Ticket)
                .WithMany(t => t.Messages)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.AuthorId) // Changed from SenderId
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // === MENTORSHIP ENTITIES ===

        // MentorProfile Configuration
        builder.Entity<MentorProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Bio).HasMaxLength(2000);
            entity.Property(e => e.Specializations).HasMaxLength(1000);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // StudentProfile Configuration
        builder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UniversityName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.StudentIdNumber).HasMaxLength(100);
            entity.Property(e => e.TranscriptUrl).HasMaxLength(1000);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.Property(e => e.TotalEarnings).HasPrecision(18, 2);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Mentor)
                .WithMany()
                .HasForeignKey(e => e.MentorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // MentorshipProject Configuration
        builder.Entity<MentorshipProject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProjectNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.ProjectNumber).IsUnique();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.Requirements).HasMaxLength(5000);
            entity.Property(e => e.Scope).HasMaxLength(5000);
            entity.Property(e => e.CancellationReason).HasMaxLength(2000);
            entity.Property(e => e.ClientFee).HasPrecision(18, 2);
            entity.Property(e => e.MentorFee).HasPrecision(18, 2);
            entity.Property(e => e.StudentFee).HasPrecision(18, 2);
            entity.Property(e => e.PlatformFee).HasPrecision(18, 2);

            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Mentor)
                .WithMany()
                .HasForeignKey(e => e.MentorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Design)
                .WithMany()
                .HasForeignKey(e => e.DesignId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ProjectIteration Configuration
        builder.Entity<ProjectIteration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(5000);
            entity.Property(e => e.ReviewNotes).HasMaxLength(5000);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Iterations)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SubmittedBy)
                .WithMany()
                .HasForeignKey(e => e.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReviewedBy)
                .WithMany()
                .HasForeignKey(e => e.ReviewedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ProjectFile Configuration
        builder.Entity<ProjectFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.StorageUrl).IsRequired().HasMaxLength(1000);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Files)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Iteration)
                .WithMany(i => i.Files)
                .HasForeignKey(e => e.IterationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UploadedBy)
                .WithMany()
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ProjectMessage Configuration
        builder.Entity<ProjectMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(5000);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Messages)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ClientDeliverable Configuration
        builder.Entity<ClientDeliverable>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClientNotes).HasMaxLength(5000);
            entity.Property(e => e.MentorNotes).HasMaxLength(5000);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Deliverables)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Iteration)
                .WithMany()
                .HasForeignKey(e => e.IterationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeliveredByMentor)
                .WithMany()
                .HasForeignKey(e => e.DeliveredByMentorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // StudentApplication Configuration
        builder.Entity<StudentApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UniversityName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.StudentIdNumber).HasMaxLength(100);
            entity.Property(e => e.TranscriptUrl).HasMaxLength(1000);
            entity.Property(e => e.PortfolioUrl).HasMaxLength(1000);
            entity.Property(e => e.CoverLetterUrl).HasMaxLength(1000);
            entity.Property(e => e.ReviewNotes).HasMaxLength(2000);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.InvitedByMentor)
                .WithMany()
                .HasForeignKey(e => e.InvitedByMentorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReviewedBy)
                .WithMany()
                .HasForeignKey(e => e.ReviewedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ProjectDispute Configuration
        builder.Entity<ProjectDispute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RaisedByRole).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(5000);
            entity.Property(e => e.Resolution).HasMaxLength(5000);

            entity.HasOne(e => e.Project)
                .WithOne(p => p.Dispute)
                .HasForeignKey<ProjectDispute>(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RaisedBy)
                .WithMany()
                .HasForeignKey(e => e.RaisedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ResolvedBy)
                .WithMany()
                .HasForeignKey(e => e.ResolvedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // MentorStudentRelationship Configuration
        builder.Entity<MentorStudentRelationship>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EndReason).HasMaxLength(500);
            entity.Property(e => e.MentorRating).HasPrecision(3, 2);
            entity.Property(e => e.StudentRating).HasPrecision(3, 2);

            entity.HasOne(e => e.Mentor)
                .WithMany()
                .HasForeignKey(e => e.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ProjectAuditLog Configuration (no BaseEntity, no soft delete)
        builder.Entity<ProjectAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActorRole).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OldValue).HasMaxLength(2000);
            entity.Property(e => e.NewValue).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(50);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.AuditLogs)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Actor)
                .WithMany()
                .HasForeignKey(e => e.ActorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
