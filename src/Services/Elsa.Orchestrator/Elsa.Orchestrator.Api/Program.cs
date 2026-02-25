using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using Elsa.Orchestrator.Api.Consumers;
using Elsa.Orchestrator.Application;
using Elsa.Orchestrator.Application.Ports;
using Elsa.Orchestrator.Contracts;
using Elsa.Orchestrator.Infrastructure;
using MassTransit;
using RabbitMQ.Client;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Database context
builder.Services.AddDbContext<OrchestratorDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("OrchestratorDb"));
});

// Add Elsa (management + runtime + API)
builder.Services.AddElsa(elsa =>
{
    elsa.UseWorkflowManagement(management =>
    {
        management.UseEntityFrameworkCore(ef =>
        {
            ef.UsePostgreSql(builder.Configuration.GetConnectionString("ElsaDb")!);
        });
    });

    elsa.UseWorkflowRuntime(runtime =>
    {
        runtime.UseEntityFrameworkCore(ef =>
        {
            ef.UsePostgreSql(builder.Configuration.GetConnectionString("ElsaDb")!);
        });
    });

    elsa.UseWorkflowsApi();
});

// Add Application services
builder.Services.AddScoped<ISessionPullLedgerRepository, SessionPullLedgerRepository>();
builder.Services.AddScoped<EnsureSessionPullService>();

builder.Services.AddHttpClient<IF1SessionSource, F1SessionSource>();

// Add MassTransit (single bus)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SessionPullRequestedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        var vhost = builder.Configuration["RabbitMq:VirtualHost"] ?? "/";

        cfg.Host(host, vhost, h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        cfg.Message<SessionPullRequestedV1>(m => m.SetEntityName(RabbitMqTopology.CommandsExchange));
        cfg.Message<SessionInfoPulledV1>(m => m.SetEntityName(RabbitMqTopology.EventsExchange));

        cfg.Publish<SessionPullRequestedV1>(p => p.ExchangeType = ExchangeType.Topic);
        cfg.Publish<SessionInfoPulledV1>(p => p.ExchangeType = ExchangeType.Topic);

        cfg.ReceiveEndpoint(RabbitMqTopology.ElsaSessionPullQueue, e =>
        {
            e.ConfigureConsumeTopology = false;

            e.Bind(RabbitMqTopology.CommandsExchange, b =>
            {
                b.ExchangeType = ExchangeType.Topic;
                b.RoutingKey = RabbitMqTopology.Routing.SessionPullRequestedV1;
            });

            e.ConfigureConsumer<SessionPullRequestedConsumer>(context);
        });
    });

});

// Add Minimal API Endpoint
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi();
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<OrchestratorDbContext>();
        await db.Database.MigrateAsync();
    }

}
app.UseWorkflowsApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
