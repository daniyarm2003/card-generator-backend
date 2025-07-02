using System.Text.Json.Serialization;

namespace CardGeneratorBackend.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CardVariant
    {
        REGULAR, NEBULA
    }
}
