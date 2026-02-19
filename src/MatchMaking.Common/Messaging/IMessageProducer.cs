namespace MatchMaking.Common.Messaging;

public interface IMessageProducer<in TMessage>
{
	public Task<bool> Publish(TMessage message);
}