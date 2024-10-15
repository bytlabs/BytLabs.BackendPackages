namespace BytLabs.Domain.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs within the domain layer.
    /// This exception is thrown when domain rules or invariants are violated.
    /// </summary>
    public class DomainException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the DomainException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error that caused the exception.</param>
        public DomainException(string message) : base(message)
        {
            
        }
    }
}
