using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Configuration;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Services
{
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private IProducer<Null, string>? _producer;
        private readonly ILogger<KafkaProducer>? _logger;
        private readonly string? _topic;
        private bool _isInitialized = false;

        public KafkaProducer()
        {
            // Default constructor
        }

        public KafkaProducer(ILogger<KafkaProducer> logger, CoinbaseWebSocketClient.Configuration.Config config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _topic = config?.KafkaTopic ?? throw new ArgumentNullException(nameof(config.KafkaTopic));
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config.KafkaBootstrapServers ?? throw new ArgumentNullException(nameof(config.KafkaBootstrapServers))
            };
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            _isInitialized = true;
            _logger.LogInformation("Kafka producer initialized successfully.");
        }

        public Task ProduceMessage(string message)
        {
            if (!_isInitialized)
            {
                _logger.LogWarning("Attempted to produce message, but Kafka producer is not initialized.");
                return Task.CompletedTask;
            }

            try
            {
                var deliveryResult = _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
                _logger?.LogInformation($"Delivered message to {deliveryResult.Result.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger?.LogError($"Delivery failed: {e.Error.Reason}");
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}