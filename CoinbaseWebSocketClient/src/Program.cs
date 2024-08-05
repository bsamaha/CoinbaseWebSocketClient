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
                var logger = sp.GetRequiredService<ILogger<Config>>();
                return new Config(logger);
            });

            services.AddSingleton<IJwtGenerator, JwtGenerator>();
            services.AddSingleton<IMessageProcessor, MessageProcessor>();
            services.AddTransient<IWebSocketClient, WebSocketClient>();
            services.AddSingleton<Func<IWebSocketClient>>(sp => () => sp.GetRequiredService<IWebSocketClient>());

            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<WebSocketManager>>();
                var messageProcessor = sp.GetRequiredService<IMessageProcessor>();
                var config = sp.GetRequiredService<IConfig>();
                var jwtGenerator = sp.GetRequiredService<IJwtGenerator>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var webSocketClientFactory = sp.GetRequiredService<Func<IWebSocketClient>>();

                return new WebSocketManagerConfig(
                    logger,
                    messageProcessor,
                    config,
                    jwtGenerator,
                    loggerFactory,
                    webSocketClientFactory
                );
            });

            services.AddSingleton<WebSocketManager>();
        }
    }
}