using System.Text.Json;

namespace BytLabs.Domain.DynamicData
{
    public interface IHaveDynamicData
    {
        public JsonElement Data { get; }
    }
}
