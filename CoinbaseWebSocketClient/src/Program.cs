using System;
using System.Threading.Tasks;
using CoinbaseWebSocketClient.Configuration;
using CoinbaseWebSocketClient.Services;
using CoinbaseWebSocketClient.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            using var serviceProvider = services.BuildServiceProvider();
            var webSocketManager = serviceProvider.GetRequiredService<WebSocketManager>();

            await webSocketManager.InitializeConnections();
            await webSocketManager.StartReceiving();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            services.AddSingleton<IConfig>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<Program>();
                var config = ConfigurationLoader.LoadConfiguration(logger);
                return new Config
                {
                    ApiKey = config.ApiKey,
                    PrivateKey = config.PrivateKey,
                    WebSocketUrl = config.WebSocketUrl,
                    ProductIds = config.ProductIds,
                    Channel = config.Channel,
                    WebSocketBufferSize = config.WebSocketBufferSize,
                    KafkaBootstrapServers = config.KafkaBootstrapServers,
                    KafkaTopic = config.KafkaTopic
                };
            });

            services.AddSingleton<IJwtGenerator, JwtGenerator>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddSingleton<IMessageProcessor, MessageProcessor>();
            services.AddSingleton<WebSocketManager>();
        }
    }
}