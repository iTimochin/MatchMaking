using Confluent.Kafka;
using System.Text.Json;

namespace MatchMaking.Common.Serialization;

public class SystemTextJsonDeserializer<T> : IDeserializer<T>
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonDeserializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull || data.Length == 0)
        {
            return default!;
        }

        return JsonSerializer.Deserialize<T>(data, _options)!;
    }
}
