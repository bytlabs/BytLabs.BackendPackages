using MediatR.Pipeline;

namespace BytLabs.Application.Test.OrdersService.Application.Decorators;

public class ExamplePreProcessorDecorator : IRequestPreProcessor<ProcessOrderCommand>
{
    public static string ExampleDecorator_Name = "Test";

    public Task Process(ProcessOrderCommand request, CancellationToken cancellationToken)
    {
        request.ExamplePreProcessorDecorator = ExampleDecorator_Name;
        return Task.CompletedTask;
    }
}