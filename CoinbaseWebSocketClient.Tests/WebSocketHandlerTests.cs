using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoinbaseWebSocketClient.Configuration;
using CoinbaseWebSocketClient.Services;
using CoinbaseWebSocketClient.Utilities;
using CoinbaseWebSocketClient.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace CoinbaseWebSocketClient.Tests
{
    public class WebSocketHandlerTests
    {
        private readonly Mock<ILogger<WebSocketHandler>> _loggerMock;
        private readonly Mock<IMessageProcessor> _messageProcessorMock;
        private readonly Mock<IConfig> _configMock;
        private readonly Mock<IJwtGenerator> _jwtGeneratorMock;
        private readonly Mock<IWebSocketClient> _webSocketMock;

        public WebSocketHandlerTests()
        {
            _loggerMock = new Mock<ILogger<WebSocketHandler>>();
            _messageProcessorMock = new Mock<IMessageProcessor>();
            _configMock = new Mock<IConfig>();
            _jwtGeneratorMock = new Mock<IJwtGenerator>();
            _webSocketMock = new Mock<IWebSocketClient>();

            _configMock.Setup(c => c.WebSocketUrl).Returns("wss://example.com");
            _configMock.Setup(c => c.ProductIds).Returns(new List<string> { "BTC-USD" });
            _configMock.Setup(c => c.Channel).Returns("ticker");
            _jwtGeneratorMock.Setup(j => j.GenerateJwt(It.IsAny<string>(), It.IsAny<string>())).Returns("fake-jwt-token");
        }

        [Fact]
        public async Task ConnectAndSubscribe_ShouldConnectAndSendSubscriptionMessage()
        {
            // Arrange
            _webSocketMock.Setup(w => w.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _webSocketMock.Setup(w => w.SendAsync(It.IsAny<ArraySegment<byte>>(), WebSocketMessageType.Text, true, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new WebSocketHandler(
                _loggerMock.Object,
                _messageProcessorMock.Object,
                _configMock.Object,
                _jwtGeneratorMock.Object,
                _webSocketMock.Object
            );

            // Act
            await handler.ConnectAndSubscribe("BTC-USD");

            // Assert
            _webSocketMock.Verify(w => w.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Once);
            _webSocketMock.Verify(w => w.SendAsync(It.IsAny<ArraySegment<byte>>(), WebSocketMessageType.Text, true, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}