using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PlanetWars.Server.Helpers
{
    public static class HttpJson
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new StringEnumConverter() }
        };
    }
}