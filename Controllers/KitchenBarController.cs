using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class KitchenBarController : BaseController
    {

        private readonly IKitchenBarRepository _kitchenBarRepository;

        public KitchenBarController(IKitchenBarRepository kitchenBarRepository)
        {
            _kitchenBarRepository = kitchenBarRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult KitchenRunningOrders()
        {
            List<Order> orders = _kitchenBarRepository.GetRunningKitchenOrders();
            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateItemStatus(int orderItemId, OrderItemStatus orderItemStatus)
        {
            _kitchenBarRepository.UpdateOrderItemStatus(orderItemId, orderItemStatus);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, OrderStatus orderStatus)
        {
            _kitchenBarRepository.UpdateOrderStatus(orderId, orderStatus);
            return Json(new { success = true });
        }

        public IActionResult KitchenFinishedOrders()
        {
            return View();
        }

        public IActionResult Bar()
        {
            return View();
        }
    }
}
