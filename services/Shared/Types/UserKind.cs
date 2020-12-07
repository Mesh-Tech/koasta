using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Koasta.Shared.Types
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserType
    {
        User,
        Employee,
        Any
    }
}
