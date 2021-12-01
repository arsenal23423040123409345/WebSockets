using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WebSocketsServer.Models;
using WebSocketsServer.Services;

namespace WebSocketsServer.Middleware
{
    public class WebSocketConnectionHandler
    {
        private readonly RequestDelegate _nextRequestDelegate;
        private readonly WebSocketsOperationService _webSocketsOperationService;

        public WebSocketConnectionHandler(RequestDelegate nextRequestDelegate, WebSocketsOperationService webSocketsOperationService)
        {
            _nextRequestDelegate = nextRequestDelegate;
            _webSocketsOperationService = webSocketsOperationService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WriteRequestParams(context);

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                Console.WriteLine("WebSocket Connected");

                var connId = _webSocketsOperationService.AddSocket(webSocket);

                await SendConnIdAsync(webSocket, connId);

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
                await ReceiveMessageAsync(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine("Message received!");
                        Console.WriteLine($"Message: {Encoding.UTF8.GetString(buffer)}");

                        await RouteJsonMessageAsync(Encoding.UTF8.GetString(buffer));
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Received Close command!");

                        var id = _webSocketsOperationService
                            .GetSockets()
                            .FirstOrDefault(x => x.Value == webSocket)
                            .Key;

                        _webSocketsOperationService.GetSockets().TryRemove(id, out _);

                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    }
                });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
            }
            else
            {
                Console.WriteLine("Dealing with non-socket protocol, 2nd request delegate");

                await _nextRequestDelegate(context);
            }
        }

        #region Private

        private static void WriteRequestParams(HttpContext context)
        {
            Console.WriteLine($"Request method: {context.Request.Method}");
            Console.WriteLine($"Request protocol: {context.Request.Protocol}");

            if (context.Request.Headers.Any())
            {
                foreach (var (parameter, value) in context.Request.Headers)
                {
                    Console.WriteLine($"--> {parameter}: {value}");
                }
            }
        }

        private static async Task ReceiveMessageAsync(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handle)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                handle(result, buffer);
            }
        }

        private static Task SendConnIdAsync(WebSocket socket, string connId)
        {
            var buffer = Encoding.UTF8.GetBytes($"ConnectionId: {connId}");

            return socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task RouteJsonMessageAsync(string jsonMessage)
        {
            var objectMessage = JsonConvert.DeserializeObject<ClientMessage>(jsonMessage);

            if (objectMessage == null)
            {
                Console.WriteLine("Invalid message!");

                return;
            }

            if (Guid.TryParse(objectMessage.To, out _))
            {
                Console.WriteLine("Targeted");

                var (_, value) = _webSocketsOperationService
                    .GetSockets()
                    .FirstOrDefault(x => x.Key == objectMessage.To);

                if (value is { State: WebSocketState.Open })
                {
                    await value.SendAsync(Encoding.UTF8.GetBytes(objectMessage.Message), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    Console.WriteLine("Invalid receiver id!");
                }
            }
            else
            {
                Console.WriteLine("Broadcast");

                foreach (var (_, value) in _webSocketsOperationService.GetSockets())
                {
                    if (value.State == WebSocketState.Open)
                    {
                        await value.SendAsync(Encoding.UTF8.GetBytes(objectMessage.Message), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }

        }

        #endregion

    }
}
