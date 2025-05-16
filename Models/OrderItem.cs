namespace ChapeauPOS.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int TemporaryId { get; set; }
        public MenuItem MenuItem { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAccountedForQuantity 
        {
            get { return (MenuItem?.ItemPrice ?? 0) * Quantity; }
        }

        public OrderItemStatus OrderItemStatus { get; set; }
        // public CourseStatus CourseStatus { get; set; }

        public string CourseStatus
        {
            get
            {
                if (OrderItemStatus == OrderItemStatus.Ordered)
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

        public string Notes { get; set; }

        public OrderItem() 
        { 
        }


        public OrderItem(int orderItemId, MenuItem menuItem, int quantity, OrderItemStatus orderItemStatus,  string notes)
        {
            OrderItemId = orderItemId;
            MenuItem = menuItem;
            Quantity = quantity;
            OrderItemStatus = orderItemStatus;
            Notes = notes;
        }
        public void SetOrderItemTemporaryItemId(int index)
        {
            TemporaryId = index + 1;
        }
    }
}
