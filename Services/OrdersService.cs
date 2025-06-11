using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Repositories;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;

namespace ChapeauPOS.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ITableRepository _tableRepository;
        public OrdersService(IOrdersRepository ordersRepository, ITableRepository tableRepository)
        {
            _ordersRepository = ordersRepository;
            _tableRepository = tableRepository;
        }
        // Implement the methods from IOrdersService here
        public List<Order> GetAllOrders()
        {
            return _ordersRepository.GetAllOrders();
        }
        public Order GetOrderById(int orderId)
        {
            return _ordersRepository.GetOrderById(orderId);
        }
        public void AddOrder(Order order)
        {
            _ordersRepository.AddOrder(order);
        }
        public void UpdateOrderItem(OrderItem orderItem)
        {
            _ordersRepository.UpdateOrderItem(orderItem);
        }
        public void DeleteOrder(int orderId)
        {
            _ordersRepository.DeleteOrder(orderId);
        }

        public List<Order> GetOrdersByTableId(int tableId)
        {
            return _ordersRepository.GetOrdersByTableId(tableId);
        }

        public List<Order> GetOrdersByEmployeeId(int employeeId)
        {
            return _ordersRepository.GetOrdersByEmployeeId(employeeId);
        }

        public List<Order> GetOrdersByStatus(OrderStatus status)
        {
            return _ordersRepository.GetOrdersByStatus(status);
        }

        public Order GetOrderByTableId(int tableId)
        {
            return _ordersRepository.GetOrderByTableId(tableId);
        }

        public OrderItem GetOrderItemById(int id)
        {
            return _ordersRepository.GetOrderItemById(id);
        }

        public void RemoveOrderItem(int orderId, int orderItemId)
        {
            _ordersRepository.RemoveOrderItem(orderId, orderItemId);
        }

        private const string OrderSessionKeyPrefix = "TableOrder_";

        public Order GetOrderFromSession(HttpContext context, int tableId)
        {
            return context.Session.GetObject<Order>($"{OrderSessionKeyPrefix}{tableId}") ?? new Order
            {
                OrderItems = new List<OrderItem>(),
                CreatedAt = DateTime.Now
            };
        }

        public void SaveOrderToSession(HttpContext context, int tableId, Order order)
        {
            context.Session.SetObject($"{OrderSessionKeyPrefix}{tableId}", order);
        }

        public void RemoveOrderFromSession(HttpContext context, int tableId)
        {
            context.Session.Remove($"{OrderSessionKeyPrefix}{tableId}");
        }

        public void AddMenuItemToExistingOrder(int itemId, string? note, MenuItem menuItem, Order order)
        {
            // Normalize note to avoid null reference issues
            string normalizedNote = note?.Trim() ?? string.Empty;

            // Check if the item already exists in the order, as well as if the notes are the same
            var existingItem = order.OrderItems.FirstOrDefault(oi =>
                oi.MenuItem.MenuItemID == itemId &&
                string.Equals(oi.Notes?.Trim() ?? string.Empty, normalizedNote, StringComparison.OrdinalIgnoreCase)
            );
            var existingInterumItem = order.InterumOrderItems.FirstOrDefault(oi =>
                oi.MenuItem.MenuItemID == itemId &&
                string.Equals(oi.Notes?.Trim() ?? string.Empty, normalizedNote, StringComparison.OrdinalIgnoreCase)
            );

            // If the item exists,and it is not stored in the Database, increase the quantity
            if (existingItem != null && order.OrderStatus == OrderStatus.Pending)
            {
                existingItem.Quantity++;
            }
            else if (existingInterumItem != null)
            {
                existingInterumItem.Quantity++;
            } //If the item exists, and it IS stored in the Database, increase the quantity
            else
            {
                // If the item doesn't exist, create a new order item
                OrderItem orderItem = new OrderItem
                {
                    MenuItem = menuItem,
                    Quantity = 1,
                    Notes = normalizedNote,
                    OrderItemStatus = OrderItemStatus.Ordered
                };// If the order is not in the DataBase place the new orderItem in the Order
                if (order.OrderStatus == OrderStatus.Pending)
                {
                    order.OrderItems.Add(orderItem);
                }// If the order is already in the DataBase place the new orderItem
                 // in a temporary List awaiting to be send to the DB where it will append the first
                 // Items in the same order
                else
                {
                    order.InterumOrderItems.Add(orderItem);
                }
            }
            order.TemporaryItemIdSetter();
        }

        public void AddToOrder(Order order)
        {
            _ordersRepository.AddToOrder(order);
        }

        public void MoveOrderToAnotherTable(int tableId, Order order)
        {
            _ordersRepository.MoveOrderToAnotherTable(tableId, order);
        }
        public void FinishOrderAndFreeTable(Order order, Payment payment)
        {
            _ordersRepository.SavePayment(payment);
            _ordersRepository.FinalizeOrder(order.OrderID);
            _tableRepository.UpdateTableStatus(order.Table.TableNumber, TableStatus.Free);
        }

        public Bill GetBillByOrderId(int orderId)
        {
            return _ordersRepository.GetBillByOrderId(orderId);
        }        


    }

}
