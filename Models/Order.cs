namespace ChapeauPOS.Models
{
    public class Order
    {
        public Order()
        {
        }

        public Order(int orderID, Table table, Employee employee, OrderStatus status, DateTime createdAt, DateTime closedAt)
        {
            OrderID = orderID;
            Table = table;
            Employee = employee;
            Status = status;
            CreatedAt = createdAt;
            ClosedAt = closedAt;
        }

        public int OrderID { get; set; }
        public Table Table { get; set; }
        public Employee Employee { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ClosedAt { get; set; }
    }
}
