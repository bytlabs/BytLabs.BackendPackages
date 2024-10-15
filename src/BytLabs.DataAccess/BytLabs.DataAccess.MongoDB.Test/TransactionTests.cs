using BytLabs.Application;
using BytLabs.Application.CQS.Commands;
using BytLabs.DataAccess.MongDB;
using BytLabs.DataAccess.MongDB.Configuration;
using BytLabs.Multitenancy;
using BytLabs.Multitenancy.Resolvers;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BytLabs.DataAccess.MongoDB.Test;

public class TransactionTests
{
    private readonly IServiceScope _serviceScope;
    private readonly IMediator _mediator;

    public TransactionTests()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var provider = services.BuildServiceProvider();
        _serviceScope = provider.CreateScope();
        _mediator = _serviceScope.ServiceProvider.GetRequiredService<IMediator>();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddFakeLogging();
        services.AddMultitenancy()
                    .AddResolver<ITenantIdResolver>(provider => new ValueTenantIdResolver(new TenantId("bytlabs")));
        services.AddCQS([typeof(TransactionTests).Assembly], _ => { });
        services.AddMongoDatabase(new MongoDatabaseConfiguration
        {
            ConnectionString = "mongodb://root:local_dev@localhsot:27017?replicaSet=bytlabs-mongo-set",
            DatabaseName = "test",
            UseTransactions = true
        });
    }

    #region Sequential Commands Test

    public record FirstCommand : ICommand;
    public record SecondCommand : ICommand;

    public class FirstCommandHandler : ICommandHandler<FirstCommand>
    {
        public Task Handle(FirstCommand request, CancellationToken cancellationToken) => 
            Task.CompletedTask;
    }

    public class SecondCommandHandler : ICommandHandler<SecondCommand>
    {
        public Task Handle(SecondCommand request, CancellationToken cancellationToken) => 
            Task.CompletedTask;
    }

    [Fact]
    public async Task GIVEN_TwoSequentialCommands_WHEN_ExecutedInSameScope_THEN_ShouldNotThrowException()
    {
        // Arrange
        await _mediator.Send(new FirstCommand());

        // Act
        var act = () => _mediator.Send(new SecondCommand());

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Nested Commands Test

    public record OuterCommand : ICommand;
    public record NestedCommand : ICommand;

    public class OuterCommandHandler : ICommandHandler<OuterCommand>
    {
        private readonly IMediator _mediator;

        public OuterCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Handle(OuterCommand request, CancellationToken cancellationToken) =>
            _mediator.Send(new NestedCommand(), cancellationToken);
    }

    public class NestedCommandHandler : ICommandHandler<NestedCommand>
    {
        public Task Handle(NestedCommand request, CancellationToken cancellationToken) => 
            Task.CompletedTask;
    }

    [Fact]
    public async Task GIVEN_NestedCommand_WHEN_ExecutedWithinOuterCommand_THEN_ShouldNotThrowException()
    {
        // Act
        var act = () => _mediator.Send(new OuterCommand());

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion
}
