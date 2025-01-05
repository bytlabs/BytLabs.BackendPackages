using BytLabs.Application.CQS.Commands;
using BytLabs.Application.DataAccess;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using static BytLabs.DataAccess.MongoDB.Test.TransactionTests;

namespace BytLabs.DataAccess.MongoDB.Test;

public class RepositoryTests : TestsBase
{
    private readonly ServiceProvider _provider;

    public RepositoryTests()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _provider = services.BuildServiceProvider();
        
    }

    [Fact]
    public async Task GIVEN_CreateOrderCommand_WHEN_SentToMediatr_THEN_ShouldExecuteCommandSuccessfully()
    {
        // Arrange
        var serviceScope = _provider.CreateScope();
        var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
        var command = new CreateOrderCommand(Guid.NewGuid(), "John Doe", 100);        

        // Act
        var act = () => mediator.Send(command);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GIVEN_TwoCreateOrderCommand_WHEN_SentToDifferentScopes_THEN_ShouldExecuteCommandSuccessfully()
    {
        {
            var serviceScope1 = _provider.CreateScope();
            var mediator1 = serviceScope1.ServiceProvider.GetRequiredService<IMediator>();
            var command1 = new CreateOrderCommand(Guid.NewGuid(), "John Doe", 100);
            var act1 = async () =>
            {
                await mediator1.Send(command1);
                serviceScope1.Dispose();
            };
            await act1.Should().NotThrowAsync();
        }

        {
            var serviceScope2 = _provider.CreateScope();
            var mediator2 = serviceScope2.ServiceProvider.GetRequiredService<IMediator>();
            var command2 = new CreateOrderCommand(Guid.NewGuid(), "John Doe", 100);
            var act2 = () => mediator2.Send(command2);
            await act2.Should().NotThrowAsync();
        }
    }

    #region supporting classes
    public record CreateOrderCommand(Guid Id, string CustomerName, decimal TotalAmount) : ICommand;

    public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
    {
        private readonly IRepository<OrderAggregate, Guid> repository;

        public CreateOrderCommandHandler(IRepository<OrderAggregate, Guid> repository)
        {
            this.repository = repository;
        }

        public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {

            var order = new OrderAggregate(request.Id, request.CustomerName, request.TotalAmount);
            await repository.InsertAsync(order, cancellationToken);
        }
    }
    #endregion
}
