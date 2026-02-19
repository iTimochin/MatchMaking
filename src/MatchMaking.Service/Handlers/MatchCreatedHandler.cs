using MatchMaking.Service.Events;
using MediatR;
using StackExchange.Redis;
using System.Text.Json;

namespace MatchMaking.Service.Handlers;

internal sealed class MatchCreatedHandler(IConnectionMultiplexer redis) : INotificationHandler<FulfilledMatchEvent>
{
	private readonly IDatabase _redis = redis.GetDatabase();

	public async Task Handle(FulfilledMatchEvent notification, CancellationToken cancellationToken)
	{
		var match = notification.Payload;
		var serialized = JsonSerializer.Serialize(match);

		await _redis.StringSetAsync(match?.MatchId, serialized);

		foreach (var userId in match!.UserIds)
		{
			await _redis.StringSetAsync($"user:{userId}", match.MatchId);
		}

		await _redis.StringSetAsync(match.MatchId, JsonSerializer.Serialize(match));
	}
}
