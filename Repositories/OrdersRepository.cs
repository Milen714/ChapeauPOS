using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ChapeauPOS.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly string? _connectionString;
        public OrdersRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDB");
        }

        private Order ReadOrder(SqlDataReader reader)
        {
            int orderId = (int)reader["OrderID"];
            int tableNumber = (int)reader["TableNumber"];
            int employeeID = (int)reader["EmployeeID"];
            OrderStatus orderStatus = reader["OrderStatus"] == DBNull.Value ? OrderStatus.Ordered : (OrderStatus)Enum.Parse(typeof(OrderStatus), reader["OrderStatus"].ToString());
            DateTime createdAt = (DateTime)reader["CreatedAt"];
            DateTime? closedAt = reader["ClosedAt"] == DBNull.Value ? null : (DateTime?)reader["ClosedAt"];

            Table table = new Table { TableNumber = tableNumber };
            Employee employee = new Employee { EmployeeId = employeeID };

            return new Order(orderId, table, employee, orderStatus, createdAt, closedAt);
        }

        private OrderItem ReadOrderItem(SqlDataReader reader)
        {
            int orderItemID = (int)reader["OrderItemID"];
            int menuItemID = (int)reader["MenuItemID"];
            int quantity = (int)reader["Quantity"];
            decimal itemPrice = (decimal)reader["ItemPrice"];
            MenuCourse menuCourse = (MenuCourse)Enum.Parse(typeof(MenuCourse), reader["Course"].ToString());
            OrderItemStatus orderItemStatus = reader["OrderItemStatus"] == DBNull.Value ? OrderItemStatus.Ordered : (OrderItemStatus)Enum.Parse(typeof(OrderItemStatus), reader["OrderItemStatus"].ToString());
           // CourseStatus courseStatus = reader["CourseStatus"] == DBNull.Value ? CourseStatus.Ordered : (CourseStatus)Enum.Parse(typeof(CourseStatus), reader["CourseStatus"].ToString());
            string notes = reader["Notes"] == DBNull.Value ? "" : (string)reader["Notes"];
            string itemName = (string)reader["ItemName"];
            string itemDescription = reader["ItemDescription"] == DBNull.Value ? "" : (string)reader["ItemDescription"];

            MenuItem menuItem = new MenuItem { MenuItemID = menuItemID, ItemName = itemName, ItemDescription = itemDescription, ItemPrice = itemPrice };
            return new OrderItem(orderItemID, menuItem, quantity, menuCourse, orderItemStatus, notes);
        }
        public List<Order> GetAllOrders()
        {
            List<Order> orders = new List<Order>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Orders.OrderID, t.TableNumber, Orders.EmployeeID, OrderStatus, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course,  oi.OrderItemStatus, Notes, ItemName, ItemDescription, mi.ItemPrice " +
                    "FROM Orders " +
                    "JOIN Tables t ON Orders.TableID = t.TableID " +
                    "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                    "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                    "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID ";
                    //"WHERE Orders.OrderStatus <> 'Served' AND oi.MenuCourse <> 'Drink' " +
                    //"ORDER BY Orders.CreatedAt"
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = ReadOrder(reader);
                            OrderItem orderItem = ReadOrderItem(reader);
                            order.OrderItems.Add(orderItem);
                            orders.Add(order);
                        }

                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving orders from database", ex);
            }
            return orders;
        }
        public Order GetOrderById(int orderId)
        {
            throw new NotImplementedException();
        }
        public void AddOrder(Order order)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {   //First insert the order into the Orders table and get the new OrderID
                    string query = "INSERT INTO Orders (TableID ,EmployeeID, OrderStatus, CreatedAt) " +
                                   "VALUES (@TableID, @EmployeeID, @OrderStatus, @CreatedAt); SELECT SCOPE_IDENTITY();";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TableID", order.Table.TableID);
                    command.Parameters.AddWithValue("@EmployeeID", order.Employee.EmployeeId);
                    command.Parameters.AddWithValue("@OrderStatus", order.OrderStatus.ToString());
                    command.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);
                    connection.Open();
                    int newOrderId = Convert.ToInt32(command.ExecuteScalar());
                    foreach (var item in order.OrderItems)
                    {   //Insert each order item into the OrderItems table Using the new OrderID
                        string itemQuery = "INSERT INTO OrderItems (OrderID, MenuItemID, Quantity, OrderItemStatus, Notes) " +
                                           "VALUES (@OrderID, @MenuItemID, @Quantity, @OrderItemStatus, @Notes)";
                        SqlCommand itemCommand = new SqlCommand(itemQuery, connection);
                        itemCommand.Parameters.AddWithValue("@OrderID", newOrderId);
                        itemCommand.Parameters.AddWithValue("@MenuItemID", item.MenuItem.MenuItemID);
                        itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                        //itemCommand.Parameters.AddWithValue("@MenuCourse", item.MenuCourse.ToString());
                        itemCommand.Parameters.AddWithValue("@OrderItemStatus", item.OrderItemStatus.ToString());
                       // itemCommand.Parameters.AddWithValue("@CourseStatus", item.CourseStatus.ToString());
                        itemCommand.Parameters.AddWithValue("@Notes", (object)item.Notes ?? DBNull.Value);

                        itemCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding order to database", ex);
            }
        }
        public void UpdateOrder(Order order)
        {
            throw new NotImplementedException();
        }
        public void DeleteOrder(int orderId)
        {
            throw new NotImplementedException();
        }
        public List<Order> GetOrdersByTableId(int tableId)// not by tableID BUT BY TABLE NUMBER
        {
            List<Order> orders = new List<Order>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Orders.OrderID, t.TableNumber, Orders.EmployeeID, OrderStatus, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, oi.OrderItemStatus, oi.CourseStatus, Notes, ItemName, ItemDescription, mi.ItemPrice " +
                    "FROM Orders " +
                    "JOIN Tables t ON Orders.TableID = t.TableID " +
                    "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                    "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                    "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                    "WHERE t.TableNumber = @TableNumber";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TableNumber", tableId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order order = ReadOrder(reader);
                            OrderItem orderItem = ReadOrderItem(reader);
                            order.OrderItems.Add(orderItem);
                            orders.Add(order);
                        }

                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving orders from database", ex);
            }
            return orders;
        }
        
        public List<Order> GetOrdersByEmployeeId(int employeeId)
        {
            throw new NotImplementedException();
        }
        public List<Order> GetOrdersByStatus(OrderStatus status)
        {
            throw new NotImplementedException();
        }

        public Order GetOrderByTableId(int tableId)
        {
            Order order = new Order();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Orders.OrderID, t.TableNumber, Orders.EmployeeID, OrderStatus, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, oi.OrderItemStatus, Notes, ItemName, ItemDescription, mi.ItemPrice " +
                    "FROM Orders " +
                    "JOIN Tables t ON Orders.TableID = t.TableID " +
                    "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                    "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                    "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                    "WHERE t.TableNumber = @TableNumber AND OrderStatus IN ('Ordered', 'Served', 'Ready', 'Preparing') ";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TableNumber", tableId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {// Loop to return all of the data from the OrderItems table
                     // Since order = ReadOrder(reader); returns only one line OrderItem orderItem = ReadOrderItem(reader);
                     // will retruns only one line too if order = ReadOrder(reader); not wrapped in a if(bool=true)
                        bool firstRow = true;
                        while (reader.Read())
                        {
                            if (firstRow)
                            {
                                order = ReadOrder(reader); 
                                firstRow = false;
                            }

                            OrderItem orderItem = ReadOrderItem(reader); 
                            order.OrderItems.Add(orderItem);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving order from database", ex);
            }
            return order;

        }
    }
    
}
