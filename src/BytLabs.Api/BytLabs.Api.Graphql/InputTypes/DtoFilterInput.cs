using BytLabs.Domain.DynamicData;
using HotChocolate.Data.Filters;

namespace BytLabs.Api.Graphql.InputTypes
{
    public class DtoFilterInput<TDto> : FilterInputType<TDto> where TDto : class
    {
        protected override void Configure(IFilterInputTypeDescriptor<TDto> descriptor)
        {
            descriptor.BindFieldsImplicitly();
            descriptor.Name($"{typeof(TDto).Name.Replace("Dto", "")}FilterInput");

            if (CheckIfImplementsInterface<TDto, IHaveDynamicData>())
            {
                descriptor.Field(nameof(IHaveDynamicData.Data).ToLower())
                    .Type<DataOperationFilterInputType>()
                    .MakeNullable();
            }
        }

        public static bool CheckIfImplementsInterface<TType, TInterface>()
        {
            return typeof(TInterface).IsInterface && typeof(TInterface).IsAssignableFrom(typeof(TType));
        }
    }
}
