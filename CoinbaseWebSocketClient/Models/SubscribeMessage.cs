using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoinbaseWebSocketClient.Models
{
    public class SubscribeMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "subscribe";

        [JsonPropertyName("product_ids")]
        public List<string> ProductIds { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("jwt")]
        public string Jwt { get; set; }

        public SubscribeMessage(List<string> productIds, string channel, string jwt)
        {
            ProductIds = productIds;
            Channel = channel;
            Jwt = jwt;
        }
    }
}