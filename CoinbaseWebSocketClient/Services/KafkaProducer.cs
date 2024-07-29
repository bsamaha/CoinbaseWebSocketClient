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
        private readonly string _topic;
        private bool _isInitialized = false;

        public KafkaProducer(ILogger<KafkaProducer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC") ?? throw new ArgumentNullException("KAFKA_TOPIC");
            
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? throw new ArgumentNullException("KAFKA_BOOTSTRAP_SERVERS"),
                SecurityProtocol = SecurityProtocol.SaslPlaintext,
                SaslMechanism = SaslMechanism.ScramSha256,
                SaslUsername = Environment.GetEnvironmentVariable("KAFKA_USERNAME") ?? throw new ArgumentNullException("KAFKA_USERNAME"),
                SaslPassword = Environment.GetEnvironmentVariable("KAFKA_PASSWORD") ?? throw new ArgumentNullException("KAFKA_PASSWORD"),
            };

            var kafkaDebug = Environment.GetEnvironmentVariable("KAFKA_DEBUG");
            if (!string.IsNullOrEmpty(kafkaDebug))
            {
                producerConfig.Debug = kafkaDebug;
            }

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
                _logger.LogInformation($"Attempting to produce message to Kafka topic '{_topic}': {message}");
                var deliveryResult = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
                _logger.LogInformation($"Delivered message to {deliveryResult.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}. Error Code: {e.Error.Code}. Message: {e.Error.ToString()}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error producing message to Kafka. Message: {ex.Message}. StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}