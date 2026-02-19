using Confluent.Kafka;
using System.Text.Json;

namespace MatchMaking.Common.Serialization;

public class SystemTextJsonSerializer<T> : ISerializer<T>
{
	public byte[] Serialize(T data, SerializationContext context)
		=> data is null ? Array.Empty<byte>() : JsonSerializer.SerializeToUtf8Bytes(data);
}
