namespace BytLabs.Domain.Audit
{
    public class AuditInfo
    {
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}