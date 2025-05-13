using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;

namespace ChapeauPOS.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IMenuRepository _menuRepository;
        public OrdersService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }
        // Implement the methods from IOrdersService here
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
