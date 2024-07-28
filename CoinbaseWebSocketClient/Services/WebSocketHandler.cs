using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Configuration;
using CoinbaseWebSocketClient.Utilities;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Services
{

    public class WebSocketHandler
    {
        private readonly IWebSocketClient _webSocket;
        private readonly ILogger<WebSocketHandler> _logger;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IConfig _config;
        private readonly StringBuilder _messageBuffer = new StringBuilder();
        private readonly RateLimiter _messageRateLimiter;
        private readonly IJwtGenerator _jwtGenerator;
        private static readonly SemaphoreSlim ConnectionSemaphore = new SemaphoreSlim(5, 5);

        public WebSocketHandler(ILogger<WebSocketHandler> logger, IMessageProcessor messageProcessor, IConfig config, IJwtGenerator jwtGenerator, IWebSocketClient webSocket)
        {
            _logger = logger;
            _messageProcessor = messageProcessor;
            _config = config;
            _jwtGenerator = jwtGenerator;
            _webSocket = webSocket;
            _messageRateLimiter = new RateLimiter(300, 30); // 300 messages per 10 seconds
        }

        public async Task ConnectAndSubscribe(string? productId)
        {
            try
            {
                await ConnectionSemaphore.WaitAsync();
                var uri = new Uri(_config.WebSocketUrl);
                await _webSocket.ConnectAsync(uri, CancellationToken.None);
                _logger?.LogInformation("Connected to WebSocket for product: {ProductId}", productId ?? "All");

                _logger?.LogInformation("Generating JWT for product: {ProductId}", productId ?? "All");
                var jwt = _jwtGenerator.GenerateJwt(_config.ApiKey, _config.PrivateKey);
                _logger?.LogInformation("JWT generated successfully for product: {ProductId}", productId ?? "All");

                var subscribeMessage = new
                {
                    type = "subscribe",
                    product_ids = productId != null ? new[] { productId } : _config.ProductIds.ToArray(),
                    channel = _config.Channel,
                    signature = jwt
                };

                var subscribeJson = JsonSerializer.Serialize(subscribeMessage);
                _logger?.LogInformation("Subscription message: {Message}", subscribeJson);

                await _messageRateLimiter.WaitForTokenAsync(CancellationToken.None);
                _logger?.LogInformation("Sending subscription message for product: {ProductId}", productId ?? "All");
                await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(subscribeJson)), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger?.LogInformation("Subscription message sent for product: {ProductId}", productId ?? "All");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error connecting to WebSocket for product: {ProductId}", productId ?? "All");
                throw;
            }
            finally
            {
                ConnectionSemaphore.Release();
                // Ensure we don't exceed 5 connections per 5 seconds
                await Task.Delay(1000);
            }
        }

        public async Task ReceiveMessages()
        {
            var buffer = new byte[_config.WebSocketBufferSize];
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await _messageProcessor.ProcessReceivedMessage(message);
                }
            }
        }
    }
}