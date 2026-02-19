using MatchMaking.Common.Models;
using MatchMaking.Service.Options;
using MatchMaking.Service.Requests;
using MatchMaking.Service.Results;
using MediatR;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace MatchMaking.Service.Handlers;

internal sealed class GetExistingMatchHandler(
	IConnectionMultiplexer redis, 
	IOptions<ApiOptions> apiOptions) 
	: IRequestHandler<FulfilledMatchRequest, MatchResult>
{
	private readonly IDatabase _redis = redis.GetDatabase();
	private readonly ApiOptions _apiOptions = apiOptions.Value;

	public async Task<MatchResult> Handle(FulfilledMatchRequest request, CancellationToken cancellationToken)
	{
		var userId = request.UserId;
		var throttleKey = $"throttle:matchinfo:{userId}";

		var allowed = await _redis.StringSetAsync(
			throttleKey,
			string.Empty,
			TimeSpan.FromMilliseconds(_apiOptions.MatchThrottleInMs),
			When.NotExists);

		if (!allowed)
		{
			return new MatchThrottled();
		}

		var matchId = await _redis.StringGetAsync($"user:{userId}");
		if (matchId.IsNullOrEmpty)
		{
			return new MatchNotFound();
		}

		var matchData = await _redis.StringGetAsync(matchId.ToString());

		if (matchData.IsNullOrEmpty)
		{
			return new MatchNotFound();
		}

		return new MatchFound(JsonSerializer.Deserialize<Match>(matchData!));
	}
}
