using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Services
{
    public class WebSocketClient : IWebSocketClient
    {
        private ClientWebSocket _clientWebSocket;

        public WebSocketState State => _clientWebSocket?.State ?? WebSocketState.None;

        public WebSocketClient()
        {
            _clientWebSocket = new ClientWebSocket();
        }

        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            // Create a new ClientWebSocket instance for each connection
            _clientWebSocket = new ClientWebSocket();
            await _clientWebSocket.ConnectAsync(uri, cancellationToken);
        }

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            => _clientWebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
            => _clientWebSocket.ReceiveAsync(buffer, cancellationToken);

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            => _clientWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
    }
}