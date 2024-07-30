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
        private readonly ILogger<KafkaProducer> _logger;
        private readonly IConfig _config;
        private bool _isInitialized = false;

        public KafkaProducer(ILogger<KafkaProducer> logger, IConfig config)
        {
            _logger = logger;
            _config = config;
            InitializeProducer();
        }

        private void InitializeProducer()
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _config.KafkaBootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.ScramSha256,
                SaslUsername = _config.KafkaUsername,
                SaslPassword = _config.KafkaPassword,
                Debug = _config.KafkaDebug
            };

            _logger.LogInformation($"Initializing Kafka producer with config: {System.Text.Json.JsonSerializer.Serialize(producerConfig)}");

            try
            {
                _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
                _isInitialized = true;
                _logger.LogInformation("Kafka producer initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Kafka producer");
                throw;
            }
        }

        public async Task ProduceMessage(string message)
        {
            if (!_isInitialized || _producer == null)
            {
                throw new InvalidOperationException("Kafka producer is not initialized.");
            }

            try
            {
                var result = await _producer.ProduceAsync(_config.KafkaTopic, new Message<Null, string> { Value = message });
                _logger.LogInformation($"Delivered message to {result.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}