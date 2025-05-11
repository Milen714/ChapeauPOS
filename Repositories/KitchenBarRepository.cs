using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ChapeauPOS.Repositories
{
    public class KitchenBarRepository : IKitchenBarRepository
    {
        private readonly string? _connectionString;
        public KitchenBarRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDB");
        }

        public List<Order> GetRunningKitchenOrders()
        {
            List<Order> orders = new List<Order>();
            List<OrderItem> orderItems = new List<OrderItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Orders.OrderID, t.TableNumber, Orders.EmployeeID, OrderStatus, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, oi.MenuCourse, oi.OrderItemStatus, Notes, ItemName, ItemDescription " +
                    "FROM Orders " +
                    "JOIN Tables t ON Orders.TableID = t.TableID " +
                    "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                    "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                    "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                    "WHERE Orders.OrderStatus <> 'Served' AND oi.MenuCourse <> 'Drink'" +
                    "ORDER BY Orders.CreatedAt";

                SqlCommand cmd = new SqlCommand(query, conn);

                try
                {
                    conn.Open();
                    Order currentOrder = null;
                    int lastOrderId = -1;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Order order = MapOrder(reader);
                            //order.OrderItems.Add(MapOrderItem(reader));
                            //orders.Add(order);
                            int currentOrderId = (int)reader["OrderID"];

                            // Only create new Order when OrderID changes
                            if (currentOrderId != lastOrderId)
                            {
                                currentOrder = MapOrder(reader);
                                orders.Add(currentOrder);
                                lastOrderId = currentOrderId;
                            }

                            // Always add the order item to current order
                            currentOrder.OrderItems.Add(MapOrderItem(reader));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Something went wrong with the database.", ex);
                }
            }

            return orders;
        }

        private Order MapOrder(SqlDataReader reader)
        {
            int orderId = (int)reader["OrderID"];
            int tableNumber = (int)reader["TableNumber"];
            int employeeID = (int)reader["EmployeeID"];
            OrderStatus orderStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), reader["OrderStatus"].ToString());
            DateTime createdAt = (DateTime)reader["CreatedAt"];
            DateTime? closedAt = reader["ClosedAt"] is DBNull ? null : (DateTime?)reader["ClosedAt"];
           
            Table table = new Table { TableNumber = tableNumber };
            Employee employee = new Employee { EmployeeId = employeeID };

            return new Order(orderId, table, employee, orderStatus, createdAt, closedAt);
        }

        private OrderItem MapOrderItem(SqlDataReader reader)
        {
            int orderItemID = (int)reader["OrderItemID"];
            int menuItemID = (int)reader["MenuItemID"];
            int quantity = (int)reader["Quantity"];
            MenuCourse menuCourse = (MenuCourse)Enum.Parse(typeof(MenuCourse), reader["MenuCourse"].ToString());
            OrderItemStatus orderItemStatus = (OrderItemStatus)Enum.Parse(typeof(OrderItemStatus), reader["OrderItemStatus"].ToString());
            string notes = reader["Notes"] == DBNull.Value ? "" : (string)reader["Notes"];
            string itemName = (string)reader["ItemName"];
            string itemDescription = reader["ItemDescription"] == DBNull.Value ? "" : (string)reader["ItemDescription"];

            MenuItem menuItem = new MenuItem { MenuItemID = menuItemID, ItemName = itemName, ItemDescription = itemDescription };
            return new OrderItem(orderItemID, menuItem, quantity, menuCourse, orderItemStatus, notes);
        }

        public void UpdateOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE OrderItems SET OrderItemStatus = @Status WHERE OrderItemID = @OrderItemID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Status", orderItemStatus.ToString());
                cmd.Parameters.AddWithValue("@OrderItemID", orderItemId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Something went wrong while updating order item status.", ex);
                }
            }

        }

        public void UpdateOrderStatus(int orderId, OrderStatus orderStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                    string query = "UPDATE Orders SET OrderStatus = @Status WHERE OrderID = @OrderID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Status", orderStatus.ToString());
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Something went wrong while updating order status.", ex);
                }
            }
            
        }
    }
}
