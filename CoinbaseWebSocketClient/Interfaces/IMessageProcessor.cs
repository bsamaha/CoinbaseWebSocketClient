using System.Threading.Tasks;

namespace CoinbaseWebSocketClient.Interfaces
{
    public interface IMessageProcessor
    {
        Task ProcessReceivedMessage(string message, string productId);
    }
}