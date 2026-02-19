using MatchMaking.Common.Models;
using MediatR;

namespace MatchMaking.Service.Events;

internal class FulfilledMatchEvent() : IEvent<Match>, INotification
{
	public Match? Payload { get; set; }
}
