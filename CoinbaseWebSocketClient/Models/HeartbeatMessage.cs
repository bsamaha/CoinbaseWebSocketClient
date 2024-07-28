using System;

namespace CoinbaseWebSocketClient.Models
{
    public class HeartbeatMessage
    {
        public string Channel { get; set; } = "heartbeats";
        public string ClientId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int SequenceNum { get; set; }
        public HeartbeatEvent[] Events { get; set; } = Array.Empty<HeartbeatEvent>();
    }

    public class HeartbeatEvent
    {
        public string CurrentTime { get; set; } = "";
        public string HeartbeatCounter { get; set; } = "";
    }
}