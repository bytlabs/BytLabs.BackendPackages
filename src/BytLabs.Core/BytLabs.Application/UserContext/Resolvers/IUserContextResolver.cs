namespace BytLabs.Application.UserContext.Resolvers;

/// <summary>
/// Defines a strategy for resolving user identity from a specific context.
/// Part of the chain of responsibility pattern for user identification.
/// </summary>
public interface IUserContextResolver
{
    /// <summary>
    /// Attempts to resolve the current user's identifier.
    /// </summary>
    /// <returns>The user ID if successfully resolved, or null if resolution fails</returns>
    string? GetUserId();
}