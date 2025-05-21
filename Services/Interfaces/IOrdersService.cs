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

        Order GetOrderFromSession(HttpContext context, int tableId);
        void SaveOrderToSession(HttpContext context, int tableId, Order order);
        void RemoveOrderFromSession(HttpContext context, int tableId);
        void AddMenuItemToExistingOrder(int itemId, string? note, MenuItem menuItem, Order order);
        void AddToOrder(Order order);
    }
}
