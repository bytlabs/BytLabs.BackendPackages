using BytLabs.Application.DataAccess;
using BytLabs.Application.Test.OrdersService.Application.Decorators;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BytLabs.Application.Test;

public class CommandQueryTests
{
    [Fact]
    public async Task GIVEN_CommandHandler_WHEN_MediatorProcessCommand_THEN_CommandHandlerShouldResolve()
    {
        //Arrange
        var orderRepository = new Mock<IRepository<OrderAggregate, Guid>>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCQS(new[] { typeof(ProcessOrderCommand).Assembly });
        serviceCollection.AddLogging();
        serviceCollection.AddScoped(provider => orderRepository.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var orderId = Guid.NewGuid();
        var order = new OrderAggregate(orderId, "John Doe", 1000);
        orderRepository.Setup(repo => repo.GetByIdAsync(orderId, CancellationToken.None)).ReturnsAsync(order); 

        //Act        
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var orderResult = await mediator.Send(new ProcessOrderCommand() { OrderId = orderId });

        //Assert
        Assert.Equal(orderId, orderResult.OrderId);
        Assert.Equal(OrderStatus.Processing, orderResult.Status);
    }

    [Fact]
    public async Task GIVEN_CommandHandlerAndDecorator_WHEN_MediatorProcessCommand_THEN_CommandHandlerShouldResolveWithDecorator()
    {
        //Arrange
        var orderRepository = new Mock<IRepository<OrderAggregate, Guid>>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCQS(new[] { typeof(GetOrderDetailsQuery).Assembly });
        serviceCollection.AddLogging();
        serviceCollection.AddScoped(provider => orderRepository.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var orderId = Guid.NewGuid();
        var order = new OrderAggregate(orderId, "John Doe", 1000);
        orderRepository.Setup(repo => repo.GetByIdAsync(orderId, CancellationToken.None)).ReturnsAsync(order);

        //Act        
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var orderDetails = await mediator.Send(new GetOrderDetailsQuery() { OrderId = orderId });

        //Assert
        Assert.Equal(orderId, orderDetails.Id);
        Assert.Equal("John Doe", orderDetails.CustomerName);
        Assert.Equal(1000, orderDetails.TotalAmount);
    }

    [Fact]
    public async Task GIVEN_QueryHandler_WHEN_MediatorProcessQuery_THEN_QueryHandlerShouldResolve()
    {
        //Arrange
        var orderRepository = new Mock<IRepository<OrderAggregate, Guid>>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCQS(new[] { typeof(ProcessOrderCommand).Assembly }, configuration =>{
            configuration.AddRequestPreProcessor<ExamplePreProcessorDecorator>();
        });
        serviceCollection.AddLogging();
        serviceCollection.AddScoped(provider => orderRepository.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var orderId = Guid.NewGuid();
        var order = new OrderAggregate(orderId, "John Doe", 1000);
        orderRepository.Setup(repo => repo.GetByIdAsync(orderId, CancellationToken.None)).ReturnsAsync(order);

        //Act        
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var command = new ProcessOrderCommand() { OrderId = orderId };
        var orderResult = await mediator.Send(command);

        //Assert
        Assert.Equal(orderId, orderResult.OrderId);
        Assert.Equal(OrderStatus.Processing, orderResult.Status);
        Assert.Equal(ExamplePreProcessorDecorator.ExampleDecorator_Name, command.ExamplePreProcessorDecorator);
    }
}