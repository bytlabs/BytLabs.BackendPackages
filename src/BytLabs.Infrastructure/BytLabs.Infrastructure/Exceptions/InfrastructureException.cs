namespace BytLabs.Infrastructure.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs within the infrastructure layer.
    /// This exception is thrown when domain rules or invariants are violated.
    /// </summary>
    public class InfrastructureException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the InfrastructureException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error that caused the exception.</param>
        public InfrastructureException(string message) : base(message)
        {

        }
    }
}
