namespace ChapeauPOS.Models
{
    public class Order
    {
        public int OrderID { get; set; }
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

        public Order()
        {
            
        }

        public Order(int orderID, Table table, Employee employee, DateTime createdAt, DateTime? closedAt)
        {
            OrderID = orderID;
            Table = table;
            Employee = employee;
            CreatedAt = createdAt;
            ClosedAt = closedAt;
            OrderItems = new List<OrderItem>();
        }
    }
}
