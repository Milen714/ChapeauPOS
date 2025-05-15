namespace ChapeauPOS.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public MenuItem MenuItem { get; set; }
        public int Quantity { get; set; }
        public OrderItemStatus OrderItemStatus { get; set; }
        public string CourseStatus
        {
            get
            {
                if(OrderItemStatus == OrderItemStatus.Ordered)
                {
                    return "Ordered";
                }
                else if (OrderItemStatus == OrderItemStatus.Preparing)
                {
                    return "Preparing";
                }
                else
                {
                    return "Ready";
                }
            }
        }
        //public CourseStatus CourseStatus { get; set; }
        public string Notes { get; set; }

        public OrderItem() 
        { 
        }

        public OrderItem(int orderItemId, MenuItem menuItem, int quantity, OrderItemStatus orderItemStatus, string notes)
        {
            OrderItemId = orderItemId;
            MenuItem = menuItem;
            Quantity = quantity;
            OrderItemStatus = orderItemStatus;
            Notes = notes;
        }
    }
}
