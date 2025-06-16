using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;

namespace ChapeauPOS.Services
{
    public class KitchenBarService : IKitchenBarService
    {
        private readonly IKitchenBarRepository _kitchenBarRepository;

        public KitchenBarService(IKitchenBarRepository kitchenBarRepository)
        {
            _kitchenBarRepository = kitchenBarRepository;
        }

        public List<Order> GetRunningKitchenOrders()
        {
            return _kitchenBarRepository.GetRunningKitchenOrders();
        }

        public List<Order> GetFinishedKitchenOrders()
        {
            return _kitchenBarRepository.GetFinishedKitchenOrders();
        }

        public List<Order> GetRunningBarOrders()
        {
            return _kitchenBarRepository.GetRunningBarOrders();
        }

        public List<Order> GetFinishedBarOrders()
        {
            return _kitchenBarRepository.GetFinishedBarOrders();
        }

        public void UpdateBarOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus)
        {
            _kitchenBarRepository.UpdateBarOrderItemStatus(orderItemId, orderItemStatus);
        }

        public void UpdateKitchenOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus)
        {
            _kitchenBarRepository.UpdateKitchenOrderItemStatus(orderItemId, orderItemStatus);
        }

        public void CloseFoodOrder(int orderId)
        {
            _kitchenBarRepository.CloseFoodOrder(orderId);
        }

        public void CloseDrinkOrder(int orderId)
        {
            _kitchenBarRepository.CloseDrinkOrder(orderId);
        }

        public void UpdateItemStatusBasedOnCourse(int orderId, MenuCourse course, OrderItemStatus orderItemStatus)
        {
            _kitchenBarRepository.UpdateItemStatusBasedOnCourse(orderId, course, orderItemStatus);
        }

        public void SetCourseToServed(int orderId, MenuCourse course, OrderItemStatus orderItemStatus)
        {

            _kitchenBarRepository.SetCourseToServed(orderId, course, orderItemStatus);
        }
    }
}
