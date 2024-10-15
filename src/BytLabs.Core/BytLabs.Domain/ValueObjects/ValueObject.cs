namespace BytLabs.Domain.ValueObjects
{
    /// <summary>
    /// Abstract base class for implementing value objects in domain-driven design.
    /// Provides value-based equality comparison and hash code generation.
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        /// Gets the components of the value object that determine its equality.
        /// Must be implemented by derived classes to define their equality behavior.
        /// </summary>
        /// <returns>An enumerable of objects that together determine equality</returns>
        protected abstract IEnumerable<object> GetEqualityComponents();

        /// <summary>
        /// Equality operator overload for comparing two value objects
        /// </summary>
        /// <param name="one">The first value object to compare</param>
        /// <param name="two">The second value object to compare</param>
        /// <returns>True if the value objects are equal, false otherwise</returns>
        public static bool operator ==(ValueObject one, ValueObject two) =>
            EqualOperator(one, two);

        /// <summary>
        /// Inequality operator overload for comparing two value objects
        /// </summary>
        /// <param name="one">The first value object to compare</param>
        /// <param name="two">The second value object to compare</param>
        /// <returns>True if the value objects are not equal, false otherwise</returns>
        public static bool operator !=(ValueObject one, ValueObject two) =>
            NotEqualOperator(one, two);

        /// <summary>
        /// Helper method to implement equality comparison between two value objects
        /// </summary>
        /// <param name="left">The first value object to compare</param>
        /// <param name="right">The second value object to compare</param>
        /// <returns>True if the value objects are equal, false otherwise</returns>
        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
                return false;
            return ReferenceEquals(left, right) || left!.Equals(right);
        }

        /// <summary>
        /// Helper method to implement inequality comparison between two value objects
        /// </summary>
        /// <param name="left">The first value object to compare</param>
        /// <param name="right">The second value object to compare</param>
        /// <returns>True if the value objects are not equal, false otherwise</returns>
        protected static bool NotEqualOperator(ValueObject left, ValueObject right) =>
            !EqualOperator(left, right);

        /// <summary>
        /// Determines whether the specified object is equal to the current value object
        /// </summary>
        /// <param name="obj">The object to compare with the current value object</param>
        /// <returns>True if the specified object is equal to the current value object; otherwise, false</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;

            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <summary>
        /// Serves as the default hash function, combining the hash codes of all equality components
        /// </summary>
        /// <returns>A hash code for the current value object</returns>
        public override int GetHashCode() =>
            GetEqualityComponents()
                .Select(x => x.GetHashCode())
                .Aggregate((x, y) => x ^ y);
    }
}