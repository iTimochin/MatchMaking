using MatchMaking.Common.Models;

namespace MatchMaking.Worker.Models;

internal sealed class WorkerMatch
{
	public required Match Match { get; set; }
	public int UserCount { get; set; }
	public bool IsCompleted { get; set; }
}
