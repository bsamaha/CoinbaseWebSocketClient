using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Configuration;
using CoinbaseWebSocketClient.Models;
using CoinbaseWebSocketClient.Utilities;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Services
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IKafkaProducer _kafkaProducer;

        public MessageProcessor(ILogger<MessageProcessor> logger, IKafkaProducer kafkaProducer)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        public async Task ProcessReceivedMessage(string message, string productId)
        {
            _logger.LogInformation($"Processing received message for {productId}: {message.Substring(0, Math.Min(100, message.Length))}...");
            try
            {
                var jsonDocument = JsonDocument.Parse(message);
                var root = jsonDocument.RootElement;

                if (root.TryGetProperty("channel", out var channelProperty))
                {
                    switch (channelProperty.GetString())
                    {
                        case "candles":
                            if (root.TryGetProperty("events", out var eventsProperty) && eventsProperty.ValueKind == JsonValueKind.Array)
                            {
                                var firstEvent = eventsProperty[0];
                                if (firstEvent.TryGetProperty("candles", out var candlesProperty) && candlesProperty.ValueKind == JsonValueKind.Array)
                                {
                                    var firstCandle = candlesProperty[0];
                                    if (firstCandle.TryGetProperty("product_id", out var productIdProperty))
                                    {
                                        string messageProductId = productIdProperty.GetString();
                                        _logger.LogInformation($"Message product ID: {messageProductId}, Expected product ID: {productId}");
                                    }
                                }
                            }
                            _logger.LogInformation($"Producing message to Kafka for {productId}");
                            await _kafkaProducer.ProduceMessage(message);
                            break;
                        case "heartbeats":
                            ProcessHeartbeat(root);
                            break;
                        default:
                            _logger.LogInformation($"Received message for unknown channel: {channelProperty.GetString()}");
                            break;
                    }
                }
                else
                {
                    _logger.LogInformation($"Received message without channel property for {productId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message for {productId}: {ex.Message}");
            }
        }

        private void ProcessHeartbeat(JsonElement root)
        {
            if (root.TryGetProperty("events", out var eventsElement) && eventsElement.ValueKind == JsonValueKind.Array)
            {
                var firstEvent = eventsElement.EnumerateArray().FirstOrDefault();
                if (firstEvent.TryGetProperty("current_time", out var timeElement) &&
                    firstEvent.TryGetProperty("heartbeat_counter", out var counterElement))
                {
                    var time = timeElement.GetString();
                    var counter = counterElement.GetString();
                    _logger.LogInformation($"Heartbeat: Time={time}, Counter={counter}");
                }
            }
        }

        private async Task ProcessTicker(string message)
        {
            _logger.LogInformation($"Processing ticker message: {message}");
            await _kafkaProducer.ProduceMessage(message);
        }
    }
}