using MatchMaking.Common.Messaging;
using MatchMaking.Common.Models;
using MatchMaking.Service.Events;

namespace MatchMaking.Service.Services;

internal sealed class KafkaConsumerBackgroundService
	(IMessageConsumer<Match, FulfilledMatchEvent> consumer) 
	: BackgroundService
{
	private readonly IMessageConsumer<Match, FulfilledMatchEvent> _consumer = consumer;

	protected override Task ExecuteAsync(CancellationToken cancellationToken)
	{
		return Task.Run(() =>
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				_consumer.StartConsuming(cancellationToken);
			}
		}, cancellationToken);
	}
}
