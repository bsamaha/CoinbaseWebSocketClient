using System;
using System.Collections.Generic;
using System.Linq;
using CoinbaseWebSocketClient.Interfaces;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CoinbaseWebSocketClient.Configuration
{
    public class Config : IConfig
    {
        private readonly ILogger<Config> _logger;

        public Config(ILogger<Config> logger)
        {
            _logger = logger;
            LoadConfiguration();
        }

        private string _apiKey = "";
        private string _privateKey = "";
        private string _webSocketUrl = "wss://advanced-trade-ws.coinbase.com";
        private List<string> _productIds = new List<string> { "BTC-USD", "ETH-USD" };
        private string _channel = Constants.Channels.Candles;
        private int _webSocketBufferSize = 32768;

        public string ApiKey
        {
            get => _apiKey;
            set => _apiKey = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("ApiKey cannot be null or empty");
        }

        public string PrivateKey
        {
            get => _privateKey;
            set => _privateKey = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("PrivateKey cannot be null or empty");
        }

        public string WebSocketUrl
        {
            get => _webSocketUrl;
            set => _webSocketUrl = Uri.IsWellFormedUriString(value, UriKind.Absolute) ? value : throw new ArgumentException("Invalid WebSocketUrl");
        }

        public List<string> ProductIds
        {
            get => _productIds;
            set => _productIds = value?.Count > 0 ? value : throw new ArgumentException("ProductIds must contain at least one item");
        }

        public string Channel
        {
            get => _channel;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Channel cannot be null or empty");

                if (!IsValidChannel(value))
                    throw new ArgumentException($"Invalid channel: {value}");

                _channel = value;
            }
        }

        public int WebSocketBufferSize
        {
            get => _webSocketBufferSize;
            set => _webSocketBufferSize = value > 0 ? value : throw new ArgumentException("WebSocketBufferSize must be greater than 0");
        }

        private bool IsValidChannel(string channel)
        {
            return typeof(Constants.Channels).GetFields().Any(f => 
            {
                var value = f.GetValue(null);
                return value != null && value.ToString() == channel;
            });
        }

        private void LoadConfiguration()
        {
            ApiKey = Environment.GetEnvironmentVariable("COINBASE_API_KEY") ?? ApiKey;
            var privateKeyEnv = Environment.GetEnvironmentVariable("COINBASE_PRIVATE_KEY");
            if (privateKeyEnv != null)
            {
                PrivateKey = privateKeyEnv.Replace("\\n", "\n");
            }
            WebSocketUrl = Environment.GetEnvironmentVariable("COINBASE_WEBSOCKET_URL") ?? WebSocketUrl;
            ProductIds = Environment.GetEnvironmentVariable("COINBASE_PRODUCT_IDS")?.Split(',').ToList() ?? ProductIds;
            Channel = Environment.GetEnvironmentVariable("COINBASE_CHANNEL") ?? Channel;
            WebSocketBufferSize = int.Parse(Environment.GetEnvironmentVariable("WEBSOCKET_BUFFER_SIZE") ?? WebSocketBufferSize.ToString());

            LogConfiguration();
        }

        private void LogConfiguration()
        {
            _logger.LogInformation("Configuration loaded:");
            _logger.LogInformation($"API Key: {MaskString(ApiKey)}");
            _logger.LogInformation($"Private Key (first 20 chars): {PrivateKey.Substring(0, Math.Min(20, PrivateKey.Length))}");
            _logger.LogInformation($"WebSocket URL: {WebSocketUrl}");
            _logger.LogInformation($"Product IDs: {string.Join(", ", ProductIds)}");
            _logger.LogInformation($"Channel: {Channel}");
        }

        private string MaskString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return $"{value.Substring(0, 4)}...{value.Substring(value.Length - 4)}";
        }
    }
}