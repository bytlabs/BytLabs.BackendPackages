using MediatR;

namespace BytLabs.Application.CQS.Queries;

/// <summary>
/// Represents a handler for queries in the CQRS pattern.
/// Implements IRequestHandler from MediatR to process queries and return responses.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle</typeparam>
/// <typeparam name="TResponse">The type of response returned by the query</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
