using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Jose;
using Microsoft.Extensions.Logging;
using CoinbaseWebSocketClient.Interfaces;

namespace CoinbaseWebSocketClient.Utilities
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly ILogger<JwtGenerator> _logger;
        private static readonly Random random = new Random();

        public JwtGenerator(ILogger<JwtGenerator> logger)
        {
            _logger = logger;
        }

        public string GenerateJwt(string apiKey, string privateKey)
        {
            _logger.LogDebug("Generating JWT for API Key: {ApiKey}", apiKey);
            string key = ParseKey(privateKey);
            var jwt = GenerateToken(apiKey, key);
            _logger.LogInformation("Generated JWT: {JWT}", jwt);
            return jwt;
        }

        private string GenerateToken(string apiKey, string secret)
        {
            var privateKeyBytes = Convert.FromBase64String(secret);
            using var key = ECDsa.Create();
            key.ImportECPrivateKey(privateKeyBytes, out _);

            var now = DateTimeOffset.UtcNow;
            var payload = new Dictionary<string, object>
            {
                { "sub", apiKey },
                { "iss", "coinbase-cloud" },
                { "nbf", now.ToUnixTimeSeconds() },
                { "exp", now.AddMinutes(1).ToUnixTimeSeconds() }
            };

            var extraHeaders = new Dictionary<string, object>
            {
                { "kid", apiKey },
                { "nonce", RandomHex(10) },
                { "typ", "JWT" }
            };

            return JWT.Encode(payload, key, JwsAlgorithm.ES256, extraHeaders);
        }

        private static string ParseKey(string key)
        {
            var keyLines = key.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            return string.Join("", keyLines[1..^1]);
        }

        private static string RandomHex(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            return BitConverter.ToString(buffer).Replace("-", "");
        }
    }
}