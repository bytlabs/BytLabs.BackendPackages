namespace BytLabs.Domain.Entities
{
    public interface ISoftDeletable
    {
        public bool IsDeleted { get; }
    }
}
