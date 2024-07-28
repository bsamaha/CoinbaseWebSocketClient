using System;

namespace CoinbaseWebSocketClient.Models
{
    public class TickerMessage
    {
        public string Channel { get; set; } = "ticker";
        public string ClientId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int SequenceNum { get; set; }
        public TickerEvent[] Events { get; set; } = Array.Empty<TickerEvent>();
    }

    public class TickerEvent
    {
        public string Type { get; set; } = "";
        public Ticker[] Tickers { get; set; } = Array.Empty<Ticker>();
    }

    public class Ticker
    {
        public string Type { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string Price { get; set; } = "";
        public string Volume24H { get; set; } = "";
        public string Low24H { get; set; } = "";
        public string High24H { get; set; } = "";
        public string Low52W { get; set; } = "";
        public string High52W { get; set; } = "";
        public string PricePercentChg24H { get; set; } = "";
        public string BestBid { get; set; } = "";
        public string BestBidQuantity { get; set; } = "";
        public string BestAsk { get; set; } = "";
        public string BestAskQuantity { get; set; } = "";
    }
}