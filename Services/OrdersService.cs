using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;

namespace ChapeauPOS.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IOrdersRepository _ordersRepository;
        public OrdersService(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }
        // Implement the methods from IOrdersService here
        public List<Order> GetAllOrders()
        {
            return _ordersRepository.GetAllOrders();
        }
        public Order GetOrderById(int orderId)
        {
            return _ordersRepository.GetOrderById(orderId);
        }
        public void AddOrder(Order order)
        {
            _ordersRepository.AddOrder(order);
        }
        public void UpdateOrder(Order order)
        {
            _ordersRepository.UpdateOrder(order);
        }
        public void DeleteOrder(int orderId)
        {
            _ordersRepository.DeleteOrder(orderId);
        }

        public List<Order> GetOrdersByTableId(int tableId)
        {
            return _ordersRepository.GetOrdersByTableId(tableId);
        }

        public List<Order> GetOrdersByEmployeeId(int employeeId)
        {
            return _ordersRepository.GetOrdersByEmployeeId(employeeId);
        }

        public List<Order> GetOrdersByStatus(OrderStatus status)
        {
            return _ordersRepository.GetOrdersByStatus(status);
        }

        public Order GetOrderByTableId(int tableId)
        {
            return _ordersRepository.GetOrderByTableId(tableId);
        }

    }
    
}
