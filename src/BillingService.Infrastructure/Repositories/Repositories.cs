using BillingService.Application.Interfaces;
using BillingService.Domain.Entities;
using BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly BillingDbContext _db;
    public CustomerRepository(BillingDbContext db) => _db = db;

    public Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Customers.OrderBy(c => c.Name).ToListAsync(ct);

    public async Task AddAsync(Customer customer, CancellationToken ct = default) =>
        await _db.Customers.AddAsync(customer, ct);
}

public class ProductRepository : IProductRepository
{
    private readonly BillingDbContext _db;
    public ProductRepository(BillingDbContext db) => _db = db;

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
        await _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Products.OrderBy(p => p.Name).ToListAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await _db.Products.AddAsync(product, ct);
}

public class OrderRepository : IOrderRepository
{
    private readonly BillingDbContext _db;
    public OrderRepository(BillingDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Customer)
            .Include(o => o.Payment)
            .Include(o => o.Invoice)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default) =>
        _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await _db.Orders.AddAsync(order, ct);

    public Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken ct = default) =>
        _db.Orders.AnyAsync(o => o.OrderNumber == orderNumber, ct);
}
