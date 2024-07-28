using System.Collections.Generic;

namespace CoinbaseWebSocketClient.Interfaces
{
    public interface IConfig
    {
        string ApiKey { get; }
        string PrivateKey { get; }
        string WebSocketUrl { get; }
        List<string> ProductIds { get; }
        string Channel { get; }
        int WebSocketBufferSize { get; }
        string KafkaBootstrapServers { get; }
        string KafkaTopic { get; }
    }
}