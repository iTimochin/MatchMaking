using MatchMaking.Common.Models;
using MatchMaking.Service.Requests;
using MatchMaking.Service.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MatchMaking.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class MatchController(IMediator mediator) : ControllerBase
{
	private readonly IMediator _mediator = mediator;

	[HttpPost(Name = nameof(SearchForMatch))]
	public async Task<IActionResult> SearchForMatch([FromBody] User user)
	{
		var result = await _mediator.Send<MatchSearchResult>(new UserMatchRequest(user));
		return result switch
		{
			MatchSearchStarted => StatusCode(202),
			MatchSearchInProgress => StatusCode(409, "Search already in progress"),
			MatchAlreadyFound => StatusCode(409, "Match already found"),
			MatchSearchFailed => StatusCode(503),
			_ => StatusCode(500),
		};
	}

	[HttpGet("{userId}", Name = nameof(GetMatchByUserId))]
	public async Task<IActionResult> GetMatchByUserId(string userId)
	{
		var result = await _mediator.Send<MatchResult>(new FulfilledMatchRequest(userId));

		return result switch
		{
			MatchFound found => Ok(found.Payload),
			MatchThrottled => StatusCode(429, "Too Many Requests"),
			MatchNotFound => NotFound(),
			_ => StatusCode(500)
		};
	}
}
