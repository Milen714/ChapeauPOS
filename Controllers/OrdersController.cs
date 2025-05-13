using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Models.ViewModels;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{

    public class OrdersController : BaseController
    {
        private readonly IEmployeesService _employeesService;
        private readonly ITablesService _tablesService;
        private readonly IOrdersService _ordersService;
        private readonly IMenuService _menuService;
        public OrdersController(IEmployeesService employeesService, ITablesService tablesService, IOrdersService ordersService, IMenuService menuService)
        {
            _employeesService = employeesService;
            _tablesService = tablesService;
            _ordersService = ordersService;
            _menuService = menuService;
        }

        private List<MenuItem> TemuOrder = new List<MenuItem>();
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CreateOrder(int id)
        {
            Table table = _tablesService.GetTableByID(id);
            return View(table);
        }

        public IActionResult GetMenuItems(string category)
        {
            while (category != null)
            {
                if (category == "Lunch")
                {
                    MenuViewModel lunchMenu = new MenuViewModel(category, _menuService.GetLunch(), _menuService.GetDrinks());
                    return PartialView("_MenuPartial", lunchMenu);
                }
                else if (category == "Dinner")
                {
                    MenuViewModel dinnerMenu = new MenuViewModel(category, _menuService.GetDinner(), _menuService.GetDrinks());
                    return PartialView("_MenuPartial", dinnerMenu);
                }
                else if (category == "Drinks")
                {
                    MenuViewModel dinnerMenu = new MenuViewModel(category, _menuService.GetDinner(), _menuService.GetDrinks());
                    return PartialView("_MenuPartial", dinnerMenu);
                }
                else
                {
                    return NotFound();
                }
            }
            return NotFound();
        }
        [HttpPost]
       
        public IActionResult AddItemToOrder(int itemId, int tableId)
        {
            ViewBag.ItemId = tableId;
            MenuItem menuItem = _menuService.GetMenuItemById(itemId);
            TemuOrder.Add(menuItem);


            return PartialView("_OrderListPartial", TemuOrder);
        }

    }
}
