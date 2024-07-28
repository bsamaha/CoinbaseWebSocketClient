using System;
using System.Collections.Generic;
using System.Linq;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Configuration
{
    public class Config : IConfig
    {
        public string ApiKey { get; set; } = Environment.GetEnvironmentVariable("COINBASE_API_KEY") ?? "";
        public string PrivateKey { get; set; } = Environment.GetEnvironmentVariable("COINBASE_PRIVATE_KEY") ?? "";
        public string WebSocketUrl { get; set; } = Environment.GetEnvironmentVariable("COINBASE_WEBSOCKET_URL") ?? "";
        public List<string> ProductIds { get; set; } = (Environment.GetEnvironmentVariable("COINBASE_PRODUCT_IDS") ?? "").Split(',').ToList();
        public string Channel { get; set; } = Environment.GetEnvironmentVariable("COINBASE_CHANNEL") ?? "";
        public int WebSocketBufferSize { get; set; } = int.Parse(Environment.GetEnvironmentVariable("WEBSOCKET_BUFFER_SIZE") ?? "32768");
        public string KafkaBootstrapServers { get; set; } = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "";
        public string KafkaTopic { get; set; } = Environment.GetEnvironmentVariable("KAFKA_TOPIC") ?? "";
        public string KafkaUsername { get; set; } = Environment.GetEnvironmentVariable("KAFKA_USERNAME") ?? "";
        public string KafkaPassword { get; set; } = Environment.GetEnvironmentVariable("KAFKA_PASSWORD") ?? "";
    }
}