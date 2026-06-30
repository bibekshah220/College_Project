namespace CafeManagementSystem.Models
{
    public enum OrderStatus
    {
        Pending,
        Preparing,
        Ready,
        Served,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public int TableInfoId { get; set; }
        public TableInfo? TableInfo { get; set; }

        public string? WaiterId { get; set; }
        public ApplicationUser? Waiter { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Bill? Bill { get; set; }
    }
}