using BytLabs.Domain.Entities;

namespace BytLabs.States.Domain
{
    public class Trigger : AggregateRootBase<string>
    {
        public Trigger(string id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
