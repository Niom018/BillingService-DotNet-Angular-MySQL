using BillingService.Application.Interfaces;

namespace BillingService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly BillingDbContext _db;
    public UnitOfWork(BillingDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
