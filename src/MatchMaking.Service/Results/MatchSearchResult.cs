namespace MatchMaking.Service.Results;

internal abstract record MatchSearchResult;
internal record MatchSearchStarted() : MatchSearchResult;
internal record MatchSearchInProgress() : MatchSearchResult;
internal record MatchAlreadyFound() : MatchSearchResult;
internal record MatchSearchFailed() : MatchSearchResult;