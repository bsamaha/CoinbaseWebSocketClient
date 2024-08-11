using System.Threading.Tasks;
using Confluent.Kafka;
using CoinbaseWebSocketClient.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoinbaseWebSocketClient.Services
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(IConfig config, ILogger<KafkaProducer> logger)
        {
            _logger = logger;
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config.KafkaBootstrapServers,
                ClientId = config.KafkaClientId,
                SecurityProtocol = Enum.Parse<SecurityProtocol>(config.KafkaSecurityProtocol),
                SaslMechanism = Enum.Parse<SaslMechanism>(config.KafkaSaslMechanism),
                SaslUsername = config.KafkaSaslUsername,
                SaslPassword = config.KafkaSaslPassword,
            };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async Task ProduceAsync(string topic, Message<string, string> message)
        {
            try
            {
                var deliveryResult = await _producer.ProduceAsync(topic, message);
                _logger.LogInformation($"Delivered message to {deliveryResult.TopicPartitionOffset}");
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
            }
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}