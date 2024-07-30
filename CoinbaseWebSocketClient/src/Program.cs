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

                if (string.IsNullOrEmpty(config.KafkaUsername) || string.IsNullOrEmpty(config.KafkaPassword))
                {
                    throw new InvalidOperationException("Kafka username and password must be set. Please check your environment variables.");
                }

                return new Config
                {
                    ApiKey = config.ApiKey,
                    PrivateKey = config.PrivateKey,
                    WebSocketUrl = config.WebSocketUrl,
                    ProductIds = config.ProductIds,
                    Channel = config.Channel,
                    WebSocketBufferSize = config.WebSocketBufferSize,
                    KafkaBootstrapServers = config.KafkaBootstrapServers,
                    KafkaTopic = config.KafkaTopic,
                    KafkaUsername = config.KafkaUsername,
                    KafkaPassword = config.KafkaPassword,
                    KafkaDebug = config.KafkaDebug
                };
            });

            services.AddSingleton<IJwtGenerator, JwtGenerator>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddSingleton<IMessageProcessor, MessageProcessor>();
            services.AddTransient<IWebSocketClient, WebSocketClient>();
            services.AddSingleton<Func<IWebSocketClient>>(sp => () => sp.GetRequiredService<IWebSocketClient>());
            services.AddSingleton<WebSocketManager>();
        }
    }
}