using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanMorph.Core.Entities;

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
    }
}
