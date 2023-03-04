using System.Reflection;
using MassTransit;
using TestService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
RabbitMQSettings rabbitMQSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumers(Assembly.GetEntryAssembly());
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMQSettings.Uri);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
