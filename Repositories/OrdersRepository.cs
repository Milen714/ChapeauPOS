using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;

namespace ChapeauPOS.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly string? _connectionString;
        public OrdersRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDB");
        }
        // Implement the methods from IOrdersRepository here
        public List<Order> GetAllOrders()
        {
            throw new NotImplementedException();
        }
        public Order GetOrderById(int orderId)
        {
            throw new NotImplementedException();
        }
        public void AddOrder(Order order)
        {
            throw new NotImplementedException();
        }
        public void UpdateOrder(Order order)
        {
            throw new NotImplementedException();
        }
        public void DeleteOrder(int orderId)
        {
            throw new NotImplementedException();
        }
        public List<Order> GetOrdersByTableId(int tableId)
        {
            throw new NotImplementedException();
        }
        public List<Order> GetOrdersByEmployeeId(int employeeId)
        {
            throw new NotImplementedException();
        }
        public List<Order> GetOrdersByStatus(OrderStatus status)
        {
            throw new NotImplementedException();
        }
    }
    
}
