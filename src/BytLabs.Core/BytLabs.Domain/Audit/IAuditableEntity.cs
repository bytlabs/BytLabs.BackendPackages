namespace BytLabs.Domain.Audit
{
    /// <summary>
    /// Represents an entity that supports auditing. Entities implementing this interface will have 
    /// an associated <see cref="EntityAuditInfo"/> capturing their creation, update, and deletion details.
    /// </summary>
    public interface IAuditableEntity
    {
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }

}