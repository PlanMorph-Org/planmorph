using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces.Repositories;

namespace PlanMorph.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Design> Designs { get; }
    IRepository<DesignFile> DesignFiles { get; }
    IRepository<Order> Orders { get; }
    IRepository<ConstructionContract> ConstructionContracts { get; }
    IRepository<ModificationRequest> ModificationRequests { get; }
    IRepository<DesignVerification> DesignVerifications { get; }
    IRepository<PayoutRequest> PayoutRequests { get; }
    IRepository<Wallet> Wallets { get; }
    IRepository<WalletTransaction> WalletTransactions { get; }
    IRepository<CommissionTier> CommissionTiers { get; }
    IRepository<FinancialAuditLog> FinancialAuditLogs { get; }
    IRepository<PaystackEventLog> PaystackEventLogs { get; }
    IRepository<User> Users { get; }
    IRepository<Ticket> Tickets { get; }
    IRepository<TicketMessage> TicketMessages { get; }

    // Specific ticket repositories
    ITicketRepository TicketRepository { get; }
    ITicketMessageRepository TicketMessageRepository { get; }

    // Mentorship repositories
    IRepository<MentorProfile> MentorProfiles { get; }
    IRepository<StudentProfile> StudentProfiles { get; }
    IRepository<MentorshipProject> MentorshipProjects { get; }
    IRepository<ProjectIteration> ProjectIterations { get; }
    IRepository<ProjectFile> ProjectFiles { get; }
    IRepository<ProjectMessage> ProjectMessages { get; }
    IRepository<ClientDeliverable> ClientDeliverables { get; }
    IRepository<StudentApplication> StudentApplications { get; }
    IRepository<ProjectDispute> ProjectDisputes { get; }
    IRepository<MentorStudentRelationship> MentorStudentRelationships { get; }
    IRepository<ProjectAuditLog> ProjectAuditLogs { get; }
    IMentorshipProjectRepository MentorshipProjectRepository { get; }
    IStudentApplicationRepository StudentApplicationRepository { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}