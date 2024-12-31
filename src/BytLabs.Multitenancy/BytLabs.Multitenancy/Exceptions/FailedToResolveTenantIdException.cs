namespace BytLabs.Multitenancy.Exceptions
{
    /// <summary>
    /// Exception thrown when tenant identification fails.
    /// </summary>
    public class FailedToResolveTenantIdException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToResolveTenantIdException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the failure.</param>
        public FailedToResolveTenantIdException(string message) : base(message) { }
    }
}

