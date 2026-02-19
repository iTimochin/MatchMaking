namespace MatchMaking.Worker.Options;

internal sealed class RedisOptions
{
	public required string ConnectionString { get; init; }
	public required int RetryCount { get; init; }
	public required int RetryDelayMilliseconds { get; init; }
}

