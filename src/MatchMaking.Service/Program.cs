using MatchMaking.Common.Extensions;
using MatchMaking.Common.Messaging;
using MatchMaking.Common.Models;
using MatchMaking.Common.Options;
using MatchMaking.Service.Events;
using MatchMaking.Service.Options;
using MatchMaking.Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.WriteTo.Console()
	.CreateLogger();

builder.Logging.AddSerilog();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
	.AddCheck("self", () =>
		Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

builder.Services.Configure<KafkaOptions>(
	builder.Configuration.GetSection("Messaging:Kafka"));
builder.Services.Configure<ApiOptions>(
	builder.Configuration.GetSection("Api"));
builder.Services.Configure<RedisOptions>(
	builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
	var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
	return ConnectionMultiplexer.Connect(redisOptions.ConnectionString);
});


builder.Services.AddProducerMessaging<User>();
builder.Services.AddConsumerMessaging<Match, FulfilledMatchEvent>();

builder.Services.AddTransient<IMessageProducer<User>, KafkaProducer<User>>();
builder.Services.AddHostedService<KafkaConsumerBackgroundService>();

builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference((options, httpContext) =>
	{
		var isRunningInContainer = bool.TryParse(
			Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
			out var value) && value;
		var baseUrl = builder.Configuration["BaseUrl"];
		if (isRunningInContainer && !string.IsNullOrEmpty(baseUrl)) 
		{ 
			options.AddServer(baseUrl, "Current");
		}
	});
}

app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
