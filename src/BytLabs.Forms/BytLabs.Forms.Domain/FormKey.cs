using BytLabs.Domain.ValueObjects;
using System.Text;
using Humanizer;
using System.Text.RegularExpressions;

namespace BytLabs.Forms.Domain
{
    public class FormKey : ValueObject
    {
        public FormKey(string value)
        {
            Validate(value);
            Value = value;
        }
        public string Value { get; }

        public void Validate(string value)
        {
            // Ensure the key is not empty
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Key cannot be empty.");
            }

            // Validate camelCase format
            if (!Regex.IsMatch(value, "^[a-z][a-zA-Z0-9]*$"))
            {
                throw new ArgumentException("Key must be in camelCase format, starting with a lowercase letter and containing only alphanumeric characters.");
            }

            // Check MongoDB-specific rules
            if (value.Contains('.') || value.Contains('$'))
            {
                throw new ArgumentException("Key cannot contain '.' or '$'.");
            }

            // Check the length constraint
            if (Encoding.UTF8.GetByteCount(value) >= 1024)
            {
                throw new ArgumentException("Key must be shorter than 1024 bytes.");
            }
        }

        public static string NewKey(string key)
        {
            return key.Dehumanize().Camelize();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            return new object[] { Value };
        }

        public static implicit operator FormKey(string value) => new FormKey(value);
        public static implicit operator string(FormKey value) => value.Value;
    }

}
