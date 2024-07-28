using System;

namespace CoinbaseWebSocketClient.Models
{
    public class Level2Message
    {
        public string Channel { get; set; } = "l2_data";
        public string ClientId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int SequenceNum { get; set; }
        public Level2Event[] Events { get; set; } = Array.Empty<Level2Event>();
    }

    public class Level2Event
    {
        public string Type { get; set; } = "";
        public string ProductId { get; set; } = "";
        public Level2Update[] Updates { get; set; } = Array.Empty<Level2Update>();
    }

    public class Level2Update
    {
        public string Side { get; set; } = "";
        public DateTime EventTime { get; set; }
        public string PriceLevel { get; set; } = "";
        public string NewQuantity { get; set; } = "";
    }
}