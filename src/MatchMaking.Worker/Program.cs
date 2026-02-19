using MatchMaking.Worker.Options;
using Microsoft.Extensions.DependencyInjection;
using MatchMaking.Worker;
using MatchMaking.Common.Options;
using MatchMaking.Common.Models;
using MatchMaking.Common.Extensions;
using MatchMaking.Worker.Events;
using StackExchange.Redis;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.WriteTo.Console()
	.CreateLogger();

builder.Logging.AddSerilog();

builder.Services.Configure<KafkaOptions>(
	builder.Configuration.GetSection("Messaging:Kafka"));
builder.Services.Configure<MatchOptions>(
	builder.Configuration.GetSection("Match"));
builder.Services.Configure<RedisOptions>(
	builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
{
	var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
	return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
});

builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddProducerMessaging<Match>();
builder.Services.AddConsumerMessaging<User, AddUserToMatchEvent>();

builder.Services.AddHostedService<WorkerEntrypoint>();

var host = builder.Build();
host.Run();