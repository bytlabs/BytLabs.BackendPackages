using BytLabs.Domain.ValueObjects;

namespace BytLabs.States.Domain
{
    [Serializable]
    public sealed class Trigger : ValueObject
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
