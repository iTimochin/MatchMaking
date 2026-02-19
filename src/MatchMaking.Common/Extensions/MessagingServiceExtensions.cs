using Confluent.Kafka;
using MatchMaking.Common.KafkaConsumer;
using MatchMaking.Common.Messaging;
using MatchMaking.Common.Models;
using MatchMaking.Common.Options;
using MatchMaking.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MatchMaking.Common.Extensions;

public static class MessagingServiceExtensions
{
	public static IServiceCollection AddProducerMessaging<TProducerMessage>(this IServiceCollection services)
	{
		services.AddSingleton<IProducer<Null, TProducerMessage>>(sp =>
		{
			var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
			var config = new ProducerConfig
			{
				BootstrapServers = kafkaOptions.Endpoint
			};

			return new ProducerBuilder<Null, TProducerMessage>(config)
				.SetValueSerializer(new SystemTextJsonSerializer<TProducerMessage>())
				.Build();
		});

		services.AddTransient<IMessageProducer<TProducerMessage>, KafkaProducer<TProducerMessage>>();

		return services;
	}

	public static IServiceCollection AddConsumerMessaging<TConsumerMessage, TEvent>(this IServiceCollection services)
		where TEvent : IEvent<TConsumerMessage>, new()
	{
		services.AddSingleton(sp =>
		{
			var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
			return new ConsumerBuilder<Ignore, TConsumerMessage>(new ConsumerConfig
			{
				BootstrapServers = kafkaOptions.Endpoint,
				GroupId = kafkaOptions.GroupId,
				AutoOffsetReset = AutoOffsetReset.Earliest,
				EnableAutoCommit = false
			})
			.SetValueDeserializer(new SystemTextJsonDeserializer<TConsumerMessage>())
			.Build();
		});

		services.AddTransient<IMessageConsumer<TConsumerMessage, TEvent>, KafkaConsumer<TConsumerMessage, TEvent>>();

		return services;
	}
}
