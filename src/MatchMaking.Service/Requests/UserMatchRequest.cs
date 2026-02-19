using MatchMaking.Common.Models;
using MatchMaking.Service.Results;
using MediatR;

namespace MatchMaking.Service.Requests;

internal record UserMatchRequest(User User) : IRequest<MatchSearchResult>;