namespace ChapeauPOS.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public MenuItem MenuItem { get; set; }
        public int Quantity { get; set; }
        public MenuCourse MenuCourse { get; set; }
        public OrderItemStatus OrderItemStatus { get; set; }
        public string Notes { get; set; }

        public OrderItem() 
        { 
        }

        public OrderItem(int orderItemId, MenuItem menuItem, int quantity, MenuCourse menuCourse, OrderItemStatus orderItemStatus, string notes)
        {
            OrderItemId = orderItemId;
            MenuItem = menuItem;
            Quantity = quantity;
            MenuCourse = menuCourse;
            OrderItemStatus = orderItemStatus;
            Notes = notes;
        }
    }
}
