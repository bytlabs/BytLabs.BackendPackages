namespace BytLabs.Multitenancy.Exceptions
{
    /// <summary>
    /// Exception thrown when tenant identification fails.
    /// </summary>
    public class FailedToResolveTenantId : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToResolveTenantId"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the failure.</param>
        public FailedToResolveTenantId(string message) : base(message) { }
    }
}
