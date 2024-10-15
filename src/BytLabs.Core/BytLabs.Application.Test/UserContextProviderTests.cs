using System.Security.Claims;
using BytLabs.Application.UserContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

namespace BytLabs.Application.Test;

[Trait("Category", "Unit")]
public class UserContextProviderIntegrationTests
{
    [Fact]
    public void GIVEN_NoUserContextResolver_WHEN_ResolveUserId_THEN_ShouldReturnUnkownUserId()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddUserContextProviders();
        services.AddLogging();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IUserContextProvider userContextProvider = serviceProvider.GetRequiredService<IUserContextProvider>();

        // Act
        string resolvedUserId = userContextProvider.GetUserId();

        // Assert
        Assert.Equal(KnownUserId.Unknown, resolvedUserId);
    }

    [Fact]
    public void GIVEN_ServiceCollection_WHEN_AddMultipleUserContextProviders_THEN_ShouldNotRegisterDuplicateServices()
    {
        // Arrange (Given)
        var services = new ServiceCollection();

        // Act (When)
        services.AddUserContextProviders(); // First invocation
        services.AddUserContextProviders(); // Second invocation

        // Get all registered services of specific types
        var userContextProviders = services.Where(s => s.ServiceType == typeof(IUserContextProvider)).ToList();

        // Assert (Then)
        Assert.Single(userContextProviders); // Should registered one implementation
    }
}
