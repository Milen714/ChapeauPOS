using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class TablesController : BaseController
    {
        private readonly ITablesService _tablesService;
        private readonly IOrdersService _ordersService;
        private readonly IKitchenBarService _kitchenBarService;
        public TablesController(ITablesService tablesService, IOrdersService ordersService, IKitchenBarService kitchenBarService)
        {
            _tablesService = tablesService;
            _ordersService = ordersService;
            _kitchenBarService = kitchenBarService;
        }
        [SessionAuthorize(Roles.Manager, Roles.Waiter)]
        public IActionResult Index()
        {
            var tables = _tablesService.GetAllTables();
            _tablesService.SynchronizeTableStatuses(tables);
            var orders = _ordersService.GetOrdersByStatus(OrderStatus.Ordered);//Ordered because I need only running orders and not historic data from "two years ago"
            ViewBag.KitchenOrders = orders;//used to pipe the running orders data to the view
            return View(tables);
        }
        [HttpPost]
        public IActionResult SetOrderItemsAsServed(int orderId, MenuCourse menuCourse, OrderItemStatus orderItemsStatus, int tableNumber)
        {
            var tables = _tablesService.GetAllTables();

            try
            {
                _kitchenBarService.SetCourseToServed(orderId, menuCourse, orderItemsStatus);
                TempData["Success"] = $"Table: {tableNumber} - {menuCourse.ToString()} Course Has Been Served!!!";
                return RedirectToAction("Index", tables);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Something went wrong: {ex.Message}";
                return RedirectToAction("Index", tables);
            }
        }



    }
}
