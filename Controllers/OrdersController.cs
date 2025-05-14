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
        private const string OrderSessionKeyPrefix = "TableOrder_";
        //Gets the order from the session based on table Number
        private Order GetOrderFromSession(int tableId)
        {
            return HttpContext.Session.GetObject<Order>($"{OrderSessionKeyPrefix}{tableId}") ?? new Order
            {
                OrderItems = new List<OrderItem>(),
                CreatedAt = DateTime.Now
            };
        }
        //Saves table order to the session based on table Number
        private void SaveOrderToSession(int tableId, Order order)
        {
            HttpContext.Session.SetObject($"{OrderSessionKeyPrefix}{tableId}", order);
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ChangeOrderStatus()
        {
            int tableNumber = 1; // Example table number
            Order order = GetOrderFromSession(tableNumber);
            order.OrderStatus = OrderStatus.Ordered;
            SaveOrderToSession(tableNumber, order);
            return RedirectToAction("Index", "Tables");
        }
        [HttpGet]
        public IActionResult CreateOrder(int id)
        {
            Employee? loggedInEmployee = new Employee();
            loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
            ViewBag.LoggedInEmployee = loggedInEmployee;
            Table table = _tablesService.GetTableByID(id);
            //get the order from the session
            Order order = GetOrderFromSession(id);
            //Check if the the ocupied table's order has been sent to kithchen/bar and if not load the order from DB
            if (table.TableStatus == TableStatus.Occupied && order.OrderStatus != OrderStatus.Pending)
            {
                Console.WriteLine("order has been sent to the kitchen: Load order from the db instedd and store it in the session");
            }

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
        public IActionResult DisplayOrderView(string tableId)
        {
            int tableNumber = int.Parse(tableId);
            Order order = GetOrderFromSession(tableNumber);
            return PartialView("_OrderListPartial", order);
        }
        [HttpPost]
       
        public IActionResult AddItemToOrder(int itemId, int tableId, int employeeId)
        {
            MenuItem menuItem = _menuService.GetMenuItemById(itemId);
            Table table = _tablesService.GetTableByID(tableId);
            Employee employee = _employeesService.GetEmployeeById(employeeId);

            Order order = GetOrderFromSession(tableId);

            // If this is a new order (i.e., table wasn't previously occupied)
            if (order.Table == null)
            {
                order.Table = table;
                order.Employee = employee;
                order.CreatedAt = DateTime.Now;
                order.OrderStatus = OrderStatus.Pending;
            }

            OrderItem orderItem = new OrderItem
            {
                MenuItem = menuItem,
                MenuCourse = menuItem.Course,
                OrderItemStatus = OrderItemStatus.Ordered,
                CourseStatus = CourseStatus.Ordered,
                Quantity = 1
            };
            order.OrderItems.Add(orderItem);
            SaveOrderToSession(tableId, order);

            // Check if the table is already occupied
            if (table.TableStatus != TableStatus.Occupied)
            {
                
            }
           
            table.TableStatus = TableStatus.Occupied;
            _tablesService.UpdateTableStatus(table.TableNumber, table.TableStatus);

            return PartialView("_OrderListPartial", order);
        }
        [HttpPost]
        public IActionResult SendOrder()
        {
            // Here Iam gonna send the order to the kitchen and bar 
            // and update the order status to sent
            // and save the order to the database
            return RedirectToAction("Tables", "Index");
            
        }

    }
}
