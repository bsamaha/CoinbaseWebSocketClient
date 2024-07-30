using System;
using System.Collections.Generic;
using System.Linq;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Configuration
{
    public class Config : IConfig
    {
        public string ApiKey { get; set; } = "";
        public string PrivateKey { get; set; } = "";
        public string WebSocketUrl { get; set; } = "";
        public List<string> ProductIds { get; set; } = new List<string>();
        public string Channel { get; set; } = "";
        public int WebSocketBufferSize { get; set; } = 32768;
        public string KafkaBootstrapServers { get; set; } = "";
        public string KafkaTopic { get; set; } = "";
        public string KafkaUsername { get; set; } = "";
        public string KafkaPassword { get; set; } = "";
        public string KafkaDebug { get; set; } = "";
    }
}