using BytLabs.Domain.Entities;
using BytLabs.Domain.ValueObjects;

namespace BytLabs.States.Domain
{
    public class Trigger : ValueObject
    {
        public Trigger(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            return new object[] { Name };
        }
    }
}
