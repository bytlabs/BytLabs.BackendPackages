using BytLabs.Infrastructure.Exceptions;
using FluentAssertions;
using MongoDB.Driver;
using Moq;

namespace BytLabs.DataAccess.MongoDB.Test;

/// <summary>
/// What the unit of work does when a transaction is rolled back.
/// </summary>
/// <remarks>
/// The case that matters is a transaction the <b>server</b> has already ended. MongoDB aborts a
/// transaction that loses a write conflict, and a write conflict needs nothing more exotic than two
/// requests touching one document, so a command that fails can easily reach its rollback with no
/// transaction left to abort.
/// <para>
/// No server is needed to pin this: the session is a stand-in, and <c>IsInTransaction</c> is the one
/// thing it has to answer.
/// </para>
/// </remarks>
public class TransactionFailureTests
{
    private readonly Mock<IClientSessionHandle> _session = new();
    private readonly MongoUnitOfWork _unitOfWork;

    public TransactionFailureTests()
    {
        var client = new Mock<IMongoClient>();
        client
            .Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_session.Object);

        var database = new Mock<IMongoDatabase>();
        database.Setup(d => d.Client).Returns(client.Object);

        _unitOfWork = new MongoUnitOfWork(database.Object);
    }

    [Fact]
    public async Task GIVEN_ServerAlreadyEndedTheTransaction_WHEN_RollingBack_THEN_ItDoesNotThrow()
    {
        _session.Setup(s => s.IsInTransaction).Returns(false);
        await _unitOfWork.OpenTransactionAsync();

        var rollback = () => _unitOfWork.RollbackAsync();

        // Whatever the command failed on is the thing worth reporting. A rollback that throws here
        // replaces it, and the caller is sent after a transaction that was never the problem.
        await rollback.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GIVEN_ServerAlreadyEndedTheTransaction_WHEN_RollingBack_THEN_ItDoesNotTryToAbort()
    {
        _session.Setup(s => s.IsInTransaction).Returns(false);
        await _unitOfWork.OpenTransactionAsync();

        await _unitOfWork.RollbackAsync();

        // The driver refuses an abort once the transaction has ended.
        _session.Verify(s => s.AbortTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GIVEN_ALiveTransaction_WHEN_RollingBack_THEN_ItAborts()
    {
        _session.Setup(s => s.IsInTransaction).Returns(true);
        await _unitOfWork.OpenTransactionAsync();

        await _unitOfWork.RollbackAsync();

        _session.Verify(s => s.AbortTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GIVEN_NoTransactionWasOpened_WHEN_RollingBack_THEN_ItSaysSo()
    {
        var rollback = () => _unitOfWork.RollbackAsync();

        // Rolling back something that was never started is a mistake in the calling code, and a
        // different thing from a transaction the server ended. It stays loud.
        await rollback.Should().ThrowAsync<InfrastructureException>()
            .WithMessage("Transaction was not initialized.");
    }
}
