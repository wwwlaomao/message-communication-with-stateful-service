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
            await DummyProcessingAsync(webSocket, cancellationToken);
            _logger.LogInformation("Connection from {id} is closed", id);
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
            using CancellationTokenSource cts = new();
            byte[] buffer = new byte[1024 * 16];
            Task receivingTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseOutputAsync(
                            WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                        cts.Cancel();
                    }
                    else
                    {
                        await webSocket.SendAsync(buffer, result.MessageType, result.EndOfMessage, cancellationToken);
                    }
                }
            }, cancellationToken);
            while (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(
                    Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToString()),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken);
                await Task.Delay(10000, cts.Token);
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException)
        {
            _logger.LogDebug("The client is cancelled.");
        }
    }
}
