using BytLabs.Domain.Entities;

namespace BytLabs.Forms.Domain
{
    public abstract class FormSchemaAggregate<TEntity, TEntityId> : AggregateRootBase<FormSchemaId<TEntity, TEntityId>>
         where TEntity : Entity<TEntityId>
    {
        protected FormSchemaAggregate(FormSchemaId<TEntity, TEntityId> id, string name, string type, string schema) : base(id)
        {
            Name = name;
            Type = type;
            Schema = schema;
        }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public string Schema { get; private set; }
    }

}
