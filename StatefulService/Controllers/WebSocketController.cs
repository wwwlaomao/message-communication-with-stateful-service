using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StatefulService.Services;

namespace StatefulService.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;
    private readonly ExternalPartyRegistry _externalPartyRegistry;

    public WebSocketController(
        ILogger<WebSocketController> logger,
        ExternalPartyRegistry externalPartyRegistry)
    {
        _logger = logger;
        _externalPartyRegistry = externalPartyRegistry;
    }

    [HttpGet("uncoordinated/{id}")]
    public async Task GetAsync(
        string id, CancellationToken cancellationToken)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            _logger.LogInformation("Connection request from {id}", id);
            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _externalPartyRegistry.Add(id);
            await DummyProcessingAsync(webSocket, HttpContext.RequestAborted);
            _logger.LogInformation("Connection is closed");
            _externalPartyRegistry.Remove(id);
        }
        else
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            HttpContext.Response.ContentType = "text/plain";
            byte[] bytes = Encoding.UTF8.GetBytes("This endpoint is for web socket.");
            await HttpContext.Response.Body.WriteAsync(bytes, cancellationToken);
        }
    }

    private async Task DummyProcessingAsync(
        WebSocket webSocket, CancellationToken cancellationToken)
    {
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(10000, cancellationToken);
                await webSocket.SendAsync(
                    Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToString()),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken);
            }
            await webSocket.CloseAsync(
                WebSocketCloseStatus.Empty, null, cancellationToken);
        }
        catch (Exception ex) when (ex is OperationCanceledException)
        {
            _logger.LogDebug("The client is cancelled.");
        }
    }
}
