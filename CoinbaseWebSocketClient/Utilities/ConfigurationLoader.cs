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
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var config = JsonSerializer.Deserialize<Config>(configJson, options);
                if (config == null)
                {
                    throw new InvalidOperationException("Deserialized config is null.");
                }

                // Load Kafka password from environment variable
                config.KafkaPassword = Environment.GetEnvironmentVariable("KAFKA_PASSWORD") ?? "";

                if (string.IsNullOrEmpty(config.KafkaUsername) || string.IsNullOrEmpty(config.KafkaPassword))
                {
                    logger.LogWarning("Kafka username or password is not set. Make sure to set the kafkaUsername in appsettings.json and the KAFKA_PASSWORD environment variable.");
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