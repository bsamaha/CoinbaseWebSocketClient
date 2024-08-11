using System.Threading.Tasks;
using Confluent.Kafka;

namespace CoinbaseWebSocketClient.Interfaces
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(string topic, Message<string, string> message);
    }
}