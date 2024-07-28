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
            _logger.LogInformation($"Received message: {receivedMessage}");
            try
            {
                await _kafkaProducer.ProduceMessage(receivedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to produce message to Kafka. Continuing processing.");
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