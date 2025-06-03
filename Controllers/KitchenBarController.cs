using ChapeauPOS.Commons;
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
            //Employee? loggedInEmployee = null;
            //loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");

            //if (loggedInEmployee == null || loggedInEmployee.Role != Roles.Manager || loggedInEmployee.Role != Roles.Cook)
            //{
            //    TempData["ErrorMessage"] = "You do not have permission to access this page.";
            //    HttpContext.Session.Remove("LoggedInUser");
            //    return RedirectToAction("Login", "Home");
            //}
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
            //Employee? loggedInEmployee = null;
            //loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");

            //if (loggedInEmployee == null || loggedInEmployee.Role != Roles.Manager || loggedInEmployee.Role != Roles.Cook)
            //{
            //    TempData["ErrorMessage"] = "You do not have permission to access this page.";
            //    HttpContext.Session.Remove("LoggedInUser");
            //    return RedirectToAction("Login", "Home");
            //}
            List<Order> orders = _kitchenBarService.GetFinishedKitchenOrders();
            return View(orders);
        }

        public IActionResult BarRunningOrders()
        {
            //Employee? loggedInEmployee = null;
            //loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");

            //if (loggedInEmployee == null || loggedInEmployee.Role != Roles.Manager || loggedInEmployee.Role != Roles.Bartender)
            //{
            //    TempData["ErrorMessage"] = "You do not have permission to access this page.";
            //    HttpContext.Session.Remove("LoggedInUser");
            //    return RedirectToAction("Login", "Home");
            //}
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
            //Employee? loggedInEmployee = null;
            //loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");

            //if (loggedInEmployee == null || loggedInEmployee.Role != Roles.Manager || loggedInEmployee.Role != Roles.Bartender)
            //{
            //    TempData["ErrorMessage"] = "You do not have permission to access this page.";
            //    HttpContext.Session.Remove("LoggedInUser");
            //    return RedirectToAction("Login", "Home");
            //}
            List<Order> orders = _kitchenBarService.GetFinishedBarOrders();
            return View(orders);
        }

        [HttpPost]
        public IActionResult CloseFoodOrder(int orderId)
        {
            _kitchenBarService.CloseFoodOrder(orderId);
            return RedirectToAction("KitchenRunningOrders");
        }

        [HttpPost]
        public IActionResult CloseDrinkOrder(int orderId)
        {
            _kitchenBarService.CloseDrinkOrder(orderId);
            return RedirectToAction("BarRunningOrders");
        }

        [HttpPost]
        public IActionResult UpdateItemStatusBasedOnCourse(int orderId, MenuCourse course, CourseStatus courseStatus)
        {
            OrderItemStatus orderItemStatus;
            if (courseStatus == CourseStatus.Ordered)
            {
                orderItemStatus = OrderItemStatus.Ordered;
            }
            else if (courseStatus == CourseStatus.Preparing)
            {
                orderItemStatus = OrderItemStatus.Preparing;
            }
            else
            {
                orderItemStatus = OrderItemStatus.Ready;
            }

            _kitchenBarService.UpdateItemStatusBasedOnCourse(orderId, course, orderItemStatus);
            return RedirectToAction("KitchenRunningOrders");
        }

        public IActionResult GetRunningKitchenTime(int orderId)
        {
            var order = _kitchenBarService.GetRunningKitchenOrders().FirstOrDefault(o => o.OrderID == orderId);

            if (order?.CreatedAt == null)
                return Content("00:00:00");

            return Content((DateTime.Now - order.CreatedAt).ToString(@"hh\:mm\:ss"));
        }

        public IActionResult GetRunningBarTime(int orderId)
        {
            var order = _kitchenBarService.GetRunningBarOrders().FirstOrDefault(o => o.OrderID == orderId);

            if (order?.CreatedAt == null)
                return Content("00:00:00");

            return Content((DateTime.Now - order.CreatedAt).ToString(@"hh\:mm\:ss"));
        }

    }
}
