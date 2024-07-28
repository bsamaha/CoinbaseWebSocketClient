using System;

namespace CoinbaseWebSocketClient.Models
{
    public class CandlesMessage
    {
        public string Channel { get; set; } = "candles";
        public string ClientId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int SequenceNum { get; set; }
        public CandleEvent[] Events { get; set; } = Array.Empty<CandleEvent>();
    }

    public class CandleEvent
    {
        public string Type { get; set; } = "";
        public Candle[] Candles { get; set; } = Array.Empty<Candle>();
    }

    public class Candle
    {
        public string Start { get; set; } = "";
        public string High { get; set; } = "";
        public string Low { get; set; } = "";
        public string Open { get; set; } = "";
        public string Close { get; set; } = "";
        public string Volume { get; set; } = "";
        public string ProductId { get; set; } = "";
    }
}