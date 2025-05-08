namespace ChapeauPOS.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int Quantity { get; set; }
        public string? MenuCourse { get; set; }
        public string? OrderItemStatus { get; set; }
        public string? Notes { get; set; }
    }
}
