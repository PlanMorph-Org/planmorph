using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Design> Designs { get; }
    IRepository<DesignFile> DesignFiles { get; }
    IRepository<Order> Orders { get; }
    IRepository<ConstructionContract> ConstructionContracts { get; }
    IRepository<ModificationRequest> ModificationRequests { get; }
    IRepository<DesignVerification> DesignVerifications { get; }
    IRepository<User> Users { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}