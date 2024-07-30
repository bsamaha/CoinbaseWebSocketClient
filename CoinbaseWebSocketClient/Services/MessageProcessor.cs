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

        public async Task ProcessReceivedMessage(string receivedMessage)
        {
            _logger.LogInformation("Received message: {Message}", receivedMessage);

            try
            {
                var jsonDocument = JsonDocument.Parse(receivedMessage);
                var root = jsonDocument.RootElement;

                if (root.TryGetProperty("type", out var typeProperty) && typeProperty.GetString() == "ticker")
                {
                    _logger.LogInformation("Processing ticker message");
                    await _kafkaProducer.ProduceMessage(receivedMessage);
                    _logger.LogInformation("Successfully published ticker message to Kafka");
                }
                else if (root.TryGetProperty("channel", out var channelProperty) && channelProperty.GetString() == "heartbeats")
                {
                    _logger.LogInformation("Detected channel: heartbeats");
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
                else if (root.TryGetProperty("events", out var eventsElement) && eventsElement.ValueKind == JsonValueKind.Array)
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
                else
                {
                    _logger.LogError("Unknown message type received");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing received message");
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