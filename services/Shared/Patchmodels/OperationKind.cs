using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Koasta.Shared.PatchModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OperationKind
    {
        Update,
        Remove
    }
}
