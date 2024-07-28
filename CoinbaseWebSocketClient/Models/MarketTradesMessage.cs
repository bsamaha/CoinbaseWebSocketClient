using System;

namespace CoinbaseWebSocketClient.Models
{
    public class MarketTradesMessage
    {
        public string Channel { get; set; } = "market_trades";
        public string ClientId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int SequenceNum { get; set; }
        public MarketTradeEvent[] Events { get; set; } = Array.Empty<MarketTradeEvent>();
    }

    public class MarketTradeEvent
    {
        public string Type { get; set; } = "";
        public MarketTrade[] Trades { get; set; } = Array.Empty<MarketTrade>();
    }

    public class MarketTrade
    {
        public string TradeId { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string Price { get; set; } = "";
        public string Size { get; set; } = "";
        public string Side { get; set; } = "";
        public DateTime Time { get; set; }
    }
}