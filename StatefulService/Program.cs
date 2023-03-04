using System.Reflection;
using MassTransit;
using StatefulService.Consumers;
using StatefulService.Models;
using StatefulService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ExternalPartyRegistry>();
RabbitMQSettings rabbitMQSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumers(Assembly.GetEntryAssembly());
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMQSettings.Uri);
        cfg.ReceiveEndpoint($"start_request_{Guid.NewGuid()}", e =>
        {
            e.DiscardFaultedMessages();
            e.DiscardSkippedMessages();
            e.ConfigureConsumer<MessageToExternalPartyConsumer>(context);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseWebSockets();
app.MapControllers();

app.Run();
