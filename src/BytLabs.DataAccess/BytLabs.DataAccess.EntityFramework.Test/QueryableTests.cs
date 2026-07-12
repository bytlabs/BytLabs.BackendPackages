using BytLabs.Application.DataAccess;
using BytLabs.DataAccess.EntityFramework.Test.Domain;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.DataAccess.EntityFramework.Test;

public class QueryableTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public QueryableTests()
    {
        _connection = TestServiceFactory.OpenConnection();
        _provider = TestServiceFactory.Create(_connection);
    }

    [Fact]
    public async Task GIVEN_InsertedOrder_WHEN_QueriedViaIQueryable_THEN_ReturnsNoTrackingProjection()
    {
        var id = Guid.NewGuid();
        using (var scope = _provider.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await uow.OpenTransactionAsync();
            await repo.InsertAsync(new OrderAggregate(id, "Dave", 99m), CancellationToken.None);
            await uow.CommitAsync();
        }

        using var readScope = _provider.CreateScope();
        var orders = readScope.ServiceProvider.GetRequiredService<IQueryable<OrderAggregate>>();

        var names = await orders.Where(o => o.Id == id).Select(o => o.CustomerName).ToListAsync();

        names.Should().ContainSingle().Which.Should().Be("Dave");
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
