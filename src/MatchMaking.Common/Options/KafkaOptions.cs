namespace MatchMaking.Common.Options;

public sealed class KafkaOptions
{
	public required string Endpoint { get; init; } = default!;
	public required string GroupId { get; init; } = default!;
	public required string ProducerTopic { get; init; } = default!;
	public required string ConsumerTopic { get; init; } = default!;
}