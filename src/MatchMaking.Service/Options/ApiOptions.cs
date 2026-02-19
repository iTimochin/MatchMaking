namespace MatchMaking.Service.Options;

internal sealed class ApiOptions(int MatchThrottleInMs)
{
	public required int MatchThrottleInMs { get; init; }
}
