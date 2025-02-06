
using BytLabs.Application.DynamicData;
using HotChocolate.Data.Filters;

namespace BytLabs.Api.Graphql.InputTypes
{
    public class DataFilterInputType : FilterInputType<DataFilter>
    {
        protected override void Configure(IFilterInputTypeDescriptor<DataFilter> descriptor)
        {
            descriptor.BindFieldsImplicitly()
                .AllowAnd(false)
                .AllowOr(false);
        }
    }
}
