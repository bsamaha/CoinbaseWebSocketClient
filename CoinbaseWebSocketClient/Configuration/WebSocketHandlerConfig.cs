using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Configuration
{
    public class WebSocketHandlerConfig : IWebSocketHandlerConfig
    {
        public ILogger Logger { get; }
        public IMessageProcessor MessageProcessor { get; }
        public IConfig Config { get; }
        public IJwtGenerator JwtGenerator { get; }
        public IWebSocketClient WebSocket { get; }
        public string ProductId { get; }
        public IKafkaProducer KafkaProducer { get; }

        public WebSocketHandlerConfig(
            ILogger logger,
            IMessageProcessor messageProcessor,
            IConfig config,
            IJwtGenerator jwtGenerator,
            IWebSocketClient webSocket,
            string productId,
            IKafkaProducer kafkaProducer)
        {
            Logger = logger;
            MessageProcessor = messageProcessor;
            Config = config;
            JwtGenerator = jwtGenerator;
            WebSocket = webSocket;
            ProductId = productId;
            KafkaProducer = kafkaProducer;
        }
    }
}