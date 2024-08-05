using System;

namespace CoinbaseWebSocketClient.Configuration
{
    public static class Constants
    {
        public static class Channels
        {
            public const string Level2 = "level2";
            public const string User = "user";
            public const string Tickers = "ticker";
            public const string TickerBatch = "ticker_batch";
            public const string Status = "status";
            public const string MarketTrades = "market_trades";
            public const string Candles = "candles";
            public const string Heartbeats = "heartbeats";
        }
    }
}