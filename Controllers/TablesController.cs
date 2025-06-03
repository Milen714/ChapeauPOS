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

        public IActionResult Index()
        {
            var tables = _tablesService.GetAllTables();
            _tablesService.SynchronizeTableStatuses(tables);
            var orders = _ordersService.GetOrdersByStatus(OrderStatus.Ordered);
            ViewBag.KitchenOrders = orders;
            return View(tables);
        }
        [HttpPost]
        public IActionResult SetOrderItemsAsServed(int orderId, MenuCourse menuCourse, OrderItemStatus orderItemsStatus, int tableNumber)
        {
            _kitchenBarService.SetCourseToServed(orderId, menuCourse, orderItemsStatus);
            TempData["Success"] = $"Table: {tableNumber} - {menuCourse.ToString()} Course Has Been Served!!!";
            var tables = _tablesService.GetAllTables();
            return RedirectToAction("Index", tables);
        }



    }
}
