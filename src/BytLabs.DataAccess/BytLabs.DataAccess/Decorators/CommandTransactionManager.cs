using BytLabs.Application.CQS.Commands;
using BytLabs.Application.DataAccess;
using Microsoft.Extensions.Logging;

namespace BytLabs.DataAccess.Decorators;

/// <summary>
/// Defines the contract for managing command-level transaction lifecycle.
/// </summary>
internal interface ICommandTransactionManager
{
    /// <summary>
    /// Prepares the transaction environment before command execution.
    /// </summary>
    /// <param name="command">The command being executed.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <remarks>
    /// For the first command in a chain, this method creates a new transaction.
    /// For subsequent commands, it reuses the existing transaction.
    /// </remarks>
    Task BeforeRequest(
        ICommandBase command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles successful command completion by managing transaction state.
    /// </summary>
    /// <param name="command">The completed command.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <remarks>
    /// Commits the transaction only when all chained commands have completed successfully.
    /// </remarks>
    Task OnRequestFinished(
        ICommandBase command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles command execution failures by managing transaction rollback.
    /// </summary>
    /// <param name="command">The failed command.</param>
    /// <param name="error">The exception that occurred.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <remarks>
    /// Rolls back the transaction when an error occurs in any command within the chain.
    /// </remarks>
    Task OnRequestError(
        ICommandBase command,
        Exception error,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Manages transaction scopes at the DI scope level, supporting chained command execution.
/// </summary>
/// <remarks>
/// This manager ensures that:
/// - All operations within a command chain share the same transaction
/// - Nested commands are properly handled within the parent transaction
/// - Transactions are committed only when all commands complete successfully
/// - Any failure in the command chain triggers a complete rollback
/// 
/// Important: This implementation does not support concurrent command execution within the same scope.
/// </remarks>
internal class CommandTransactionManager(
    ILogger<CommandTransactionManager> logger,
    IUnitOfWork unitOfWork) : ICommandTransactionManager
{
    /// <summary>
    /// Stack of currently executing commands within the transaction scope.
    /// </summary>
    private readonly Stack<ICommandBase> _activeCommands = new();

    public async Task BeforeRequest(
        ICommandBase command,
        CancellationToken cancellationToken = default)
    {
        if (_activeCommands.Count == 0)
        {
            await unitOfWork.OpenTransactionAsync()!;
            logger.LogTrace("Transaction Started");
        }
        else
        {
            logger.LogTrace("Transaction Reused");
        }

        _activeCommands.Push(command);
    }

    public async Task OnRequestFinished(
        ICommandBase command,
        CancellationToken cancellationToken = default)
    {
        if ((_activeCommands.Count - 1) <= 0)
        {
            await unitOfWork.CommitAsync();
            _activeCommands.Pop();
            logger.LogTrace("Transaction Completed");
        }
        else
        {
            _activeCommands.Pop();
            logger.LogTrace("{commandCount} Commands left", _activeCommands.Count);
        }
    }

    public async Task OnRequestError(
        ICommandBase command,
        Exception error,
        CancellationToken cancellationToken = default)
    {
        _activeCommands.Pop();
        if (_activeCommands.Count == 0)
        {
            await unitOfWork.RollbackAsync()!;
            logger.LogTrace("Transaction Aborted. Error {EMessage}", error.Message);
        }
        else
        {
            logger.LogTrace("{CommandCount} Commands left", _activeCommands.Count);
        }
    }
}

