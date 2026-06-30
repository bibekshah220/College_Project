namespace CafeManagementSystem.Models
{
    public enum PaymentStatus
    {
        Unpaid,
        Paid
    }

    public class Bill
    {
        public int Id { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}