using BytLabs.Application.UserContext.Resolvers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BytLabs.Api.UserContextResolvers;

/// <summary>
/// Provides an implementation of <see cref="IUserContextResolver"/> that retrieves the user ID from the current HTTP context.
/// This class is specifically designed for web applications where user identification is based on the HTTP request's user principal.
/// </summary>
public class HttpUserContextResolver : IUserContextResolver
{
    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpUserContextResolver"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor to access the current HTTP context.</param>
    /// <remarks>
    /// The HTTP context accessor is used to extract the current user's identity from the HttpContext, which is populated by ASP.NET Core middleware based on the incoming request.
    /// </remarks>
    public HttpUserContextResolver(IHttpContextAccessor httpContextAccessor)
    {
        _contextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Retrieves the user ID from the HTTP context's User principal.
    /// </summary>
    /// <returns>
    /// The user identifier as a string. If the user ID cannot be determined (e.g., no user is authenticated), this method returns the value specified by <see cref="KnownUserId.Unknown"/>.
    /// </returns>
    /// <remarks>
    /// This method attempts to find the NameIdentifier claim in the User principal's claims. If the claim is not found or if there is no authenticated user,
    /// the method returns <see cref="KnownUserId.Unknown"/> indicating that the user is unknown.
    /// </remarks>
    public string? GetUserId()
    {
        Claim? subClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        return subClaim?.Value;
    }
}