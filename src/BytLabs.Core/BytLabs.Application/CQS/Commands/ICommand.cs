using MediatR;

namespace BytLabs.Application.CQS.Commands
{
    /// <summary>
    /// Base interface for all commands in the system.
    /// Serves as a marker interface for command identification.
    /// </summary>
    public interface ICommandBase
    {

    }

    /// <summary>
    /// Represents a command that does not return a value.
    /// Implements both IRequest from MediatR and ICommandBase for command handling.
    /// </summary>
    public interface ICommand : IRequest, ICommandBase
    {

    }

    /// <summary>
    /// Represents a command that returns a value of type TResult.
    /// Implements both IRequest{TResult} from MediatR and ICommandBase for command handling.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the command</typeparam>
    public interface ICommand<out TResult> : IRequest<TResult>, ICommandBase
    {

    }
}