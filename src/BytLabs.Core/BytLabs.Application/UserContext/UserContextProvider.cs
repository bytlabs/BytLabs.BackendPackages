using BytLabs.Application.UserContext.Resolvers;
using Microsoft.Extensions.Logging;

namespace BytLabs.Application.UserContext;

/// <summary>
/// Implements a composite pattern for user identification by aggregating multiple resolvers.
/// Provides fallback mechanisms when primary identification methods fail.
/// </summary>
public sealed class UserContextProvider : IUserContextProvider
{
    private readonly IEnumerable<IUserContextResolver> _resolvers;
    private readonly ILogger<UserContextProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the UserContextProvider.
    /// </summary>
    /// <param name="resolvers">Collection of resolvers for user identification</param>
    /// <exception cref="ArgumentNullException">Thrown when resolvers is null</exception>
    public UserContextProvider(IEnumerable<IUserContextResolver> resolvers,
        ILogger<UserContextProvider> logger)
    {
        _resolvers = resolvers ?? throw new ArgumentNullException(nameof(resolvers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Attempts to identify the current user using configured resolvers.
    /// </summary>
    /// <returns>
    /// The first valid user ID found, or KnownUserId.Unknown if no resolver succeeds
    /// </returns>
    public string GetUserId()
    {
        foreach (var resolver in _resolvers)
        {
            try
            {
                var userId = resolver.GetUserId();
                if (userId != null)
                {
                    return userId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception caught during user identification in {UserContextResolver}", resolver.GetType().Name);
            }
        }

        return KnownUserId.Unknown;
    }
}
