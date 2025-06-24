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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Orders.OrderID, t.TableNumber, e.EmployeeID, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, oi.OrderItemStatus, Notes, ItemName, ItemDescription " +
                    "FROM Orders " +
                    "JOIN Tables t ON Orders.TableID = t.TableID " +
                    "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                    "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                    "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                    "WHERE (oi.OrderItemStatus <> 'Served' AND oi.OrderItemStatus <> 'Ready') AND mi.Course <> 'Drink' " +
                    "ORDER BY Orders.CreatedAt";

                SqlCommand cmd = new SqlCommand(query, conn);

                try
                {
                    conn.Open();
                    Order? currentOrder = null;
                    int lastOrderId = -1;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int currentOrderId = (int)reader["OrderID"];

                            // Only create new Order when OrderID changes
                            if (currentOrderId != lastOrderId)
                            {
                                currentOrder = MapOrder(reader);
                                orders.Add(currentOrder);
                                lastOrderId = currentOrderId;
                            }

                            // Always add the order item to current order
                            currentOrder?.OrderItems.Add(MapOrderItem(reader));
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
            DateTime createdAt = (DateTime)reader["CreatedAt"];
            DateTime? closedAt = reader["ClosedAt"] == DBNull.Value ? null : (DateTime?)reader["ClosedAt"];

            Table table = new Table { TableNumber = tableNumber };
            Employee employee = new Employee { EmployeeId = employeeID };

            return new Order(orderId, table, employee, createdAt, closedAt);
        }

        private OrderItem MapOrderItem(SqlDataReader reader)
        {
            int orderItemID = (int)reader["OrderItemID"];
            int menuItemID = (int)reader["MenuItemID"];
            int quantity = (int)reader["Quantity"];
            MenuCourse menuCourse = (MenuCourse)Enum.Parse(typeof(MenuCourse), reader["Course"].ToString());
            OrderItemStatus orderItemStatus = reader["OrderItemStatus"] == DBNull.Value ? OrderItemStatus.Ordered : (OrderItemStatus)Enum.Parse(typeof(OrderItemStatus), reader["OrderItemStatus"].ToString());
            string notes = reader["Notes"] == DBNull.Value ? "" : (string)reader["Notes"];
            string itemName = (string)reader["ItemName"];
            string itemDescription = reader["ItemDescription"] == DBNull.Value ? "" : (string)reader["ItemDescription"];

            MenuItem menuItem = new MenuItem { MenuItemID = menuItemID, ItemName = itemName, ItemDescription = itemDescription, Course = menuCourse };
            return new OrderItem(orderItemID, menuItem, quantity, orderItemStatus, notes);
        }

        public void UpdateKitchenOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE OrderItems SET OrderItemStatus = @Status " +
                    "WHERE OrderItemID = @OrderItemID";
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

        public List<Order> GetFinishedKitchenOrders()
        {
            List<Order> orders = new List<Order>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Orders.OrderID, t.TableNumber, e.EmployeeID, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, oi.OrderItemStatus, Notes, ItemName, ItemDescription " +
                    "FROM Orders " +
                    "JOIN Tables t ON Orders.TableID = t.TableID " +
                    "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                    "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                    "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                    "WHERE (oi.OrderItemStatus = 'Served' OR oi.OrderItemStatus = 'Ready') AND mi.Course <> 'Drink' AND CAST(Orders.CreatedAt AS DATE) = CAST(DATEADD(HOUR, 2, GETUTCDATE()) AS DATE) " +
                    "ORDER BY Orders.CreatedAt";

                SqlCommand cmd = new SqlCommand(query, conn);

                try
                {
                    conn.Open();
                    Order? currentOrder = null;
                    int lastOrderId = -1;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int currentOrderId = (int)reader["OrderID"];

                            if (currentOrderId != lastOrderId)
                            {
                                currentOrder = MapOrder(reader);
                                orders.Add(currentOrder);
                                lastOrderId = currentOrderId;
                            }

                            currentOrder?.OrderItems.Add(MapOrderItem(reader));
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

        public List<Order> GetRunningBarOrders()
        {
            List<Order> orders = new List<Order>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Orders.OrderID, t.TableNumber, e.EmployeeID, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, oi.OrderItemStatus, Notes, ItemName, ItemDescription " +
                    "FROM Orders " +
                    "JOIN Tables t ON Orders.TableID = t.TableID " +
                    "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                    "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                    "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                    "WHERE (oi.OrderItemStatus <> 'Served' AND oi.OrderItemStatus <> 'Ready') AND mi.Course = 'Drink' " +
                    "ORDER BY Orders.CreatedAt";

                SqlCommand cmd = new SqlCommand(query, conn);

                try
                {
                    conn.Open();
                    Order? currentOrder = null;
                    int lastOrderId = -1;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int currentOrderId = (int)reader["OrderID"];

                            if (currentOrderId != lastOrderId)
                            {
                                currentOrder = MapOrder(reader);
                                orders.Add(currentOrder);
                                lastOrderId = currentOrderId;
                            }

                            currentOrder?.OrderItems.Add(MapOrderItem(reader));
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

        public void UpdateBarOrderItemStatus(int orderItemId, OrderItemStatus orderItemStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE OrderItems SET OrderItemStatus = @Status " +
                               "WHERE OrderItemID = @OrderItemID ";
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
                    throw new Exception("Something went wrong while updating order status.", ex);
                }
            }

        }

        public void CloseFoodOrder(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE oi SET OrderItemStatus = 'Ready' " +
                               "FROM OrderItems oi " +
                               "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                               "WHERE OrderID = @OrderID AND mi.Course <> 'Drink'; " +
                               "UPDATE Orders SET ClosedAt = GETDATE() " +
                               "WHERE OrderID = @OrderID";
                SqlCommand cmd = new SqlCommand(query, conn);
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

        public void CloseDrinkOrder(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE oi SET OrderItemStatus = 'Ready' " +
                               "FROM OrderItems oi " +
                               "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                               "WHERE OrderID = @OrderID AND mi.Course = 'Drink'; " +
                               "UPDATE Orders SET ClosedAt = GETDATE() " +
                               "WHERE OrderID = @OrderID";
                SqlCommand cmd = new SqlCommand(query, conn);
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

        public void UpdateItemStatusBasedOnCourse(int orderId, MenuCourse course, OrderItemStatus orderItemStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE oi SET OrderItemStatus = @OrderItemStatus " +
                               "FROM OrderItems oi " +
                               "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                               "WHERE OrderID = @OrderID AND mi.Course = @Course AND oi.OrderItemStatus <> 'Ready'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderItemStatus", orderItemStatus.ToString());
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@Course", course.ToString());

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
        public void SetCourseToServed(int orderId, MenuCourse course, OrderItemStatus orderItemStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE oi SET OrderItemStatus = @OrderItemStatus " +
                               "FROM OrderItems oi " +
                               "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                               "WHERE OrderID = @OrderID AND mi.Course = @Course AND oi.OrderItemStatus = 'Ready' ";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderItemStatus", orderItemStatus.ToString());
                cmd.Parameters.AddWithValue("@OrderID", orderId);
                cmd.Parameters.AddWithValue("@Course", course.ToString());

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


        public List<Order> GetFinishedBarOrders()
        {
            List<Order> orders = new List<Order>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Orders.OrderID, t.TableNumber, e.EmployeeID, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, oi.OrderItemStatus, Notes, ItemName, ItemDescription " +
                    "FROM Orders " +
                    "JOIN Tables t ON Orders.TableID = t.TableID " +
                    "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                    "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                    "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                    "WHERE (oi.OrderItemStatus = 'Served' OR oi.OrderItemStatus = 'Ready') AND mi.Course = 'Drink' AND CAST(Orders.CreatedAt AS DATE) = CAST(DATEADD(HOUR, 2, GETUTCDATE()) AS DATE)" +
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
                            int currentOrderId = (int)reader["OrderID"];

                            if (currentOrderId != lastOrderId)
                            {
                                currentOrder = MapOrder(reader);
                                orders.Add(currentOrder);
                                lastOrderId = currentOrderId;
                            }

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
    }
}
