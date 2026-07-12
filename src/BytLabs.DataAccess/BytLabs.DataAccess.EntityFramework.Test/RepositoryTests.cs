using BytLabs.Application.DataAccess;
using BytLabs.Application.Exceptions;
using BytLabs.DataAccess.EntityFramework.Test.Domain;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.DataAccess.EntityFramework.Test;

public class RepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public RepositoryTests()
    {
        _connection = TestServiceFactory.OpenConnection();
        _provider = TestServiceFactory.Create(_connection);
    }

    private async Task<TResult> InScopeAsync<TResult>(Func<IRepository<OrderAggregate, Guid>, IUnitOfWork, Task<TResult>> action)
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await uow.OpenTransactionAsync();
        var result = await action(repo, uow);
        await uow.CommitAsync();
        return result;
    }

    [Fact]
    public async Task GIVEN_InsertedOrder_WHEN_GetById_THEN_ReturnsItWithAuditStamped()
    {
        var id = Guid.NewGuid();

        await InScopeAsync(async (repo, _) =>
        {
            await repo.InsertAsync(new OrderAggregate(id, "Alice", 42m), CancellationToken.None);
            return true;
        });

        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();
        var loaded = await repo.GetByIdAsync(id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded.CustomerName.Should().Be("Alice");
        loaded.AuditInfo.CreatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GIVEN_MissingId_WHEN_GetById_THEN_ThrowsEntityNotFound()
    {
        using var scope = _provider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();

        var act = () => repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
