/// <summary>
/// Represents a unique identifier for a tenant in a multi-tenant system.
/// </summary>
public class TenantId
{
    /// <summary>
    /// Gets the string value of the tenant identifier.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantId"/> class.
    /// </summary>
    /// <param name="value">The tenant identifier value.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public TenantId(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}
