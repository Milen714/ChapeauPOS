namespace ChapeauPOS.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int TemporaryOrderId { get; set; }
        public Table Table { get; set; }
        public Employee Employee { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public TimeSpan RunningTime 
        { 
            get 
            { 
                return DateTime.Now - CreatedAt; 
            } 
        }
        public DateTime? ClosedAt { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public List<OrderItem> InterumOrderItems { get; set; } 
        public decimal TotalAmount
        {
            get {
                return TotalAmountToPay();
            }
        }

        public Order()
        {
            InterumOrderItems = new List<OrderItem>();
            OrderItems = new List<OrderItem>();
        }

        public Order(int orderID, Table table, Employee employee, DateTime createdAt, DateTime? closedAt)
        {
            OrderID = orderID;
            Table = table;
            Employee = employee;
            CreatedAt = createdAt;
            ClosedAt = closedAt;
            OrderItems = new List<OrderItem>();
            InterumOrderItems = new List<OrderItem>();
        }

        public Order(int orderID, Table table, Employee employee, DateTime createdAt, DateTime? closedAt, List<OrderItem> orderItems)
        {
            OrderID = orderID;
            Table = table;
            Employee = employee;
            CreatedAt = createdAt;
            ClosedAt = closedAt;
            OrderItems = orderItems;
        }
        private decimal TotalAmountToPay()
        {
            decimal total = 0.00m;
            if(OrderItems != null)
            {
                foreach (var item in OrderItems)
                {
                    total += item.PriceAccountedForQuantity;
                }
            }
            
            return total;
        }
        public void SetTemporaryOrderId(int id)
        {
            TemporaryOrderId = id;
        }
        public void TemporaryItemIdSetter()
        {
            for (int i = 0; i < OrderItems.Count; i++)
            {
                OrderItems[i].SetOrderItemTemporaryItemId(i);
            }
        }


    }
}
