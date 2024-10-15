namespace BytLabs.Application.Exceptions;

/// <summary>
/// Represents errors that occur during application execution.
/// Serves as the base class for all application-specific exceptions.
/// </summary>
public class ApplicationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ApplicationException class.
    /// </summary>
    /// <param name="message">The message that describes the error that caused the exception</param>
    public ApplicationException(string message) : base(message)
    {

    }
}