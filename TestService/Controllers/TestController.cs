using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace TestService.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public TestController(
        ILogger<TestController> logger,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet("broadcast/{id}")]
    public async Task<IActionResult> BroadcastMessageAsync(
        string id, CancellationToken cancellationToken)
    {
        Guid correlatedId = Guid.NewGuid();
        _logger.LogInformation("Broadcast message with corrected id {correlatedId} to {id}", correlatedId, id);
        await _publishEndpoint.Publish<Contracts.MessageToExternalParty>(
            new
            {
                Id = id
            },
            context =>
            {
                context.CorrelationId = correlatedId;
            },
            cancellationToken);
        return Ok();
    }
}
