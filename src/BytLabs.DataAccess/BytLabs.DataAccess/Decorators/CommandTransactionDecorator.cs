using BytLabs.Application.CQS.Commands;
using MediatR;

namespace BytLabs.DataAccess.Decorators;

/// <summary>
/// Implements a pipeline behavior that wraps command execution in a database transaction.
/// </summary>
/// <typeparam name="TRequest">The type of command being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the command handler.</typeparam>
/// <remarks>
/// This decorator ensures that all database operations within a command are either:
/// - Committed together upon successful execution
/// - Rolled back together if an error occurs
/// 
/// It supports nested commands by managing transaction scope at the outermost command level.
/// </remarks>
sealed class CommandTransactionDecorator<TRequest, TResponse>(
    ICommandTransactionManager commandTransactionManager) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response;
        try
        {
            await commandTransactionManager.BeforeRequest(request, cancellationToken);
            response = await next();
            await commandTransactionManager.OnRequestFinished(request, cancellationToken);
        }
        catch (Exception error)
        {
            await commandTransactionManager.OnRequestError(request, error, cancellationToken);
            throw;
        }

        return response;
    }
}
