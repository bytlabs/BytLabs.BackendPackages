namespace BytLabs.Domain.Audit
{
    /// <summary>
    /// Represents the audit information for an entity, capturing details about its creation, update, and deletion.
    /// It stores information about the user who performed the action and the timestamp of the action.
    /// </summary>
    public abstract class EntityAuditInfo : IAuditableEntity
    {
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}