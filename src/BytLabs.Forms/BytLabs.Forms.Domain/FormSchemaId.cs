using BytLabs.Domain.Entities;
using BytLabs.Domain.ValueObjects;

namespace BytLabs.Forms.Domain
{
    public class FormSchemaId<TEntity, TEntityId> : ValueObject where TEntity : Entity<TEntityId>
    {
        public FormKey Key { get; private set; }
        public string Entity { get; private set; }

        public FormSchemaId(FormKey key)
        {
            Key = key;
            Entity = typeof(TEntity).Name;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            return new object[] { Entity, Key };
        }
    }

}
