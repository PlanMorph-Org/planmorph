using Microsoft.EntityFrameworkCore.Storage;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces;
using PlanMorph.Core.Interfaces.Repositories;
using PlanMorph.Infrastructure.Data;

namespace PlanMorph.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IRepository<Design> Designs { get; }
    public IRepository<DesignFile> DesignFiles { get; }
    public IRepository<Order> Orders { get; }
    public IRepository<ConstructionContract> ConstructionContracts { get; }
    public IRepository<ModificationRequest> ModificationRequests { get; }
    public IRepository<DesignVerification> DesignVerifications { get; }
    public IRepository<PayoutRequest> PayoutRequests { get; }
    public IRepository<Wallet> Wallets { get; }
    public IRepository<WalletTransaction> WalletTransactions { get; }
    public IRepository<CommissionTier> CommissionTiers { get; }
    public IRepository<FinancialAuditLog> FinancialAuditLogs { get; }
    public IRepository<PaystackEventLog> PaystackEventLogs { get; }
    public IRepository<User> Users { get; }
    public IRepository<Ticket> Tickets { get; }
    public IRepository<TicketMessage> TicketMessages { get; }

    // Specific ticket repositories
    public ITicketRepository TicketRepository { get; }
    public ITicketMessageRepository TicketMessageRepository { get; }

    // Mentorship repositories
    public IRepository<MentorProfile> MentorProfiles { get; }
    public IRepository<StudentProfile> StudentProfiles { get; }
    public IRepository<MentorshipProject> MentorshipProjects { get; }
    public IRepository<ProjectIteration> ProjectIterations { get; }
    public IRepository<ProjectFile> ProjectFiles { get; }
    public IRepository<ProjectMessage> ProjectMessages { get; }
    public IRepository<ClientDeliverable> ClientDeliverables { get; }
    public IRepository<StudentApplication> StudentApplications { get; }
    public IRepository<ProjectDispute> ProjectDisputes { get; }
    public IRepository<MentorStudentRelationship> MentorStudentRelationships { get; }
    public IRepository<ProjectAuditLog> ProjectAuditLogs { get; }
    public IMentorshipProjectRepository MentorshipProjectRepository { get; }
    public IStudentApplicationRepository StudentApplicationRepository { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Designs = new Repository<Design>(context);
        DesignFiles = new Repository<DesignFile>(context);
        Orders = new Repository<Order>(context);
        ConstructionContracts = new Repository<ConstructionContract>(context);
        ModificationRequests = new Repository<ModificationRequest>(context);
        DesignVerifications = new Repository<DesignVerification>(context);
        PayoutRequests = new Repository<PayoutRequest>(context);
        Wallets = new Repository<Wallet>(context);
        WalletTransactions = new Repository<WalletTransaction>(context);
        CommissionTiers = new Repository<CommissionTier>(context);
        FinancialAuditLogs = new Repository<FinancialAuditLog>(context);
        PaystackEventLogs = new Repository<PaystackEventLog>(context);
        Users = new Repository<User>(context);
        Tickets = new Repository<Ticket>(context);
        TicketMessages = new Repository<TicketMessage>(context);

        // Initialize specific ticket repositories
        TicketRepository = new TicketRepository(context);
        TicketMessageRepository = new TicketMessageRepository(context);

        // Initialize mentorship repositories
        MentorProfiles = new Repository<MentorProfile>(context);
        StudentProfiles = new Repository<StudentProfile>(context);
        MentorshipProjects = new Repository<MentorshipProject>(context);
        ProjectIterations = new Repository<ProjectIteration>(context);
        ProjectFiles = new Repository<ProjectFile>(context);
        ProjectMessages = new Repository<ProjectMessage>(context);
        ClientDeliverables = new Repository<ClientDeliverable>(context);
        StudentApplications = new Repository<StudentApplication>(context);
        ProjectDisputes = new Repository<ProjectDispute>(context);
        MentorStudentRelationships = new Repository<MentorStudentRelationship>(context);
        ProjectAuditLogs = new Repository<ProjectAuditLog>(context);
        MentorshipProjectRepository = new MentorshipProjectRepository(context);
        StudentApplicationRepository = new StudentApplicationRepository(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}