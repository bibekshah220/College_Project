namespace CafeManagementSystem.Models
{
    public enum TableStatus
    {
        Free,
        Occupied,
        Reserved
    }

    public class TableInfo
    {
        public int Id { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public TableStatus Status { get; set; } = TableStatus.Free;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}