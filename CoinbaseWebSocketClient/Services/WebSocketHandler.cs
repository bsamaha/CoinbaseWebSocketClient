using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Interfaces;
using CoinbaseWebSocketClient.Models;
using CoinbaseWebSocketClient.Configuration;
namespace CoinbaseWebSocketClient.Services
{
    public record WebSocketHandlerConfig(
        ILogger<WebSocketHandler> Logger,
        IMessageProcessor MessageProcessor,
        IConfig Config,
        IJwtGenerator JwtGenerator,
        IWebSocketClient WebSocket,
        string ProductId
    );

    public class WebSocketHandler
    {
        private readonly WebSocketHandlerConfig _config;
        private readonly RateLimiter _messageRateLimiter;
        private readonly byte[] _buffer;

        public WebSocketHandler(WebSocketHandlerConfig config)
        {
            _config = config;
            _messageRateLimiter = new RateLimiter(300, 30);
            _buffer = new byte[_config.Config.WebSocketBufferSize];
        }

        public async Task ConnectAndSubscribe()
        {
            try
            {
                await _config.WebSocket.ConnectAsync(new Uri(_config.Config.WebSocketUrl), CancellationToken.None);
                _config.Logger.LogInformation("Connected to WebSocket for product: {ProductId}", _config.ProductId);

                var jwt = _config.JwtGenerator.GenerateJwt(_config.Config.ApiKey, _config.Config.PrivateKey);
                _config.Logger.LogInformation("Generated JWT: {JWT}", jwt); // Log the full JWT

                var subscribeMessage = new SubscribeMessage(
                    new List<string> { _config.ProductId },
                    _config.Config.Channel,
                    jwt
                );
                var subscribeJson = JsonSerializer.Serialize(subscribeMessage);
                _config.Logger.LogInformation("Subscribe message: {SubscribeMessage}", subscribeJson); // Log the full subscribe message

                await _messageRateLimiter.WaitForTokenAsync(CancellationToken.None);
                await _config.WebSocket.SendAsync(Encoding.UTF8.GetBytes(subscribeJson), WebSocketMessageType.Text, true, CancellationToken.None);
                _config.Logger.LogInformation("Subscription message sent for product: {ProductId}", _config.ProductId);

                await ReceiveAcknowledgment();
            }
            catch (Exception ex)
            {
                _config.Logger.LogError(ex, "Error connecting to WebSocket for product: {ProductId}", _config.ProductId);
                throw;
            }
        }

        public async Task ReceiveMessages()
        {
            try
            {
                while (_config.WebSocket.State == WebSocketState.Open)
                {
                    var result = await _config.WebSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(_buffer, 0, result.Count);
                        await _config.MessageProcessor.ProcessReceivedMessage(message, _config.ProductId);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _config.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _config.Logger.LogError(ex, "Error receiving messages for {ProductId}", _config.ProductId);
            }
        }

        private async Task ReceiveAcknowledgment()
        {
            var result = await _config.WebSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var acknowledgment = Encoding.UTF8.GetString(_buffer, 0, result.Count);
                _config.Logger.LogInformation("Received subscription acknowledgment for {ProductId}: {Acknowledgment}", _config.ProductId, acknowledgment);
                
                // Parse the acknowledgment JSON
                var jsonDocument = JsonDocument.Parse(acknowledgment);
                if (jsonDocument.RootElement.TryGetProperty("type", out var typeElement) && typeElement.GetString() == "error")
                {
                    if (jsonDocument.RootElement.TryGetProperty("message", out var messageElement))
                    {
                        _config.Logger.LogError("Subscription error for {ProductId}: {ErrorMessage}", _config.ProductId, messageElement.GetString());
                    }
                }
            }
        }

        public async Task Unsubscribe()
        {
            var jwt = _config.JwtGenerator.GenerateJwt(_config.Config.ApiKey, _config.Config.PrivateKey);
            var unsubscribeMessage = new SubscribeMessage(new List<string> { _config.ProductId }, _config.Config.Channel, jwt)
            {
                Type = "unsubscribe"
            };
            var unsubscribeJson = JsonSerializer.Serialize(unsubscribeMessage);

            await _messageRateLimiter.WaitForTokenAsync(CancellationToken.None);
            await _config.WebSocket.SendAsync(Encoding.UTF8.GetBytes(unsubscribeJson), WebSocketMessageType.Text, true, CancellationToken.None);
            _config.Logger.LogInformation("Unsubscription message sent for product: {ProductId}", _config.ProductId);
        }
    }
}
