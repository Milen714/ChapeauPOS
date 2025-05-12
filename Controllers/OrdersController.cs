using ChapeauPOS.Models;
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
        private readonly IMenuRepository _menuRepository;
        public OrdersController(IEmployeesService employeesService, ITablesService tablesService, IOrdersService ordersService, IMenuRepository menuRepository)
        {
            _employeesService = employeesService;
            _tablesService = tablesService;
            _ordersService = ordersService;
            _menuRepository = menuRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CreateOrder(int id)
        {
            var menuCategories = _menuRepository.GetMenuItemsByCategory(new MenuCategory {CategoryID = 1, CategoryName = "Lunch" });
            ViewBag.TableNumber = id;
            List<MenuItem> menuItems = _menuRepository.GetAllMenuItems();
            return View();
        }
    }
}
