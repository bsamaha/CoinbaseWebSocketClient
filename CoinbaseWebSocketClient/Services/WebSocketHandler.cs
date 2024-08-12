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
using Confluent.Kafka; // Add this for Kafka support

namespace CoinbaseWebSocketClient.Services
{
    public class WebSocketHandler
    {
        private readonly IWebSocketHandlerConfig _config;
        private readonly RateLimiter _messageRateLimiter;
        private readonly byte[] _buffer;
        private const int MaxReconnectAttempts = 5;
        private const int InitialReconnectDelay = 1000; // 1 second

        public WebSocketHandler(IWebSocketHandlerConfig config)
        {
            _config = config;
            _messageRateLimiter = new RateLimiter(300, 30);
            _buffer = new byte[_config.Config.WebSocketBufferSize];
        }

        public async Task ConnectAndSubscribe()
        {
            await ConnectAndSubscribeWithRetry();
        }

        private async Task ConnectAndSubscribeWithRetry(int attemptCount = 0)
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
                if (attemptCount < MaxReconnectAttempts)
                {
                    int delay = (int)Math.Pow(2, attemptCount) * InitialReconnectDelay;
                    _config.Logger.LogInformation("Attempting to reconnect in {Delay}ms. Attempt {AttemptCount} of {MaxAttempts}", delay, attemptCount + 1, MaxReconnectAttempts);
                    await Task.Delay(delay);
                    await ConnectAndSubscribeWithRetry(attemptCount + 1);
                }
                else
                {
                    throw new Exception($"Failed to connect after {MaxReconnectAttempts} attempts", ex);
                }
            }
        }

        public async Task ReceiveMessages()
        {
            while (true)
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
                            
                            // Parse the message to determine its type and publish to Kafka
                            await PublishToKafka(message);
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

                _config.Logger.LogInformation("WebSocket connection closed. Attempting to reconnect...");
                await ConnectAndSubscribeWithRetry();
            }
        }

        private async Task PublishToKafka(string message)
        {
            try
            {
                var jsonDocument = JsonDocument.Parse(message);
                if (jsonDocument.RootElement.TryGetProperty("channel", out var channelElement))
                {
                    string topic = channelElement.GetString() switch
                    {
                        "candles" => "coinbase-candles",
                        "heartbeats" => "coinbase-heartbeats",
                        "ticker" => "coinbase-ticker",
                        "level2" => "coinbase-level2",
                        "user" => "coinbase-user",
                        "market_trades" => "coinbase-market-trades",
                        _ => "coinbase-unknown"
                    };

                    var kafkaMessage = new Message<string, string>
                    {
                        Key = _config.ProductId,
                        Value = message
                    };

                    await _config.KafkaProducer.ProduceAsync(topic, kafkaMessage);
                    _config.Logger.LogInformation("Published message to Kafka topic: {Topic}", topic);
                }
                else
                {
                    _config.Logger.LogWarning("Received message without a channel property: {Message}", message);
                }
            }
            catch (Exception ex)
            {
                _config.Logger.LogError(ex, "Error publishing message to Kafka: {Message}", message);
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