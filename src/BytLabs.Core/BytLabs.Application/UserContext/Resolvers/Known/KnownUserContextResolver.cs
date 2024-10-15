using BytLabs.Application.UserContext.Resolvers;

/// <summary>
/// Provides a fallback resolver that always returns the unknown user identifier.
/// Used as the last resolver in the chain when no other resolvers can identify the user.
/// </summary>
/// <remarks>
/// This resolver ensures that the system always has a valid user context by returning
/// a known default value instead of null. It should typically be registered last in
/// the resolver chain.
/// </remarks>
public class UnknownUserContextResolver : IUserContextResolver
{
    /// <summary>
    /// Returns the unknown user identifier constant.
    /// </summary>
    /// <returns>The <see cref="KnownUserId.Unknown"/> constant value</returns>
    public string GetUserId()
    {
        return KnownUserId.Unknown;
    }
}