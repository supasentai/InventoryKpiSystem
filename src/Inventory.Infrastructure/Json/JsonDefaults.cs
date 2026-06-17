using System.Text.Json;

namespace Inventory.Infrastructure;

internal static class JsonDefaults
{
    public static JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };
    }
}
