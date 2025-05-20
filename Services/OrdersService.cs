using ChapeauPOS.Commons;
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
        public void UpdateOrderItem(OrderItem orderItem)
        {
            _ordersRepository.UpdateOrderItem(orderItem);
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

        public OrderItem GetOrderItemById(int id)
        {
            return _ordersRepository.GetOrderItemById(id);
        }

        public void RemoveOrderItem(int orderId, int orderItemId)
        {
            _ordersRepository.RemoveOrderItem(orderId, orderItemId);
        }

        private const string OrderSessionKeyPrefix = "TableOrder_";

        public Order GetOrderFromSession(HttpContext context, int tableId)
        {
            return context.Session.GetObject<Order>($"{OrderSessionKeyPrefix}{tableId}") ?? new Order
            {
                OrderItems = new List<OrderItem>(),
                CreatedAt = DateTime.Now
            };
        }

        public void SaveOrderToSession(HttpContext context, int tableId, Order order)
        {
            context.Session.SetObject($"{OrderSessionKeyPrefix}{tableId}", order);
        }

        public void RemoveOrderFromSession(HttpContext context, int tableId)
        {
            context.Session.Remove($"{OrderSessionKeyPrefix}{tableId}");
        }
    }
    
}
