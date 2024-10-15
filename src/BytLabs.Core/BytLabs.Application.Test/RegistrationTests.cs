using BytLabs.Application.DataAccess;
using BytLabs.Application.Test.OrdersService.Application.Commands;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BytLabs.Application.Test;

public class RegistrationTests
{
    [Fact]
    public void GIVEN_CQRSConfig_WHEN_RegisterServices_THEN_ServiceShouldBuild()
    {
        var orderRepository = new Mock<IRepository<OrderAggregate, Guid>>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddCQS(new[] { typeof(ProcessOrderCommandHandler).Assembly });
        serviceCollection.AddScoped(provider => orderRepository.Object);
        serviceCollection.BuildServiceProvider(new ServiceProviderOptions()
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}