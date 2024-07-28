using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Services
{
    public class WebSocketClient : IWebSocketClient
    {
        private readonly ClientWebSocket _clientWebSocket = new ClientWebSocket();

        public WebSocketState State => _clientWebSocket.State;

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
            => _clientWebSocket.ConnectAsync(uri, cancellationToken);

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            => _clientWebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
            => _clientWebSocket.ReceiveAsync(buffer, cancellationToken);

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            => _clientWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
    }
}