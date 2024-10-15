using System.Security.Claims;
using BytLabs.Api.UserContextResolvers;
using BytLabs.Application;
using BytLabs.Application.UserContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

namespace BytLabs.Api.Test.UserContext;

public class UserContextProviderIntegrationTests
{
    [Fact]
    public void GIVEN_NameIdentifierInHttpContext_WHEN_ResolveUserId_THEN_ShouldReturnCorrectUserId()
    {
        // Arrange (Given)
        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        Mock<IHttpContextAccessor> httpContextAccessor = new Mock<IHttpContextAccessor>();

        // Setup HTTP Context with a User ID
        DefaultHttpContext httpContext = new DefaultHttpContext();
        ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[] {
                                                                     new Claim(ClaimTypes.NameIdentifier, "user123")
                                                                 });
        httpContext.User = new(claimsIdentity);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        services.AddSingleton(httpContextAccessor.Object);
        services.AddUserContextProviders()
            .AddResolver<HttpUserContextResolver>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserContextProvider userContextProvider = serviceProvider.GetRequiredService<IUserContextProvider>();

        // Act (When)
        string resolvedUserId = userContextProvider.GetUserId();

        // Assert (Then)
        Assert.Equal("user123", resolvedUserId);
    }

}
