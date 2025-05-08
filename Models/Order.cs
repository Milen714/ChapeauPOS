namespace ChapeauPOS.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string? OrderStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ClosedAt { get; set; }
        public OrderItem? OrderItem { get; set; }
        public Table? Table { get; set; }
        public Employee? Employee { get; set; }
    }
}
