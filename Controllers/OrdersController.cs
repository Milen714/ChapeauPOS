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
            //var orders = _ordersService.GetAllOrders();
            //foreach (var order in orders)
            //{
            //    Console.WriteLine($"Ordered from table: {order.Table.TableNumber}with ID: {order.Table.TableID}");
            //    foreach (var item in order.OrderItems)
            //    {
            //        Console.WriteLine(item.MenuItem.ItemName);
            //    }
            //}

            Employee? loggedInEmployee = new Employee();
            loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            ViewBag.LoggedInEmployee = loggedInEmployee;
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
       
        public IActionResult AddItemToOrder(int itemId, int tableId, int employeeId)
        {
            MenuItem menuItem = _menuService.GetMenuItemById(itemId);
            Table table = _tablesService.GetTableByID(tableId);
            Employee employee = _employeesService.GetEmployeeById(employeeId);
            OrderItem orderItem = new OrderItem 
            {
                MenuItem = menuItem,
                MenuCourse = menuItem.Course,
                OrderItemStatus = OrderItemStatus.Ordered,
                CourseStatus = CourseStatus.Ordered,
                Quantity = 1
            };
            Order order = new Order
            {
                Table = table,
                Employee = employee,
                OrderItems = new List<OrderItem> { orderItem },
                OrderStatus = OrderStatus.Ordered,
                CreatedAt = DateTime.Now
            };
            TemuOrder.Add(menuItem);
            table.TableStatus = TableStatus.Occupied;
            _tablesService.UpdateTableStatus(table.TableNumber, table.TableStatus);

            return PartialView("_OrderListPartial", TemuOrder);
        }

    }
}
