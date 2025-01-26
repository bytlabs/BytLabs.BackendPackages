namespace BytLabs.Domain.Audit
{
    /// <summary>
    /// Represents an entity that supports auditing. Entities implementing this interface will have 
    /// an associated <see cref="EntityAuditInfo"/> capturing their creation, update, and deletion details.
    /// </summary>
    public interface IAuditableEntity
    {
        public AuditInfo AuditInfo { get; }
    }

}