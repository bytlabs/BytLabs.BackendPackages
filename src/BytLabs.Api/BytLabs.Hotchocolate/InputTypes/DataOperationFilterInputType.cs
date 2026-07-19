using BytLabs.Application.DynamicData;

namespace BytLabs.Hotchocolate.InputTypes
{
    public class DataOperationFilterInputType : InputObjectType<DataOperationFilter>
    {
        protected override void Configure(IInputObjectTypeDescriptor<DataOperationFilter> descriptor)
        {
            descriptor.Name("DataOperationFilterInput");
            descriptor.BindFieldsImplicitly();
        }
    }
}
