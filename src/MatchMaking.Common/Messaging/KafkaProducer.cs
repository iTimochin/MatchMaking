using Confluent.Kafka;
using MatchMaking.Common.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MatchMaking.Common.Messaging;

public class KafkaProducer<TMessage>
	(IProducer<Null, TMessage> producer, IOptions<KafkaOptions> kafkaOptions) 
	: IMessageProducer<TMessage>
{
	private readonly IProducer<Null, TMessage> _producer = producer;
	private readonly KafkaOptions _kafkaOptions = kafkaOptions.Value;
	private static readonly ILogger Logger = Log.ForContext<KafkaProducer<TMessage>>();

	public async Task<bool> Publish(TMessage message)
	{
		var jsonMessage = JsonSerializer.Serialize(message);
		//Logger.Information($"Trying to sent following payload: {jsonMessage}");

		var result = await _producer.ProduceAsync(_kafkaOptions.ProducerTopic,
		  new Message<Null, TMessage> { Value = message });
		//Logger.Information($"Message was sent with status {result.Status} and payload: {jsonMessage}");

		return result.Status == PersistenceStatus.Persisted;
	}
}