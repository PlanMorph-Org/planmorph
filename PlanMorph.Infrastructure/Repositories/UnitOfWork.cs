using Microsoft.EntityFrameworkCore.Storage;
using PlanMorph.Core.Entities;
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
    public IRepository<User> Users { get; }
    public IRepository<Ticket> Tickets { get; }
    public IRepository<TicketMessage> TicketMessages { get; }

    // Specific ticket repositories
    public ITicketRepository TicketRepository { get; }
    public ITicketMessageRepository TicketMessageRepository { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Designs = new Repository<Design>(context);
        DesignFiles = new Repository<DesignFile>(context);
        Orders = new Repository<Order>(context);
        ConstructionContracts = new Repository<ConstructionContract>(context);
        ModificationRequests = new Repository<ModificationRequest>(context);
        DesignVerifications = new Repository<DesignVerification>(context);
        Users = new Repository<User>(context);
        Tickets = new Repository<Ticket>(context);
        TicketMessages = new Repository<TicketMessage>(context);

        // Initialize specific ticket repositories
        TicketRepository = new TicketRepository(context);
        TicketMessageRepository = new TicketMessageRepository(context);
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