using BytLabs.Application.DynamicData;

namespace BytLabs.Api.Graphql.InputTypes
{
    /// <summary>
    /// Helper class to retrieve dynamic data filter object
    /// </summary>
    public sealed class FilterHavingDynamicData
    {
        public FilterHavingDynamicData(DataFilter data)
        {
            Data = data;
        }

        public DataFilter? Data { get; private set; }
    }
}
