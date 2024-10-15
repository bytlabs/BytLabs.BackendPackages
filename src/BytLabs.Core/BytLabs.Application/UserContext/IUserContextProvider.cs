namespace BytLabs.Application.UserContext;

/// <summary>
/// Defines a contract for accessing user identity information.
/// Implementations provide user identification from various sources like HTTP context,
/// message context, or other authentication mechanisms.
/// </summary>
public interface IUserContextProvider
{
    /// <summary>
    /// Retrieves the current user's identifier.
    /// </summary>
    /// <returns>The user ID if found, or null if no user can be identified</returns>
    string GetUserId();
}
