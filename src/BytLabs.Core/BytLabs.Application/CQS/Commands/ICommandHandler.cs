using MediatR;

namespace BytLabs.Application.CQS.Commands;

/// <summary>
/// Represents a handler for commands that return a response.
/// Implements IRequestHandler from MediatR for command processing.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
/// <typeparam name="TResponse">The type of response returned by the command</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : notnull
{
}

/// <summary>
/// Represents a handler for commands that do not return a response.
/// Implements IRequestHandler from MediatR for command processing.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}