namespace BytLabs.Application.DynamicData
{
    public class DataFilter
    {
        public IEnumerable<DataFieldFilter>? And { get; set; } = default!;
        public IEnumerable<DataFieldFilter>? Or { get; set; } = default!;
        public DataFieldFilter? Field { get; set; } = default!;
    }
}
