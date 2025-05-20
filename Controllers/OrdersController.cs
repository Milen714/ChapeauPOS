using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Models.ViewModels;
using ChapeauPOS.ViewModels;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ChapeauPOS.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChapeauPOS.Controllers
{

    public class OrdersController : BaseController
    {
        private readonly IEmployeesService _employeesService;
        private readonly ITablesService _tablesService;
        private readonly IOrdersService _ordersService;
        private readonly IMenuService _menuService;
        private readonly IHubContext<RestaurantHub> _hubContext;
        public OrdersController(IEmployeesService employeesService, ITablesService tablesService, IOrdersService ordersService, IMenuService menuService, IHubContext<RestaurantHub> hubContext)
        {
            _employeesService = employeesService;
            _tablesService = tablesService;
            _ordersService = ordersService;
            _menuService = menuService;
            _hubContext = hubContext;
        }


        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ChangeOrderStatus()
        {
            int tableNumber = 1; // Example table number
            Order order = _ordersService.GetOrderFromSession(HttpContext, tableNumber);
            order.OrderStatus = OrderStatus.Ordered;
            _ordersService.SaveOrderToSession(HttpContext, tableNumber, order);
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
            Order order = _ordersService.GetOrderFromSession(HttpContext, id);//GetOrderFromSession(id);
            //Check if the the ocupied table's order has been sent to kithchen/bar and if not load the order from DB
            if (table.TableStatus == TableStatus.Occupied && order.OrderStatus != OrderStatus.Pending)
            {
                Console.WriteLine("order has been sent to the kitchen/bar and DB Previously: Loading order from the DB and storing it in a session");
                order = _ordersService.GetOrderByTableId(table.TableNumber);
                _ordersService.SaveOrderToSession(HttpContext, table.TableNumber, order);
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
            Order order = _ordersService.GetOrderFromSession(HttpContext, tableNumber);
            return PartialView("_OrderListPartial", order);
        }
        [HttpPost]

        public IActionResult AddItemToOrder(int itemId, int tableId, int employeeId, string? note = null)
        {
            MenuItem menuItem = _menuService.GetMenuItemById(itemId);
            Table table = _tablesService.GetTableByID(tableId);
            Employee employee = _employeesService.GetEmployeeById(employeeId);
            Order order = _ordersService.GetOrderFromSession(HttpContext, tableId);

            // If this is a new order (i.e., table wasn't previously occupied)
            if (order.Table == null)
            {
                order.Table = table;
                order.Employee = employee;
                order.CreatedAt = DateTime.Now;
                order.OrderStatus = OrderStatus.Pending;
            }
            //order.SetTemporaryOrderId(table.TableNumber);
            // Check if the item already exists in the order, aswell as if the notes are the same
            var existingItem = order.OrderItems.FirstOrDefault(oi =>
                oi.MenuItem.MenuItemID == itemId &&
                string.Equals(oi.Notes?.Trim(), note?.Trim(), StringComparison.OrdinalIgnoreCase)
            );
            // If the item exists, increase the quantity
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {// If the item doesn't exist, create a new order item
                OrderItem orderItem = new OrderItem
                {
                    MenuItem = menuItem,
                    Quantity = 1,
                    Notes = note,
                    OrderItemStatus = OrderItemStatus.Ordered
                };
                order.OrderItems.Add(orderItem);

            }
            for (int i = 0; i < order.OrderItems.Count; i++)
            {
                order.OrderItems[i].SetOrderItemTemporaryItemId(i);
            }
            // Save the order to the session
            _ordersService.SaveOrderToSession(HttpContext, tableId, order);


            // Check if the table is already occupied
            if (table.TableStatus != TableStatus.Occupied)
            {
                table.TableStatus = TableStatus.Occupied;
                _tablesService.UpdateTableStatus(table.TableNumber, table.TableStatus);
            }


            return PartialView("_OrderListPartial", order);
        }


        public async Task<IActionResult> SendOrder(int id)
        {
            // Here Iam gonna send the order to the kitchen and bar 
            // and update the order status to Ordered
            // and save the order to the database
            Order order = _ordersService.GetOrderFromSession(HttpContext, id);
            order.OrderStatus = OrderStatus.Ordered;

            _ordersService.AddOrder(order);
            _ordersService.SaveOrderToSession(HttpContext, id, order);

            await _hubContext.Clients.Group("Bartenders").SendAsync("NewOrder");
            await _hubContext.Clients.Group("Cooks").SendAsync("NewOrder");


            return RedirectToAction("Index", "Tables");

        }
        public IActionResult DeleteOrder(int id)
        {
            // Here Iam gonna delete the order from the database
            // and remove the order from the session
            // and update the table status to Available
            Order order = _ordersService.GetOrderFromSession(HttpContext, id);
            Console.WriteLine(order.OrderStatus.ToString());
            if (order.OrderStatus != OrderStatus.Pending || order.OrderStatus != OrderStatus.Finalized)
            {
                // Remove the order from the database
                _ordersService.DeleteOrder(order.OrderID);
                // Update the table status to Available
                _tablesService.UpdateTableStatus(order.Table.TableNumber, TableStatus.Free);
                // Remove the order from the session
                _ordersService.RemoveOrderFromSession(HttpContext, id);

            }
            else
            {
                // If the order is not in the database, just remove it from the session
                // and update the table status to Available
                _tablesService.UpdateTableStatus(order.Table.TableNumber, TableStatus.Free);
                _ordersService.RemoveOrderFromSession(HttpContext, id);
            }

            //Table table = _tablesService.GetTableByID(id);
            order.Table.TableStatus = TableStatus.Free;
            _tablesService.UpdateTableStatus(order.Table.TableNumber, order.Table.TableStatus);
            _ordersService.RemoveOrderFromSession(HttpContext, id);
            return RedirectToAction("Index", "Tables");
        }
        [HttpPost]
        public IActionResult RemoveOrderItem(int tableId, int orderItemIdTemp, int orderItemId)
        {
            // Get the order from the session
            Order order = _ordersService.GetOrderFromSession(HttpContext, tableId);
            Console.WriteLine(order.OrderStatus.ToString());
            if (order.OrderStatus != OrderStatus.Pending || order.OrderStatus != OrderStatus.Finalized)
            {
                var orderItemDB = _ordersService.GetOrderItemById(orderItemId);
                if (orderItemDB != null)
                {
                    // Remove the item from the database
                    _ordersService.RemoveOrderItem(order.OrderID, orderItemDB.OrderItemId);
                    order.OrderItems.Remove(orderItemDB);
                    _ordersService.SaveOrderToSession(HttpContext, tableId, order);
                }
            }
            // Find the order item to remove
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.TemporaryId == orderItemIdTemp);
            if (orderItem != null)
            {
                // Remove the item from the order
                order.OrderItems.Remove(orderItem);
                // Save the updated order back to the session
                _ordersService.SaveOrderToSession(HttpContext, tableId, order);
            }
            return RedirectToAction("CreateOrder", new { id = order.Table.TableNumber });
        }
        [HttpPost]
        public IActionResult UpdateOrderItem(int tableId, int orderItemIdTemp, int orderItemId, int quantity, string? note)
        {
            // Get the order from the session
            Order order = _ordersService.GetOrderFromSession(HttpContext, tableId);
            Console.WriteLine(order.OrderStatus.ToString());
            if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Finalized)
            {
                var orderItemDB = _ordersService.GetOrderItemById(orderItemId);
                if (orderItemDB != null)
                {
                    // Update the item in the database
                    orderItemDB.Quantity = quantity;
                    orderItemDB.Notes = note;
                    _ordersService.UpdateOrderItem(orderItemDB);
                    // Update the item in the session
                    _ordersService.SaveOrderToSession(HttpContext, tableId, order);
                }
            }
            // Find the order item to update
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.TemporaryId == orderItemIdTemp);
            if (orderItem != null)
            {
                // Update the quantity
                orderItem.Quantity = quantity;
                orderItem.Notes = note;
                // Save the updated order back to the session
                _ordersService.SaveOrderToSession(HttpContext, tableId, order);
            }
            return RedirectToAction("CreateOrder", new { id = order.Table.TableNumber });
        }
        //Nishchal
        public IActionResult Payment(int tableId)
        {
            Order order = _ordersService.GetOrderByTableId(tableId); 
            if (order == null || order.OrderItems == null || order.OrderItems.Count == 0)
            {
                return NotFound("No order found for this table.");
            }
            PaymentViewModel viewModel = new PaymentViewModel
            {
                Order = order
            };

            var items = viewModel.Order.OrderItems;

            return View(viewModel);
        }

    }
}
