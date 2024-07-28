using System.Threading.Tasks;

namespace CoinbaseWebSocketClient.Interfaces
{
    public interface IKafkaProducer
    {
        Task ProduceMessage(string message);
        // Add any other methods you need
    }
}