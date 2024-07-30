using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Configuration;

namespace CoinbaseWebSocketClient.Utilities
{
    public static class ConfigurationLoader
    {
        private const string ConfigFileName = "appsettings.json";

        public static Config LoadConfiguration(ILogger logger)
        {
            if (!File.Exists(ConfigFileName))
            {
                var errorMessage = $"Configuration file {ConfigFileName} not found. Current directory: {Directory.GetCurrentDirectory()}";
                logger.LogError(errorMessage);
                throw new FileNotFoundException(errorMessage);
            }

            var configJson = File.ReadAllText(ConfigFileName);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var config = JsonSerializer.Deserialize<Config>(configJson, options);
                if (config == null)
                {
                    throw new InvalidOperationException("Deserialized config is null.");
                }

                // Override with environment variables if they exist
                config.ApiKey = Environment.GetEnvironmentVariable("COINBASE_API_KEY") ?? config.ApiKey;
                config.PrivateKey = Environment.GetEnvironmentVariable("COINBASE_PRIVATE_KEY") ?? config.PrivateKey;
                config.WebSocketUrl = Environment.GetEnvironmentVariable("COINBASE_WEBSOCKET_URL") ?? config.WebSocketUrl;
                config.ProductIds = (Environment.GetEnvironmentVariable("COINBASE_PRODUCT_IDS") ?? string.Join(",", config.ProductIds)).Split(',').ToList();
                config.Channel = Environment.GetEnvironmentVariable("COINBASE_CHANNEL") ?? config.Channel;
                config.WebSocketBufferSize = int.Parse(Environment.GetEnvironmentVariable("WEBSOCKET_BUFFER_SIZE") ?? config.WebSocketBufferSize.ToString());
                config.KafkaBootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? config.KafkaBootstrapServers;
                config.KafkaTopic = Environment.GetEnvironmentVariable("KAFKA_TOPIC") ?? config.KafkaTopic;
                config.KafkaUsername = Environment.GetEnvironmentVariable("KAFKA_USERNAME") ?? config.KafkaUsername;
                config.KafkaPassword = Environment.GetEnvironmentVariable("KAFKA_PASSWORD") ?? config.KafkaPassword;
                config.KafkaDebug = Environment.GetEnvironmentVariable("KAFKA_DEBUG") ?? config.KafkaDebug;

                if (string.IsNullOrEmpty(config.KafkaUsername) || string.IsNullOrEmpty(config.KafkaPassword))
                {
                    logger.LogWarning("Kafka username or password is not set. Make sure to set them in appsettings.json or as environment variables.");
                }

                logger.LogInformation("Configuration loaded successfully.");
                return config;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "JSON deserialization error");
                throw;
            }
        }
    }
}