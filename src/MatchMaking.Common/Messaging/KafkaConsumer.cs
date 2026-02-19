using Confluent.Kafka;
using MatchMaking.Common.Messaging;
using MatchMaking.Common.Models;
using MatchMaking.Common.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace MatchMaking.Common.KafkaConsumer;

public class KafkaConsumer<TMessage, TEvent> : IMessageConsumer<TMessage, TEvent>
	where TEvent : IEvent<TMessage>, new()
{
	private readonly IConsumer<Ignore, TMessage> _consumer;
	private readonly IMediator _mediator;
	private readonly KafkaOptions _kafkaOptions;
	private static readonly ILogger Logger = Log.ForContext<KafkaConsumer<TMessage, TEvent>>();

	public KafkaConsumer(
		IConsumer<Ignore, TMessage> consumer,
		IOptions<KafkaOptions> kafkaOptions,
		IMediator mediator)
	{
		_consumer = consumer;
		_mediator = mediator;
		_kafkaOptions = kafkaOptions.Value;
	}

	public void StartConsuming(CancellationToken cancellationToken)
	{
		_consumer.Subscribe(_kafkaOptions.ConsumerTopic);
			
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var result = _consumer.Consume(cancellationToken);
				_ = _mediator.Publish(new TEvent() {  Payload = result.Message.Value }, cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
			Logger.Information("Closing Kafka consumer...");
			_consumer.Close();
		}
	}
}
