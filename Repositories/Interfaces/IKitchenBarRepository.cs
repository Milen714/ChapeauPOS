using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface IKitchenBarRepository
    {
        List<Order> GetRunningKitchenOrders();
        void UpdateKitchenOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus);
        //void UpdateKitchenOrderStatus(int orderId, OrderStatus orderStatus);
        //void UpdateKitchenCourseStatus(int orderId, MenuCourse menuCourse, CourseStatus courseStatus);
        List<Order> GetFinishedKitchenOrders();
        List<Order> GetRunningBarOrders();
        void UpdateBarOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus);
        //void UpdateBarOrderStatus(int orderId, OrderStatus orderStatus);
        List<Order> GetFinishedBarOrders();
        void CloseFoodOrder(int orderId);
        void UpdateItemStatusBasedOnCourse(int orderId, MenuCourse course, OrderItemStatus orderItemStatus);
    }
}
