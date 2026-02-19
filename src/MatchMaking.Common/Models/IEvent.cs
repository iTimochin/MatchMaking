namespace MatchMaking.Common.Models;

public interface IEvent<TPayload>
{
	public TPayload? Payload {  get; set; }
}