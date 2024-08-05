using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Services
{
    public record WebSocketManagerConfig(
        ILogger<WebSocketManager> Logger,
        IMessageProcessor MessageProcessor,
        IConfig Config,
        IJwtGenerator JwtGenerator,
        ILoggerFactory LoggerFactory,
        Func<IWebSocketClient> WebSocketClientFactory
    );

    public class WebSocketManager
    {
        private readonly WebSocketManagerConfig _config;
        private readonly Dictionary<string, WebSocketHandler> _handlers = new();

        public WebSocketManager(WebSocketManagerConfig config)
        {
            _config = config;
        }

        public async Task InitializeConnections()
        {
            foreach (var productId in _config.Config.ProductIds)
            {
                var handler = new WebSocketHandler(new WebSocketHandlerConfig(
                    _config.LoggerFactory.CreateLogger<WebSocketHandler>(),
                    _config.MessageProcessor,
                    _config.Config,
                    _config.JwtGenerator,
                    _config.WebSocketClientFactory(),
                    productId
                ));
                _handlers[productId] = handler;
                await handler.ConnectAndSubscribe();
            }
        }

        public async Task StartReceiving()
        {
            var receiveTasks = new List<Task>();
            foreach (var handler in _handlers.Values)
            {
                receiveTasks.Add(handler.ReceiveMessages());
            }
            await Task.WhenAll(receiveTasks);
        }
    }
}