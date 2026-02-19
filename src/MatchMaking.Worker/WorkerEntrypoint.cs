using MatchMaking.Common.Messaging;
using MatchMaking.Common.Models;
using MatchMaking.Worker.Events;

namespace MatchMaking.Worker;

internal class WorkerEntrypoint
	(IMessageConsumer<User, AddUserToMatchEvent> messageConsumer) 
	: BackgroundService
{
	private readonly IMessageConsumer<User, AddUserToMatchEvent> _messageConsumer = messageConsumer;
	private static readonly ILogger Logger = Log.ForContext<WorkerEntrypoint>();

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		Logger.Information("Matchmaking worker started.");

		try
		{
			_messageConsumer.StartConsuming(stoppingToken);
		}
		catch (OperationCanceledException)
		{
			Logger.Information("Matchmaking worker stopping...");
		}
		catch (Exception ex)
		{
			Logger.Fatal(ex, "Matchmaking worker crashed.");
			throw;
		}

		return Task.CompletedTask;
	}
}
