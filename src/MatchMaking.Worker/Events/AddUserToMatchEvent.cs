using MatchMaking.Common.Models;
using MediatR;

namespace MatchMaking.Worker.Events;

internal class AddUserToMatchEvent() : IEvent<User>, INotification
{
	public User? Payload { get ; set ; }
}
