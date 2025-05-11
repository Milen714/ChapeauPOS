using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface IKitchenBarRepository
    {
        List<Order> GetRunningKitchenOrders();
        void UpdateOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus);
        void UpdateOrderStatus(int orderId, OrderStatus orderStatus);
    }
}
