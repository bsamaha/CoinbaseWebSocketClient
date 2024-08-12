using Microsoft.Extensions.Logging;
using Confluent.Kafka;

namespace CoinbaseWebSocketClient.Interfaces
{
    public interface IWebSocketHandlerConfig
    {
        ILogger Logger { get; }
        IMessageProcessor MessageProcessor { get; }
        IConfig Config { get; }
        IJwtGenerator JwtGenerator { get; }
        IWebSocketClient WebSocket { get; }
        string ProductId { get; }
        IKafkaProducer KafkaProducer { get; }
    }
}