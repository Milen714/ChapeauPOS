using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface IKitchenBarRepository
    {
        List<Order> GetRunningKitchenOrders();
        void UpdateKitchenOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus);
        List<Order> GetFinishedKitchenOrders();
        List<Order> GetRunningBarOrders();
        void UpdateBarOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus);
        List<Order> GetFinishedBarOrders();
        void CloseFoodOrder(int orderId);
        void CloseDrinkOrder(int orderId);
        void UpdateItemStatusBasedOnCourse(int orderId, MenuCourse course, OrderItemStatus orderItemStatus);
        public void SetCourseToServed(int orderId, MenuCourse course, OrderItemStatus orderItemStatus);
    }
}
