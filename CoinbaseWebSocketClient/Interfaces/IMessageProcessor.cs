using System.Threading.Tasks;

namespace CoinbaseWebSocketClient.Interfaces
{
    public interface IMessageProcessor
    {
        Task ProcessReceivedMessage(string receivedMessage);
    }
}