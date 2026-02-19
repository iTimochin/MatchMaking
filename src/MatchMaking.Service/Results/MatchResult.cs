using MatchMaking.Common.Models;

namespace MatchMaking.Service.Results;

internal abstract record MatchResult;
internal record MatchFound(Match? Payload) : MatchResult;
internal record MatchThrottled() : MatchResult;
internal record MatchNotFound() : MatchResult;