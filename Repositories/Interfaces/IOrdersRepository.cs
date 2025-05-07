
using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface IOrdersRepository
    {
        List<Order> GetAllOrders();
        Order GetOrderById(int orderId);
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int orderId);
        List<Order> GetOrdersByTableId(int tableId);
        List<Order> GetOrdersByEmployeeId(int employeeId);
        List<Order> GetOrdersByStatus(OrderStatus status);
    }
}
