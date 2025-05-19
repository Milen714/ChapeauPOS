using ChapeauPOS.Models;

namespace ChapeauPOS.Services.Interfaces
{
    public interface IOrdersService
    {
        List<Order> GetAllOrders();
        Order GetOrderById(int orderId);
        void AddOrder(Order order);
        void UpdateOrderItem(OrderItem orderItem);
        void DeleteOrder(int orderId);
        List<Order> GetOrdersByTableId(int tableId);
        Order GetOrderByTableId(int tableId);
        List<Order> GetOrdersByEmployeeId(int employeeId);
        List<Order> GetOrdersByStatus(OrderStatus status);
        OrderItem GetOrderItemById(int id);
        void RemoveOrderItem(int orderId, int orderItemId);
    }
}
