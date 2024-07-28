namespace CoinbaseWebSocketClient.Interfaces
{
    public interface IJwtGenerator
    {
        string GenerateJwt(string apiKey, string privateKey);
    }
}