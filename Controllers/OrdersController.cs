using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Models.ViewModels;
using ChapeauPOS.ViewModels;
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
                Console.WriteLine("order has been sent to the kitchen/bar and DB Previously: Loading order from the DB and storing it in a session");
                order = _ordersService.GetOrderByTableId(table.TableNumber);
                SaveOrderToSession(table.TableNumber, order);
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
                //MenuCourse = menuItem.Course,
                OrderItemStatus = OrderItemStatus.Ordered,
                //CourseStatus = CourseStatus.Ordered,
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
        
        public IActionResult SendOrder(int id)
        {
            // Here Iam gonna send the order to the kitchen and bar 
            // and update the order status to Ordered
            // and save the order to the database
            // Finally Remove the order from the session
            Order order = GetOrderFromSession(id);
            order.OrderStatus = OrderStatus.Ordered;
            
            _ordersService.AddOrder(order);
            SaveOrderToSession(order.Table.TableNumber, order);
            


            return RedirectToAction("Index", "Tables");

        }
        public IActionResult DeleteOrder(int id)
        {
            // Here Iam gonna delete the order from the database
            // and remove the order from the session
            // and update the table status to Available
            Order order = GetOrderFromSession(id);
            
            //Table table = _tablesService.GetTableByID(id);
            order.Table.TableStatus = TableStatus.Free;
            _tablesService.UpdateTableStatus(order.Table.TableNumber, order.Table.TableStatus);
            HttpContext.Session.Remove($"{OrderSessionKeyPrefix}{id}");
            return RedirectToAction("Index", "Tables");
        }

        public IActionResult Payment(int tableId)
        {
            Order order = _ordersService.GetOrderByTableId(tableId); //Uses the service layer _ordersService to get the order from the database, not from session because order will stored in database only when the it is send to kitchen.
            if (order == null || order.OrderItems == null || order.OrderItems.Count == 0)
            {
                return NotFound("No order found for this table.");
            }

            var items = new List<PaymentItemViewModel>();

            foreach (var item in order.OrderItems)
            {
                var existing = items.FirstOrDefault(i => i.Name == item.MenuItem.ItemName); //Checks if the order is already in list if yes add quantity  else add new item in payment screen.
                if (existing != null)
                {
                    existing.Quantity += item.Quantity;
                }
                else
                {
                    items.Add(new PaymentItemViewModel
                    {
                        Name = item.MenuItem.ItemName,
                        Quantity = item.Quantity,
                        UnitPrice = item.MenuItem.ItemPrice,
                        VATRate = item.MenuItem.VATPercent
                    });
                }
            }

            decimal total = items.Sum(i => i.TotalPrice);
            decimal lowVAT = items.Where(i => i.VATRate ==9).Sum(i => i.TotalPrice * 0.09m);
            decimal highVAT = items.Where(i => i.VATRate ==21 ).Sum(i => i.TotalPrice * 0.21m);

            var viewModel = new PaymentViewModel
            {
                TableNumber = order.Table.TableNumber,
                Items = items,
                TotalAmount = total,
                LowVAT = lowVAT,
                HighVAT = highVAT
            };

            return View(viewModel);
        }

























































    }
}
