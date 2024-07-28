using System;

namespace CoinbaseWebSocketClient.Models
{
    public class UserMessage
    {
        public string Channel { get; set; } = "user";
        public string ClientId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int SequenceNum { get; set; }
        public UserEvent[] Events { get; set; } = Array.Empty<UserEvent>();
    }

    public class UserEvent
    {
        public string Type { get; set; } = "";
        public Order[] Orders { get; set; } = Array.Empty<Order>();
    }

    public class Order
    {
        public string AvgPrice { get; set; } = "";
        public string CancelReason { get; set; } = "";
        public string ClientOrderId { get; set; } = "";
        public string CompletionPercentage { get; set; } = "";
        public string ContractExpiryType { get; set; } = "";
        public string CumulativeQuantity { get; set; } = "";
        public string FilledValue { get; set; } = "";
        public string LeavesQuantity { get; set; } = "";
        public string LimitPrice { get; set; } = "";
        public string NumberOfFills { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string OrderSide { get; set; } = "";
        public string OrderType { get; set; } = "";
        public string OutstandingHoldAmount { get; set; } = "";
        public string PostOnly { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string ProductType { get; set; } = "";
        public string RejectReason { get; set; } = "";
        public string RetailPortfolioId { get; set; } = "";
        public string RiskManagedBy { get; set; } = "";
        public string Status { get; set; } = "";
        public string StopPrice { get; set; } = "";
        public string TimeInForce { get; set; } = "";
        public string TotalFees { get; set; } = "";
        public string TotalValueAfterFees { get; set; } = "";
        public string TriggerStatus { get; set; } = "";
        public DateTime CreationTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
    }
}