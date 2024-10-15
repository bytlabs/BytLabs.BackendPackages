/// <summary>
/// Defines well-known user identifiers for special system scenarios.
/// Provides a centralized source of truth for special-purpose user IDs.
/// </summary>
public static class KnownUserId
{
    /// <summary>
    /// Represents an unidentified or anonymous user.
    /// Used when user identity cannot be determined through normal authentication channels.
    /// </summary>
    /// <example>
    /// Common usage scenarios:
    /// - Public API endpoints
    /// - Unauthenticated requests
    /// - Failed authentication attempts
    /// </example>
    public const string Unknown = "unknown";

    /// <summary>
    /// Represents the system itself as an actor.
    /// Used to attribute actions performed automatically by the application.
    /// </summary>
    /// <example>
    /// Common usage scenarios:
    /// - Background jobs
    /// - Scheduled tasks
    /// - Automated maintenance
    /// </example>
    public const string System = "system";
}