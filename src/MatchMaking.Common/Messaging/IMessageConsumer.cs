using MatchMaking.Common.Models;

namespace MatchMaking.Common.Messaging;

public interface IMessageConsumer<TMessage, TEvent> 
	where TEvent : IEvent<TMessage>, new()
{
	public void StartConsuming(CancellationToken cancellationToken);
}
