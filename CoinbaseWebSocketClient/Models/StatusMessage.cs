using System;

namespace CoinbaseWebSocketClient.Models
{
    public class StatusMessage
    {
        public string Channel { get; set; } = "status";
        public string ClientId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int SequenceNum { get; set; }
        public StatusEvent[] Events { get; set; } = Array.Empty<StatusEvent>();
    }

    public class StatusEvent
    {
        public string Type { get; set; } = "";
        public Product[] Products { get; set; } = Array.Empty<Product>();
    }

    public class Product
    {
        public string ProductType { get; set; } = "";
        public string Id { get; set; } = "";
        public string BaseCurrency { get; set; } = "";
        public string QuoteCurrency { get; set; } = "";
        public string BaseIncrement { get; set; } = "";
        public string QuoteIncrement { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusMessage { get; set; } = "";
        public string MinMarketFunds { get; set; } = "";
    }
}