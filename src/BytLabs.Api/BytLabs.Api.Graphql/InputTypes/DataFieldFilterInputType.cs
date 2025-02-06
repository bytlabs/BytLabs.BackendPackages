
using BytLabs.Application.DynamicData;
using HotChocolate.Data.Filters;

namespace BytLabs.Api.Graphql.InputTypes
{
    public class DataFieldFilterInputType : FilterInputType<DataFieldFilter>
    {
        protected override void Configure(IFilterInputTypeDescriptor<DataFieldFilter> descriptor)
        {
            descriptor.Name("DataFieldFilterInput");
            descriptor.BindFieldsImplicitly()
                .AllowAnd(false)
                .AllowOr(false);
        }
    }
}
