using MassTransit;
using StatefulService.Services;

namespace StatefulService.Consumers;

public class MessageToExternalPartyConsumer : IConsumer<Contracts.MessageToExternalParty>
{
    private readonly ILogger<MessageToExternalPartyConsumer> _logger;
    private readonly ExternalPartyRegistry _externalPartyRegistry;

    public MessageToExternalPartyConsumer(
        ILogger<MessageToExternalPartyConsumer> logger,
        ExternalPartyRegistry externalPartyRegistry)
    {
        _logger = logger;
        _externalPartyRegistry = externalPartyRegistry;
    }

    public Task Consume(ConsumeContext<Contracts.MessageToExternalParty> context)
    {
        _logger.LogInformation(
            "Received MessageToExternalParty with correlated id {correlatedId} for: {Id}",
            context.CorrelationId, context.Message.Id);
        Models.ExternalPartyRecord? externalParty = _externalPartyRegistry.Get(
            context.Message.Id);
        if (externalParty == null)
        {
            return Task.CompletedTask;
        }
        _logger.LogInformation(
            "Processing MessageToExternalParty with correlated id {correlatedId} for: {Id}",
            context.CorrelationId, context.Message.Id);
        return Task.CompletedTask;
    }
}
