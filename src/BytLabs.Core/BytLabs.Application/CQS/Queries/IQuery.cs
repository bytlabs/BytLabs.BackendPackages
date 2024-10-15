using MediatR;

namespace BytLabs.Application.CQS.Queries
{
    /// <summary>
    /// Represents a query in the CQRS pattern that returns a result of type TResult.
    /// Implements IRequest from MediatR to enable query handling through the mediator pattern.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the query will return</typeparam>
    public interface IQuery<TResult> : IRequest<TResult>
    {

    }
}