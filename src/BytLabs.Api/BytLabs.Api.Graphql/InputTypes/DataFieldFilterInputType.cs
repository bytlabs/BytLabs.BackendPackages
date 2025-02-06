
using BytLabs.Application.DynamicData;

namespace BytLabs.Api.Graphql.InputTypes
{
    public class DataFieldFilterInputType : InputObjectType<DataFieldFilter>
    {
        protected override void Configure(IInputObjectTypeDescriptor<DataFieldFilter> descriptor)
        {
            descriptor.Name("DataFieldFilterInput");
            descriptor.BindFieldsImplicitly();
        }
    }
}
