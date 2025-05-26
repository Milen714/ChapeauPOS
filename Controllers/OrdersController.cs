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
using ChapeauPOS.Services;

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

        [HttpGet]
        public IActionResult CreateOrder(int id)
        {
            try
            {
                Employee loggedInEmployee = HttpContext.Session.GetObject<Employee>("LoggedInUser");
                ViewBag.LoggedInEmployee = loggedInEmployee;

                Table table = _tablesService.GetTableByID(id);
                Order order = _ordersService.GetOrderFromSession(HttpContext, id);

                if (table.TableStatus == TableStatus.Free)
                {
                    _ordersService.RemoveOrderFromSession(HttpContext, id);
                    order = new Order();
                }
                else if (table.TableStatus == TableStatus.Occupied)
                {
                    var dbOrder = _ordersService.GetOrderByTableId(table.TableNumber);
                    if (dbOrder != null && dbOrder.OrderStatus == OrderStatus.Finalized)
                    {
                        _ordersService.RemoveOrderFromSession(HttpContext, id);
                        order = new Order();
                    }
                    else if (dbOrder != null && order.OrderStatus != OrderStatus.Pending)
                    {
                        order = dbOrder;
                        _ordersService.SaveOrderToSession(HttpContext, table.TableNumber, order);
                    }
                }

                ViewBag.OrderStatus = order.OrderStatus;
                return View(table);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating the order: " + ex.Message;
                return RedirectToAction("Index", "Tables");
            }
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
        public IActionResult GetMenuItemsBySearch(string searchParams, int tableNumber)
        {
            List<MenuItem> menuItems = _menuService.GetAllMenuItems();
            if (!string.IsNullOrEmpty(searchParams))
            {
                menuItems = menuItems.Where(mi => mi.ItemName.Contains(searchParams, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (menuItems == null || menuItems.Count == 0)
            {
                TempData["Error"] = $"No items found matching your search criteria: {searchParams}";
                return RedirectToAction("CreateOrder", tableNumber);
            }
            MenuViewModel searchMenu = new MenuViewModel("Search Results", menuItems, menuItems);
            return PartialView("_MenuPartial", searchMenu);
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
            try
            {
                MenuItem menuItem = _menuService.GetMenuItemById(itemId);
                Table table = _tablesService.GetTableByID(tableId);
                Employee employee = _employeesService.GetEmployeeById(employeeId);
                Order order = _ordersService.GetOrderFromSession(HttpContext, tableId);

                if (order.Table == null)
                {
                    order.Table = table;
                    order.Employee = employee;
                    order.CreatedAt = DateTime.Now;
                    order.OrderStatus = OrderStatus.Pending;
                }

                _ordersService.AddMenuItemToExistingOrder(itemId, note, menuItem, order);
                _ordersService.SaveOrderToSession(HttpContext, tableId, order);

                if (table.TableStatus != TableStatus.Occupied)
                {
                    table.TableStatus = TableStatus.Occupied;
                    _tablesService.UpdateTableStatus(table.TableNumber, table.TableStatus);
                }

                return PartialView("_OrderListPartial", order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding the item to the order." + ex.Message;
                return RedirectToAction("Index", "Tables");
            }
        }



        public async Task<IActionResult> SendOrder(int id)
        {
            // Here Iam gonna send the order to the kitchen and bar 
            // and update the order status to Ordered
            // and save the order to the database
            try
            {
                Order order = _ordersService.GetOrderFromSession(HttpContext, id);

                if (order.OrderStatus != OrderStatus.Pending)
                {
                    _ordersService.AddToOrder(order);
                    _menuService.DeductStock(order);
                }
                else
                {
                    order.OrderStatus = OrderStatus.Ordered;
                    _ordersService.AddOrder(order);
                    _menuService.DeductStock(order);
                    _ordersService.SaveOrderToSession(HttpContext, id, order);
                }

                await _hubContext.Clients.Group("Bartenders").SendAsync("NewOrder");
                await _hubContext.Clients.Group("Cooks").SendAsync("NewOrder");

                TempData["Success"] = "Order has been successfully sent!";
                return RedirectToAction("Index", "Tables");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to send the order." + ex.Message;
                return RedirectToAction("CreateOrder", new { id });
            }

        }
        public IActionResult DeleteOrder(int id)
        {
            // Here Iam gonna delete the order from the database
            // and remove the order from the session
            // and update the table status to Available
            try
            {
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
            catch (Exception ex)
            {

                TempData["Error"] = "Failed to delete the order.";
                return RedirectToAction("CreateOrder", new { id });
            }
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
        public IActionResult MoveTable(int id)
        {
            List<Table> tables = _tablesService.GetAllUnoccupiedTables();
            ViewBag.Order = _ordersService.GetOrderByTableId(id);
            return PartialView("_MoveTable", tables);
        }
        [HttpPost]
        public IActionResult MoveOrderToTable(Order order, int tableId, int CurrentTableNumber, int MovetableNumber)
        {
            _ordersService.MoveOrderToAnotherTable(tableId, order);
            TempData["Success"] = $"Table {CurrentTableNumber}'s order has been moved to table: {MovetableNumber}";
            return RedirectToAction("Index", "Tables");
        }
        //Nishchal
        public IActionResult Payment(int id)
        {
            Order order = _ordersService.GetOrderByTableId(id);
            if (order == null || order.OrderItems == null || order.OrderItems.Count == 0)
            {
                return NotFound("No order found for this table.");
            }
            PaymentViewModel viewModel = new PaymentViewModel
            {
                Order = order
            };

            var items = viewModel.Order.OrderItems;
            ViewBag.PaymentModel = viewModel;

            return View(viewModel);
        }
        public IActionResult PaymentConfirmationPopup(int tableId)
        {
            var order = _ordersService.GetOrderByTableId(tableId);
            var viewModel = new PaymentViewModel { Order = order };
            return PartialView("_PaymentConfirmationPopup", viewModel);
        }
        [HttpPost]
        public IActionResult FinalizePayment(string total, int tableId)
        {
            var order = _ordersService.GetOrderByTableId(tableId);
            TempData["Success"] = $"Order has been successfully Paid! {total}";
            return RedirectToAction("Index", "Tables");
        }

    }
}
