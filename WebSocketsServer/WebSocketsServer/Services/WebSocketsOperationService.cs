using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WebSocketsServer.Services
{
    public class WebSocketsOperationService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public ConcurrentDictionary<string, WebSocket> GetSockets()
            => _sockets;

        public string AddSocket(WebSocket socket)
        {
            var connId = Guid.NewGuid().ToString();

            _sockets.TryAdd(connId, socket);

            Console.WriteLine($"Connection added: {connId}");

            return connId;
        }

    }
}
