namespace BytLabs.Application.Exceptions;

/// <summary>
/// Represents errors that occur during application execution.
/// Serves as the base class for all application-specific exceptions.
/// </summary>
public class ApplicationOperationException : Exception
{
    /// <summary>
    /// Gets or initializes the error code.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Gets or sets the property associated with the error.
    /// </summary>
    public string? Property { get; set; }

    /// <summary>
    /// Initializes a new instance of the ApplicationException class.
    /// </summary>
    /// <param name="message">The message that describes the error that caused the exception</param>
    public ApplicationOperationException(string message) : base(message)
    {

    }

    public ApplicationOperationException(string message, string? code, string? property) : base(message)
    {
        Code = code;
        Property = property;
    }
}