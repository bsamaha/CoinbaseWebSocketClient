using System.Text.Json;

using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Configuration;
using CoinbaseWebSocketClient.Interfaces;
using CoinbaseWebSocketClient.Models;
namespace CoinbaseWebSocketClient.Services
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly Dictionary<string, Func<JsonElement, string, Task>> _channelProcessors;

        public MessageProcessor(ILogger<MessageProcessor> logger)
        {
            _logger = logger;
            _channelProcessors = new Dictionary<string, Func<JsonElement, string, Task>>
            {
                { Constants.Channels.Candles, ProcessCandlesMessage },
                { Constants.Channels.Status, ProcessStatusMessage },
                { Constants.Channels.MarketTrades, ProcessMarketTradesMessage },
                { Constants.Channels.Heartbeats, ProcessHeartbeat }
            };
        }

        public async Task ProcessReceivedMessage(string message, string productId)
        {
            _logger.LogInformation($"Processing received message for {productId}: {message.Substring(0, Math.Min(100, message.Length))}...");
            try
            {
                var rootJson = ParseMessage(message);

                if (rootJson.TryGetProperty("channel", out var channelProperty))
                {
                    var channel = channelProperty.GetString();
                    _logger.LogInformation($"Received message for channel: {channel}");

                    if (channel != null && _channelProcessors.TryGetValue(channel, out var processor))
                    {
                        await processor(rootJson, productId);
                    }
                    else
                    {
                        _logger.LogWarning($"Received message for unhandled or null channel: {channel}");
                    }
                }
                else
                {
                    _logger.LogInformation($"Received message without channel property for {productId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message for {productId}: {ex.Message}");
            }
        }

        private JsonElement ParseMessage(string message)
        {
            var jsonDocument = JsonDocument.Parse(message);
            return jsonDocument.RootElement;
        }

        private Task ProcessCandlesMessage(JsonElement root, string productId)
        {
            if (!root.TryGetProperty("events", out var events))
            {
                _logger.LogWarning("No events property found in candle message");
                return Task.CompletedTask;
            }

            foreach (var eventElement in events.EnumerateArray())
            {
                ProcessCandleEvent(eventElement);
            }

            return Task.CompletedTask;
        }

        private void ProcessCandleEvent(JsonElement eventElement)
        {
            if (!TryGetEventType(eventElement, out var type) || type == null)
            {
                return;
            }

            if (type == "snapshot" || type == "update")
            {
                ProcessCandlesForEvent(eventElement, type);
            }
            else
            {
                _logger.LogWarning($"Unexpected event type in candle message: {type}");
            }
        }

        private bool TryGetEventType(JsonElement eventElement, out string? type)
        {
            if (!eventElement.TryGetProperty("type", out var typeProperty))
            {
                _logger.LogWarning("No type property found in candle event");
                type = null;
                return false;
            }

            type = typeProperty.GetString();
            return true;
        }

        private void ProcessCandlesForEvent(JsonElement eventElement, string type)
        {
            if (!eventElement.TryGetProperty("candles", out var candles))
            {
                _logger.LogWarning($"No candles property found in {type} event");
                return;
            }

            foreach (var candle in candles.EnumerateArray())
            {
                ProcessSingleCandle(candle);
            }
        }

        private void ProcessSingleCandle(JsonElement candle)
        {
            try
            {
                var candleData = new Candle
                {
                    Start = candle.GetProperty("start").GetString() ?? "",
                    High = candle.GetProperty("high").GetString() ?? "",
                    Low = candle.GetProperty("low").GetString() ?? "",
                    Open = candle.GetProperty("open").GetString() ?? "",
                    Close = candle.GetProperty("close").GetString() ?? "",
                    Volume = candle.GetProperty("volume").GetString() ?? "",
                    ProductId = candle.GetProperty("product_id").GetString() ?? ""
                };
                LogCandleData(candleData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing candle data: {ex.Message}");
            }
        }

        private void LogCandleData(Candle candleData)
        {
            _logger.LogInformation($"Candle: ProductId={candleData.ProductId}, Open={candleData.Open}, Close={candleData.Close}, High={candleData.High}, Low={candleData.Low}, Volume={candleData.Volume}");
        }

        private Task ProcessHeartbeat(JsonElement root, string productId)
        {
            var heartbeatMessage = JsonSerializer.Deserialize<HeartbeatMessage>(root.GetRawText());
            if (heartbeatMessage?.Events.FirstOrDefault() is HeartbeatEvent firstEvent)
            {
                _logger.LogInformation($"Heartbeat: Time={firstEvent.CurrentTime}, Counter={firstEvent.HeartbeatCounter}");
            }
            else
            {
                _logger.LogWarning("No heartbeat data found in the message");
            }
            return Task.CompletedTask;
        }

        private Task ProcessStatusMessage(JsonElement root, string productId)
        {
            var statusMessage = JsonSerializer.Deserialize<StatusMessage>(root.GetRawText());
            if (statusMessage?.Events.FirstOrDefault() is StatusEvent firstEvent)
            {
                foreach (var product in firstEvent.Products)
                {
                    _logger.LogInformation($"Status: ProductId={product.Id}, Status={product.Status}, StatusMessage={product.StatusMessage}");
                }
            }
            else
            {
                _logger.LogWarning("No status data found in the message");
            }
            return Task.CompletedTask;
        }

        private Task ProcessMarketTradesMessage(JsonElement root, string productId)
        {
            var marketTradesMessage = JsonSerializer.Deserialize<MarketTradesMessage>(root.GetRawText());
            if (marketTradesMessage?.Events.FirstOrDefault() is MarketTradeEvent firstEvent)
            {
                foreach (var trade in firstEvent.Trades)
                {
                    _logger.LogInformation($"Trade: ProductId={trade.ProductId}, Price={trade.Price}, Size={trade.Size}, Side={trade.Side}, Time={trade.Time}");
                }
            }
            else
            {
                _logger.LogWarning("No market trade data found in the message");
            }
            return Task.CompletedTask;
        }
    }
}