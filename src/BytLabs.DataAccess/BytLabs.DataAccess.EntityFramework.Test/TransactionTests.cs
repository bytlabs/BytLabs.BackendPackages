using BytLabs.Application.DataAccess;
using BytLabs.DataAccess.EntityFramework.Test.Domain;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.DataAccess.EntityFramework.Test;

public class TransactionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public TransactionTests()
    {
        _connection = TestServiceFactory.OpenConnection();
        _provider = TestServiceFactory.Create(_connection);
    }

    [Fact]
    public async Task GIVEN_Commit_WHEN_InsertThenCommit_THEN_Persists()
    {
        var id = Guid.NewGuid();
        using (var scope = _provider.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await uow.OpenTransactionAsync();
            await repo.InsertAsync(new OrderAggregate(id, "Bob", 10m), CancellationToken.None);
            await uow.CommitAsync();
        }

        using var verifyScope = _provider.CreateScope();
        var verifyRepo = verifyScope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();
        (await verifyRepo.FindByIdAsync(id, CancellationToken.None)).Should().NotBeNull();
    }

    [Fact]
    public async Task GIVEN_Rollback_WHEN_InsertThenRollback_THEN_DoesNotPersist()
    {
        var id = Guid.NewGuid();
        using (var scope = _provider.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await uow.OpenTransactionAsync();
            await repo.InsertAsync(new OrderAggregate(id, "Carol", 10m), CancellationToken.None);
            await uow.RollbackAsync();
        }

        using var verifyScope = _provider.CreateScope();
        var verifyRepo = verifyScope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();
        (await verifyRepo.FindByIdAsync(id, CancellationToken.None)).Should().BeNull();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
