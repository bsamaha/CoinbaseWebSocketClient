using System;
using System.Text.Json;
using System.Threading.Tasks;
using CoinbaseWebSocketClient.Configuration;
using CoinbaseWebSocketClient.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Tests
{
    public class MessageProcessorTests
    {
        private readonly Mock<ILogger<MessageProcessor>> _loggerMock;
        private readonly Mock<IKafkaProducer> _kafkaProducerMock;
        private readonly MessageProcessor _messageProcessor;

        public MessageProcessorTests()
        {
            _loggerMock = new Mock<ILogger<MessageProcessor>>();
            _kafkaProducerMock = new Mock<IKafkaProducer>();
            _messageProcessor = new MessageProcessor(_loggerMock.Object, _kafkaProducerMock.Object);
        }

        [Fact]
        public async Task ProcessReceivedMessage_ShouldHandleHeartbeatMessage()
        {
            // Arrange
            var heartbeatMessage = "{\"channel\":\"heartbeats\",\"events\":[{\"current_time\":\"2023-05-01T12:00:00Z\",\"heartbeat_counter\":\"1\"}]}";

            // Act
            await _messageProcessor.ProcessReceivedMessage(heartbeatMessage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Received message")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Detected channel: heartbeats")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Heartbeat: Time=2023-05-01T12:00:00Z, Counter=1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessReceivedMessage_ShouldHandleTickerMessage()
        {
            // Arrange
            var tickerMessage = "{\"type\":\"ticker\",\"sequence\":123,\"product_id\":\"BTC-USD\",\"price\":\"50000.00\",\"open_24h\":\"48000.00\",\"volume_24h\":\"1000.00\",\"low_24h\":\"47500.00\",\"high_24h\":\"51000.00\",\"volume_30d\":\"30000.00\",\"best_bid\":\"49990.00\",\"best_ask\":\"50010.00\",\"side\":\"buy\",\"time\":\"2023-04-14T12:00:00.000000Z\",\"trade_id\":456,\"last_size\":\"0.01\"}";

            _kafkaProducerMock.Setup(k => k.ProduceMessage(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            await _messageProcessor.ProcessReceivedMessage(tickerMessage);

            // Assert
            _kafkaProducerMock.Verify(k => k.ProduceMessage(It.Is<string>(s => s == tickerMessage)), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Processing ticker message")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}