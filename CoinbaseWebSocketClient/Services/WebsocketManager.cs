using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Configuration;
using CoinbaseWebSocketClient.Utilities;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Services
{
    public class WebSocketManager
    {
        private readonly ILogger<WebSocketManager> _logger;
        private readonly IMessageProcessor _messageProcessor;
        private readonly IConfig _config;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly ILoggerFactory _loggerFactory;
        private readonly List<WebSocketHandler> _handlers = new List<WebSocketHandler>();
        private readonly IWebSocketClient _webSocketClient;

        public WebSocketManager(
            ILogger<WebSocketManager> logger,
            IMessageProcessor messageProcessor,
            IConfig config,
            IJwtGenerator jwtGenerator,
            ILoggerFactory loggerFactory,
            IWebSocketClient webSocketClient)
        {
            _logger = logger;
            _messageProcessor = messageProcessor;
            _config = config;
            _jwtGenerator = jwtGenerator;
            _loggerFactory = loggerFactory;
            _webSocketClient = webSocketClient;
        }

        public async Task InitializeConnections()
        {
            foreach (var productId in _config.ProductIds)
            {
                var handler = new WebSocketHandler(
                    _loggerFactory.CreateLogger<WebSocketHandler>(),
                    _messageProcessor,
                    _config,
                    _jwtGenerator,
                    _webSocketClient
                );
                _handlers.Add(handler);
                await handler.ConnectAndSubscribe(productId);
            }
        }

        public async Task StartReceiving()
        {
            var receiveTasks = _handlers.Select(h => h.ReceiveMessages()).ToList();
            await Task.WhenAll(receiveTasks);
        }
    }
}