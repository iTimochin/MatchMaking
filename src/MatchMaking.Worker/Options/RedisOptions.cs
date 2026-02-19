namespace MatchMaking.Worker.Options;

internal record RedisOptions
(
	string ConnectionString,
	int RetryCount,
	int RetryDelayMilliseconds
);
