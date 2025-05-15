using ChapeauPOS.Models;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class KitchenBarController : BaseController
    {

        private readonly IKitchenBarService _kitchenBarService;

        public KitchenBarController(IKitchenBarService kitchenBarService)
        {
            _kitchenBarService = kitchenBarService;
        }

        public IActionResult KitchenRunningOrders()
        {
            List<Order> orders = _kitchenBarService.GetRunningKitchenOrders();
            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateKitchenItemStatus(int orderItemId, OrderItemStatus orderItemStatus)
        {
            _kitchenBarService.UpdateKitchenOrderItemStatus(orderItemId, orderItemStatus);
            return RedirectToAction("KitchenRunningOrders");
        }

        //[HttpPost]
        //public IActionResult UpdateKitchenOrderStatus(int orderId, OrderStatus orderStatus)
        //{
        //    _kitchenBarService.UpdateKitchenOrderStatus(orderId, orderStatus);
        //    return RedirectToAction("KitchenRunningOrders");
        //}

        //[HttpPost]
        //public IActionResult UpdateKitchenCourseStatus(int orderId, MenuCourse menuCourse, CourseStatus courseStatus)
        //{
        //    _kitchenBarService.UpdateKitchenCourseStatus(orderId, menuCourse, courseStatus);
        //    return RedirectToAction("KitchenRunningOrders");
        //}

        public IActionResult KitchenFinishedOrders()
        {
            List<Order> orders = _kitchenBarService.GetFinishedKitchenOrders();
            return View(orders);
        }


        public IActionResult BarRunningOrders()
        {
            List<Order> orders = _kitchenBarService.GetRunningBarOrders();
            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateBarItemStatus(int orderItemId, OrderItemStatus orderItemStatus)
        {
            _kitchenBarService.UpdateBarOrderItemStatus(orderItemId, orderItemStatus);
            return RedirectToAction("BarRunningOrders");
        }

        //[HttpPost]
        //public IActionResult UpdateBarOrderStatus(int orderId, OrderStatus orderStatus)
        //{
        //    _kitchenBarService.UpdateBarOrderStatus(orderId, orderStatus);
        //    return RedirectToAction("BarRunningOrders");
        //}

        public IActionResult BarFinishedOrders()
        {
            List<Order> orders = _kitchenBarService.GetFinishedBarOrders();
            return View(orders);
        }

        public IActionResult CloseOrder(int orderId)
        {
            _kitchenBarService.CloseFoodOrder(orderId);
            return RedirectToAction("KitchenRunningOrders");
        }

        public IActionResult UpdateItemStatusBasedOnCourse(int orderId, MenuCourse course, string courseStatus)
        {
            // Parse the course status string to enum
            if (!Enum.TryParse(courseStatus, out OrderItemStatus orderItemStatus))
            {
                return BadRequest("Invalid course status");
            }

            _kitchenBarService.UpdateItemStatusBasedOnCourse(orderId, course, orderItemStatus);
            return RedirectToAction("KitchenRunningOrders");
        }

        public IActionResult GetRunningTime(int orderId)
        {
            var order = _kitchenBarService.GetRunningKitchenOrders().FirstOrDefault(o => o.OrderID == orderId);

            if (order?.CreatedAt == null)
                return Content("00:00:00");

            return Content((DateTime.Now - order.CreatedAt).ToString(@"hh\:mm\:ss"));
        }

    }
}
