using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Models.ViewModels;
using ChapeauPOS.ViewModels;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ChapeauPOS.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using ChapeauPOS.Services;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;


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
        // This method is responsible for creating a new order for the specific table specific table.
        // Theargument 'id' is the table number for which the order is being created.
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
                        order.OrderItems = dbOrder.OrderItems;
                        order.Employee = dbOrder.Employee;
                        order.Table = dbOrder.Table;
                        order.CreatedAt = dbOrder.CreatedAt;
                        order.OrderID = dbOrder.OrderID;
                        //order = dbOrder;
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
        // This method retrieves menu items based on the category provided and returns a partial view with the menu items.
        // The 'category' parameter can be "Lunch", "Dinner", or "Drinks" and it comes from the AJAX request made by the client-side code.
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
        // This methiod retrieves menu items based on the search parameters provided by the user.
        // The 'searchParams' parameter is a string that contains the search query,
        // and 'tableNumber' is the table number for which the order is being created.
        // It is used to redirect the user back to the order creation view if no items are found.
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
        // This method is responsible for displaying the order view for a specific table.
        // It returns a partial view with the order details for the specified table.
        public IActionResult DisplayOrderView(string tableId)
        {
            int tableNumber = int.Parse(tableId);
            Order order = _ordersService.GetOrderFromSession(HttpContext, tableNumber);
            return PartialView("_OrderListPartial", order);
        }
        [HttpPost]
        // This method is responsible for adding a menu item to an order for a specific table.
        // It does several things depending on the current state of the table and the order:
        // 1. It checks if the order already exists in the session for the specified table.
        // 2. If the order does not exist, it creates a new order and associates it with the table and employee.
        // 3. It adds the selected menu item to the order.
        // 4. It updates the table status to "Occupied" if it was previously "Free".
        //
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

                if (table.TableStatus == TableStatus.Free)
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
                    _menuService.DeductStock(order);
                    _ordersService.AddToOrder(order);
                    order.InterumOrderItems.Clear();
                    _ordersService.SaveOrderToSession(HttpContext, id, order);
                }
                else if(order.OrderStatus == OrderStatus.Pending)
                {
                    order.OrderStatus = OrderStatus.Ordered;
                    _menuService.DeductStock(order);
                    _ordersService.AddOrder(order);
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

            
            ViewBag.PaymentModel = viewModel;

            return View(viewModel);
        }
        public IActionResult PaymentConfirmationPopup(int tableId, string paymentMethod)
        {
            PaymentMethod paymentMethod1 = (PaymentMethod)Enum.Parse(typeof(PaymentMethod), paymentMethod);
            var order = _ordersService.GetOrderByTableId(tableId);
            var viewModel = new PaymentViewModel { Order = order, PaymentMethod = paymentMethod1 };
            return PartialView("_PaymentConfirmationPopup", viewModel);
        }
        [HttpPost]
        public IActionResult FinalizePayment(PaymentMethod paymentMethod, int tableID, string feedBack, string total, string amountPaid)
        {
            try
            {
                decimal baseTotal = decimal.Parse(total);
                decimal grandTotalPaid = decimal.Parse(amountPaid);

                var order = _ordersService.GetOrderByTableId(tableID);
                Bill bill = _ordersService.GetBillByOrderId(order.OrderID);

                var viewModel = new PaymentViewModel
                {
                    Order = order,
                    PaymentMethod = paymentMethod
                };

                var payment = new Payment
                {
                    Bill = bill,
                    PaymentMethod = paymentMethod,
                    TotalAmount = baseTotal,
                    GrandTotal = grandTotalPaid,
                    FeedBack = feedBack,
                    PaidAt = DateTime.Now,
                    LowVAT = viewModel.LowVAT,
                    HighVAT = viewModel.HighVAT
                };

                _ordersService.FinishOrderAndFreeTable(order, payment);
                _ordersService.RemoveOrderFromSession(HttpContext, tableID);

                TempData["Success"] = $"Order has been successfully paid! Total: €{payment.TotalAmount}, Paid: €{payment.GrandTotal}, Tip: €{payment.TipAmount}";
                return RedirectToAction("Index", "Tables");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error finalizing payment: " + ex.Message;
                return RedirectToAction("Payment", new { id = tableID });
            }
        }
        [HttpGet]
        public IActionResult EqualSplitPayment(int tableId, int numberOfPeople = 0)
        {
            try
            {
                var order = _ordersService.GetOrderByTableId(tableId);
                var paymentViewModel = new PaymentViewModel { Order = order };

                var viewModel = new EqualSplitPaymentViewModel
                {
                    TableId = tableId,
                    TotalAmount = paymentViewModel.TotalAmount,
                    LowVAT = paymentViewModel.LowVAT,
                    HighVAT = paymentViewModel.HighVAT,
                    NumberOfPeople = numberOfPeople,
                    Payments = Enumerable.Repeat(new EqualIndividualPayment(), numberOfPeople).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load equal split payment screen: " + ex.Message;
                return RedirectToAction("Payment", new { id = tableId }); 
            }
        }


        [HttpPost]
        public IActionResult FinalizeEqualSplitPayment(EqualSplitPaymentViewModel model)
        {
            try
            {
                var order = _ordersService.GetOrderByTableId(model.TableId);
                Bill bill = _ordersService.GetBillByOrderId(order.OrderID);

                var viewModel = new PaymentViewModel
                {
                    Order = order
                };

                decimal totalBaseAmount = viewModel.TotalAmount;
                decimal expectedPerPerson = totalBaseAmount / model.NumberOfPeople;

                foreach (var perPerson in model.Payments)
                {
                    var payment = new Payment
                    {
                        Bill = bill,
                        PaymentMethod = perPerson.PaymentMethod,
                        TotalAmount = expectedPerPerson,
                        GrandTotal = perPerson.AmountPaid,
                        FeedBack = perPerson.Feedback,
                        PaidAt = DateTime.Now,
                        LowVAT = viewModel.LowVAT / model.NumberOfPeople,
                        HighVAT = viewModel.HighVAT / model.NumberOfPeople
                    };

                    _ordersService.FinishOrderAndFreeTable(order, payment);
                }

                _ordersService.RemoveOrderFromSession(HttpContext, model.TableId);

                TempData["Success"] = "Split payment completed.";
                return RedirectToAction("Index", "Tables");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error processing equal split payment: " + ex.Message;
                return RedirectToAction("EqualSplitPayment", new { tableId = model.TableId, numberOfPeople = model.NumberOfPeople });
            }
        }


        [HttpGet]
        public IActionResult MultiPartialPayment(int tableId)
        {
            var order = _ordersService.GetOrderByTableId(tableId);
            var viewModel = new PartialPaymentViewModel
            {
                TableId = tableId,
                TotalAmount = order.TotalAmount
            };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult FinalizeMultiPartialPayment(int TableId, decimal TotalAmount, string PaymentsJson)
        {
            try
            {
                var order = _ordersService.GetOrderByTableId(TableId);
                var bill = _ordersService.GetBillByOrderId(order.OrderID);

                var viewModel = new PaymentViewModel
                {
                    Order = order
                };

                var payments = JsonConvert.DeserializeObject<List<EqualIndividualPayment>>(PaymentsJson);

                decimal runningTotal = 0;

                foreach (var p in payments)
                {
                    runningTotal += p.AmountPaid;

                    var remainingBeforeThis = order.TotalAmount - (runningTotal - p.AmountPaid);
                    var tip = p.AmountPaid > remainingBeforeThis ? p.AmountPaid - remainingBeforeThis : 0;

                    var payment = new Payment
                    {
                        Bill = bill,
                        TotalAmount = order.TotalAmount,
                        GrandTotal = p.AmountPaid,
                        TipAmount = tip,
                        FeedBack = p.Feedback,
                        PaidAt = DateTime.Now,
                        PaymentMethod = p.PaymentMethod,
                        LowVAT = viewModel.LowVAT,
                        HighVAT = viewModel.HighVAT
                    };

                    _ordersService.FinishOrderAndFreeTable(order, payment);
                }

                _ordersService.RemoveOrderFromSession(HttpContext, TableId);

                TempData["Success"] = "All payments completed.";
                return RedirectToAction("Index", "Tables");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error finalizing multi partial payment: " + ex.Message;
                return RedirectToAction("MultiPartialPayment", new { tableId = TableId });
            }
        }


    }
}
