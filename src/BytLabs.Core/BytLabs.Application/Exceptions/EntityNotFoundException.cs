namespace BytLabs.Application.Exceptions;

/// <summary>
/// Represents an exception that occurs when a requested entity is not found.
/// Inherits from ApplicationException to maintain consistent exception handling.
/// </summary>
public class EntityNotFoundException : ApplicationOperationException
{
    /// <summary>
    /// Initializes a new instance of the EntityNotFoundException class.
    /// </summary>
    /// <param name="message">The message that describes the entity that could not be found</param>
    public EntityNotFoundException(string message) : base(message)
    {
        
    }
}