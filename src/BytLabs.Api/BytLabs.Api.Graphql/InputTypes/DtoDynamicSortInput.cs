using BytLabs.Application.DynamicData;
using HotChocolate.Data.Sorting;

namespace BytLabs.Api.Graphql.InputTypes
{
    public class DtoDynamicSortInput<TDto> : InputObjectType<SortInput<TDto>> where TDto : class
    {
        protected override void Configure(IInputObjectTypeDescriptor<SortInput<TDto>> descriptor)
        {
            descriptor.Name(typeof(TDto).Name.Replace("Dto", "") + "SortInput");
            descriptor.BindFieldsImplicitly();
            descriptor.Field(x => x.By).Type<EnumType<SortOrder>>();
        }
    }

    public class DtoSortInput<TDto> : SortInputType<TDto>
    {
        protected override void Configure(ISortInputTypeDescriptor<TDto> descriptor)
        {
            descriptor.Name(typeof(TDto).Name.Replace("Dto", "") + "SortInput");
            descriptor.BindFieldsImplicitly();
        }
    }
}
