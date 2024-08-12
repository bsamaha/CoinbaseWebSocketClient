using System;
using System.Threading.Tasks;
using CoinbaseWebSocketClient.Configuration;
using CoinbaseWebSocketClient.Services;
using CoinbaseWebSocketClient.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Interfaces;
using DotNetEnv;

namespace CoinbaseWebSocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                DotNetEnv.Env.Load();
            }

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
                var logger = sp.GetRequiredService<ILogger<Config>>();
                return new Config(logger);
            });

            services.AddSingleton<IJwtGenerator, JwtGenerator>();
            services.AddSingleton<IMessageProcessor, MessageProcessor>();
            services.AddTransient<IWebSocketClient, WebSocketClient>();
            services.AddSingleton<Func<IWebSocketClient>>(sp => () => sp.GetRequiredService<IWebSocketClient>());

            services.AddSingleton<IKafkaProducer, KafkaProducer>();

            services.AddSingleton<IWebSocketHandlerConfig>(sp =>
            {
                return new WebSocketHandlerConfig(
                    sp.GetRequiredService<ILogger<WebSocketHandler>>(),
                    sp.GetRequiredService<IMessageProcessor>(),
                    sp.GetRequiredService<IConfig>(),
                    sp.GetRequiredService<IJwtGenerator>(),
                    sp.GetRequiredService<Func<IWebSocketClient>>()(),
                    sp.GetRequiredService<IConfig>().ProductIds[0],
                    sp.GetRequiredService<IKafkaProducer>()
                );
            });

            services.AddSingleton<WebSocketManager>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<WebSocketManager>>();
                var messageProcessor = sp.GetRequiredService<IMessageProcessor>();
                var config = sp.GetRequiredService<IConfig>();
                var jwtGenerator = sp.GetRequiredService<IJwtGenerator>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var webSocketClientFactory = sp.GetRequiredService<Func<IWebSocketClient>>();
                var kafkaProducer = sp.GetRequiredService<IKafkaProducer>();

                return new WebSocketManager(new WebSocketManagerConfig(
                    logger,
                    messageProcessor,
                    config,
                    jwtGenerator,
                    loggerFactory,
                    webSocketClientFactory,
                    kafkaProducer
                ));
            });
        }
    }
}