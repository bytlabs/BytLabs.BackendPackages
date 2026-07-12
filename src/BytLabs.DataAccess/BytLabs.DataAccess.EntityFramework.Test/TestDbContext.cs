using BytLabs.DataAccess.EntityFramework.Test.Domain;
using Microsoft.EntityFrameworkCore;

namespace BytLabs.DataAccess.EntityFramework.Test;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderAggregate>(b =>
        {
            b.HasKey(x => x.Id);
            b.OwnsOne(x => x.AuditInfo);
        });

        modelBuilder.Entity<ProductAggregate>(b =>
        {
            b.HasKey(x => x.Id);
            b.OwnsOne(x => x.AuditInfo);
        });

        // Unmap DomainEvents on all aggregate roots in this assembly.
        modelBuilder.IgnoreDomainEvents(typeof(OrderAggregate).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
