using BytLabs.Application.DynamicData;

namespace BytLabs.Hotchocolate.InputTypes
{
    public class DtoSortInput<TDto> : InputObjectType<SortInput<TDto>> where TDto : class
    {
        protected override void Configure(IInputObjectTypeDescriptor<SortInput<TDto>> descriptor)
        {
            descriptor.Name(typeof(TDto).Name.Replace("Dto", "") + "SortInput");
            descriptor.BindFieldsImplicitly();
            descriptor.Field(x => x.By).Type<EnumType<SortOrder>>();
        }
    }
}
