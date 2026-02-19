using MatchMaking.Common.Messaging;
using MatchMaking.Common.Models;
using MatchMaking.Worker.Events;
using MatchMaking.Worker.Models;
using MatchMaking.Worker.Options;
using MediatR;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace MatchMaking.Worker.Handlers;

internal sealed class AddUserToMatchHandler(
	IMessageProducer<Match> messageProducer,
	IConnectionMultiplexer redis,
	IOptions<MatchOptions> matchOptions,
	IOptions<RedisOptions> redisOptions)
 	: INotificationHandler<AddUserToMatchEvent>
{
	private static readonly ILogger Logger = Log.ForContext<AddUserToMatchHandler>();
	private readonly IMessageProducer<Match> _messageProducer = messageProducer;
	private readonly MatchOptions _matchOptions = matchOptions.Value;
	private readonly RedisOptions _redisOptions = redisOptions.Value;
	private readonly IDatabase _redis = redis.GetDatabase();

	public async Task Handle(AddUserToMatchEvent notification, CancellationToken cancellationToken)
	{
		var user = notification.Payload;

		// Get list of incomplete match keys
		var incompleteMatches = await GetAllIncompleteWorkerMatchesKeys();

		// Try to process an existing match
		foreach (var matchKey in incompleteMatches)
		{
			var success = await RetryAsync(
				async () =>
				{
					return await TryProcessMatch(matchKey, user!, cancellationToken);
				},
				maxRetries: _redisOptions.RetryCount,
				initialDelayMs: _redisOptions.RetryDelayMilliseconds,
				cancellationToken);

			if (success) 
			{ 
				return;
			}
		}

		// If no existing match could be locked → create new match
		await CreateNewWorkerMatch(user!);
	}

	private async Task<List<RedisKey>> GetAllIncompleteWorkerMatchesKeys() 
	{
		var allMatchKeys = await _redis.SetMembersAsync("matches:all");
		var incompleteMatches = new List<RedisKey>();

		foreach (var key in allMatchKeys)
		{
			var data = await _redis.StringGetAsync((string)key!);
			if (data.IsNullOrEmpty)
			{
				continue;
			}

			var workerMatchModel = JsonSerializer.Deserialize<WorkerMatch>(data!)!;
			if (!workerMatchModel.IsCompleted)
			{
				incompleteMatches.Add((string)key!);
			}
		}

		return incompleteMatches;
	}

	private async Task CreateNewWorkerMatch(User user)
	{
		var matchId = Guid.NewGuid().ToString();
		var newMatchKey = $"match:{matchId}";
		var workerMatch = new WorkerMatch()
		{
			Match = new Match(matchId, new List<string> { user.UserId }),
			IsCompleted = false,
			UserCount =  1,
		};

		await _redis.StringSetAsync(newMatchKey, JsonSerializer.Serialize(workerMatch));
		await _redis.SetAddAsync("matches:all", newMatchKey);
	}

	private async Task<bool> TryProcessMatch(RedisKey matchKey, User user, CancellationToken cancellationToken)
	{
		var lockKey = $"{matchKey}:lock";
		var lockValue = Guid.NewGuid().ToString();
		var lockAcquired = await _redis.LockTakeAsync(lockKey, lockValue, TimeSpan.FromSeconds(1));

		if (!lockAcquired)
			return false;

		try
		{
			var matchData = await _redis.StringGetAsync((string)matchKey!);
			var workerMatchModel = JsonSerializer.Deserialize<WorkerMatch>(matchData!)!;
			var workerMatchModelJson = JsonSerializer.Serialize(workerMatchModel)!;

			workerMatchModel.Match.UserIds.Add(user.UserId);
			workerMatchModel.UserCount++;

			if (workerMatchModel.UserCount >= _matchOptions.MaxAmountOfPlayersInMatch)
			{
				workerMatchModel.IsCompleted = true;
				await _messageProducer.Publish(workerMatchModel.Match);
				await _redis.KeyDeleteAsync(matchKey);
				await _redis.SetRemoveAsync("matches:all", (string)matchKey!);
				Logger.Information("Match fulfilled: {match}", workerMatchModelJson);
			}
			else
			{
				await _redis.StringSetAsync(matchKey, workerMatchModelJson);
			}

			return true;
		}
		finally
		{
			await _redis.LockReleaseAsync(lockKey, lockValue);
		}
	}

	private async Task<bool> RetryAsync(Func<Task<bool>> action, int maxRetries, int initialDelayMs, CancellationToken cancellationToken)
	{
		int delay = initialDelayMs;

		for (int attempt = 1; attempt <= maxRetries && !cancellationToken.IsCancellationRequested; attempt++)
		{
			if (await action()) 
			{ 
				return true;
			}

			await Task.Delay(delay, cancellationToken);
			// exponential backoff * 2
			delay *= 2;
			Logger.Warning("High load detected: race condition detected in writing, exponential backoff applied for {delay}", delay);
		}

		return false;
	}
}

