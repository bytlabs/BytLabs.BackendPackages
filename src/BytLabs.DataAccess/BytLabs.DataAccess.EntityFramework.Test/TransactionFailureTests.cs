using BytLabs.Application.DataAccess;
using BytLabs.Infrastructure.Exceptions;
using FluentAssertions;
using BytLabs.DataAccess.EntityFramework.Test.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.DataAccess.EntityFramework.Test;

/// <summary>
/// What the unit of work does when the database has ended the transaction underneath it.
/// </summary>
/// <remarks>
/// The MongoDB unit of work meets this constantly — the server aborts a transaction that loses a
/// write conflict — and used to answer it by throwing "Transaction was not initialized" over
/// whatever the command had really failed on. These pin the same ground for Entity Framework, where
/// the transaction is ended here rather than by a server, so a fix on one side is never quietly
/// undone on the other.
/// </remarks>
public class TransactionFailureTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public TransactionFailureTests()
    {
        _connection = TestServiceFactory.OpenConnection();
        _provider = TestServiceFactory.Create(_connection);
    }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GIVEN_TransactionEndedUnderneath_WHEN_Committing_THEN_ItSaysWhatTheDatabaseSaid()
    {
        using var scope = _provider.CreateScope();
        var unitOfWork = (EfUnitOfWork)scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.OpenTransactionAsync();

        // Stands in for whatever ends a transaction without the unit of work being told: a deadlock
        // victim, a timeout, a connection reset.
        unitOfWork.Session!.GetDbTransaction().Rollback();

        var commit = () => unitOfWork.CommitAsync();

        // The database's own complaint, not a guard clause of ours standing in front of it.
        (await commit.Should().ThrowAsync<Exception>())
            .Which.Should().NotBeOfType<InfrastructureException>();
    }

    [Fact]
    public async Task GIVEN_TransactionEndedUnderneath_WHEN_RollingBack_THEN_ItDoesNotThrow()
    {
        using var scope = _provider.CreateScope();
        var unitOfWork = (EfUnitOfWork)scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.OpenTransactionAsync();

        unitOfWork.Session!.GetDbTransaction().Rollback();

        var rollback = () => unitOfWork.RollbackAsync();

        // A rollback runs in the failure path, so anything it throws replaces the failure that put
        // it there. There is nothing left to roll back, which is the outcome a rollback wanted.
        await rollback.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GIVEN_RolledBackChanges_WHEN_ReadingAgainInTheSameScope_THEN_TheOldValuesComeBack()
    {
        var id = Guid.NewGuid();

        using (var seed = _provider.CreateScope())
        {
            var repository = seed.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();
            var unitOfWork = seed.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.OpenTransactionAsync();
            await repository.InsertAsync(new OrderAggregate(id, "Bob", 10m), CancellationToken.None);
            await unitOfWork.CommitAsync();
        }

        using var scope = _provider.CreateScope();
        var orders = scope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate, Guid>>();
        var work = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await work.OpenTransactionAsync();
        var order = await orders.FindByIdAsync(id, CancellationToken.None);
        order!.Complete();
        await orders.UpdateAsync(order, CancellationToken.None);
        await work.RollbackAsync();

        var reread = await orders.FindByIdAsync(id, CancellationToken.None);

        // The row went back to Created. A context still tracking the completed instance would hand
        // that one back instead — the query finds the row, and the identity map answers with the
        // copy it already holds.
        reread!.Status.Should().Be(OrderStatus.Created);
    }

    [Fact]
    public async Task GIVEN_NoTransactionWasOpened_WHEN_RollingBack_THEN_ItSaysSo()
    {
        using var scope = _provider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var rollback = () => unitOfWork.RollbackAsync();

        // A mistake in the calling code, and a different thing from a transaction already ended.
        await rollback.Should().ThrowAsync<InfrastructureException>()
            .WithMessage("Transaction was not initialized.");
    }
}
