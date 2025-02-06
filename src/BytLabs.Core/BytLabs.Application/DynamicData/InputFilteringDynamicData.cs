namespace BytLabs.Application.DynamicData
{
    public class InputFilteringDynamicData
    {
        public DataOperationFilter? Data { get; set; }
        public IEnumerable<InputFilteringDynamicData>? And { get; set; }
        public IEnumerable<InputFilteringDynamicData>? Or { get; set; }
    }
}
