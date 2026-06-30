namespace CafeManagementSystem.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty; // kg, liters, pcs
        public decimal QuantityInStock { get; set; }
        public decimal LowStockThreshold { get; set; }
    }
}