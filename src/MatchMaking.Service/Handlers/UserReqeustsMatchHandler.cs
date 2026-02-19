using MatchMaking.Common.Messaging;
using MatchMaking.Common.Models;
using MatchMaking.Service.Requests;
using MatchMaking.Service.Results;
using MediatR;
using StackExchange.Redis;

namespace MatchMaking.Service.Handlers;

internal sealed class UserReqeustsMatchHandler(IMessageProducer<User> messageProducer, IConnectionMultiplexer redis) 
	: IRequestHandler<UserMatchRequest, MatchSearchResult>
{
	private readonly IMessageProducer<User> _messageProducer = messageProducer;
	private readonly IDatabase _redis = redis.GetDatabase();

	public async Task<MatchSearchResult> Handle(UserMatchRequest request, CancellationToken cancellationToken)
	{
		var userId = request.User.UserId;

		var existingMatch = await _redis.StringGetAsync($"user:{userId}");
		if (!existingMatch.IsNullOrEmpty)
		{
			return new MatchAlreadyFound();
		}

		var searchKey = $"search:{userId}";

		var searchExists = await _redis.KeyExistsAsync(searchKey);
		if (searchExists)
		{
			return new MatchSearchInProgress();
		}

		await _redis.StringSetAsync(searchKey, string.Empty);

		var delivered = await _messageProducer.Publish(request.User);
		if (!delivered)
		{
			await _redis.KeyDeleteAsync(searchKey);
			return new MatchSearchFailed();
		}

		return new MatchSearchStarted();
	}
}
