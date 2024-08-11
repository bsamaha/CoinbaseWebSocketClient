using System.Collections.Generic;

namespace CoinbaseWebSocketClient.Interfaces
{
    public interface IConfig
    {
        string ApiKey { get; set; }
        string PrivateKey { get; set; }
        string WebSocketUrl { get; set; }
        List<string> ProductIds { get; set; }
        string Channel { get; set; }
        int WebSocketBufferSize { get; set; }
        string KafkaBootstrapServers { get; set; }
        string KafkaClientId { get; set; }
        string KafkaSecurityProtocol { get; set; }
        string KafkaSaslMechanism { get; set; }
        string KafkaSaslUsername { get; set; }
        string KafkaSaslPassword { get; set; }
    }
}