using System.Collections;
using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using static Azure.Core.HttpHeader;

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

            return new Order
            {
                OrderID = orderId,
                Table = table,
                Employee = employee,
                OrderStatus = orderStatus,
                CreatedAt = createdAt,
                ClosedAt = closedAt,
                OrderItems = new List<OrderItem>()
            };
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
            bool VAT = reader.GetBoolean(reader.GetOrdinal("VAT"));
            int stock = reader.GetInt32(reader.GetOrdinal("Stock"));

            MenuItem menuItem = new MenuItem { MenuItemID = menuItemID, 
                                               ItemName = itemName, ItemDescription = itemDescription, 
                                               ItemPrice = itemPrice, VAT = VAT, 
                                               Stock = stock, Course = menuCourse };
            return new OrderItem(orderItemID, menuItem, quantity, orderItemStatus, notes);
        }
        private Bill ReadBill(SqlDataReader reader)
        {
            int billId = (int)reader["BillID"];
            int orderId = (int)reader["OrderID"];
            DateTime createdAt = (DateTime)reader["CreatedAt"];
            DateTime? closedAt = reader["ClosedAt"] == DBNull.Value ? null : (DateTime?)reader["ClosedAt"];
            decimal subtotal = (decimal)reader["Subtotal"];
            int finalizedBy = (int)reader["FinalizedBy"];
            Employee employee = new Employee { EmployeeId = finalizedBy };
            return new Bill
            {
                BillID = billId,
                CreatedAt = createdAt,
                ClosedAt = closedAt,
                Subtotal = subtotal,
                FinalizedBy = employee
            };
        }
        public List<Order> GetAllOrders()
        {
            List<Order> orders = new List<Order>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Orders.OrderID, t.TableNumber, Orders.EmployeeID, OrderStatus, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, mi.VAT,  oi.OrderItemStatus, Notes, ItemName, ItemDescription, mi.ItemPrice, mi.Stock " +
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
                    string BillQuery = "INSERT INTO Bills (OrderID,CreatedAt,Subtotal,FinalizedBy)" +
                                       "VALUES(@OrderID,@CreatedAt,@Subtotal,@FinalizedBy)";
                    SqlCommand Billcommand = new SqlCommand(BillQuery, connection);
                    Billcommand.Parameters.AddWithValue("@OrderID", newOrderId);
                    Billcommand.Parameters.AddWithValue("@CreatedAt",DateTime.Now);
                    Billcommand.Parameters.AddWithValue("@Subtotal",order.TotalAmount);
                    Billcommand.Parameters.AddWithValue("@FinalizedBy",order.Employee.EmployeeId);

                    Billcommand.ExecuteNonQuery();
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
        public void UpdateOrderItem(OrderItem orderItem)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE OrderItems SET MenuItemID = @MenuItemID, Quantity = @Quantity, OrderItemStatus = @OrderItemStatus, Notes = @Notes " +
                                   "WHERE OrderItemID = @OrderItemID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@MenuItemID", orderItem.MenuItem.MenuItemID);
                    command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);
                    command.Parameters.AddWithValue("@OrderItemStatus", orderItem.OrderItemStatus.ToString());
                    command.Parameters.AddWithValue("@Notes", (object)orderItem.Notes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@OrderItemID", orderItem.OrderItemId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating order item in database", ex);
            }
        }
        public void DeleteOrder(int orderId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "DELETE FROM Orders WHERE OrderID = @OrderID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting order from database", ex);
            }
        }
        public List<Order> GetOrdersByTableId(int tableId)// not by tableID BUT BY TABLE NUMBER
        {
            List<Order> orders = new List<Order>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Orders.OrderID, t.TableNumber, Orders.EmployeeID, OrderStatus, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, oi.OrderItemStatus, Notes, ItemName, ItemDescription, mi.ItemPrice, mi.VAT, mi.Stock " +
                   "FROM Orders " +
                   "JOIN Tables t ON Orders.TableID = t.TableID " +
                   "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                   "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                   "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                   "WHERE t.TableNumber = @TableNumber  ";
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
            List<Order> orders = new List<Order>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Orders.OrderID, t.TableNumber, Orders.EmployeeID, OrderStatus, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, oi.OrderItemStatus, Notes, ItemName, ItemDescription, mi.ItemPrice, mi.VAT, mi.Stock " +
                   "FROM Orders " +
                   "JOIN Tables t ON Orders.TableID = t.TableID " +
                   "JOIN Employees e ON Orders.EmployeeID = e.EmployeeID " +
                   "JOIN OrderItems oi ON Orders.OrderID = oi.OrderID " +
                   "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                   "WHERE OrderStatus = @OrderStatus  ";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OrderStatus", status.ToString());
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int orderId = (int)reader["OrderID"];
                            // Try to find the order in the list
                            var order = orders.FirstOrDefault(o => o.OrderID == orderId);
                            if (order == null)
                            {
                                order = ReadOrder(reader);
                                orders.Add(order);
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
                throw new Exception("Error retrieving orders from database", ex);
            }
            return orders;
        }

        public Order GetOrderByTableId(int tableId)//TABLE NUMBER
        {
            Order order = new Order();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Orders.OrderID, t.TableNumber, Orders.EmployeeID, OrderStatus, Orders.CreatedAt, ClosedAt, oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, oi.OrderItemStatus, Notes, ItemName, ItemDescription, mi.ItemPrice, mi.VAT, mi.Stock " +
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

        public OrderItem GetOrderItemById(int id)
        {
            OrderItem orderItem = new OrderItem();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT oi.OrderItemID, oi.MenuItemID, oi.Quantity, oi.OrderItemStatus, oi.Notes, mi.ItemName, mi.ItemDescription, mi.ItemPrice, mi.Course, mi.VAT, mi.Stock " +
                    "FROM OrderItems oi " +
                    "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                    "WHERE oi.OrderItemID = @OrderItemID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OrderItemID", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderItem = ReadOrderItem(reader);
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
            return orderItem;
        }

        public void RemoveOrderItem(int orderId, int orderItemId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "DELETE FROM OrderItems WHERE OrderID = @OrderID AND OrderItemID = @OrderItemID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    command.Parameters.AddWithValue("@OrderItemID", orderItemId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error removing order item from database", ex);
            }
        }

        public void AddToOrder(Order order)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {   connection.Open();

                    foreach (var item in order.InterumOrderItems)
                    {   //Insert each order item into the OrderItems table Using the new OrderID
                        string itemQuery = "INSERT INTO OrderItems (OrderID, MenuItemID, Quantity, OrderItemStatus, Notes) " +
                                           "VALUES (@OrderID, @MenuItemID, @Quantity, @OrderItemStatus, @Notes)";
                        SqlCommand itemCommand = new SqlCommand(itemQuery, connection);
                        itemCommand.Parameters.AddWithValue("@OrderID", order.OrderID);
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
                Console.WriteLine(ex.Message);
                throw new Exception("Error connecting to database", ex);
                
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding order to database", ex);
            }
        }

        public void MoveOrderToAnotherTable(int tableId, Order order)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = " UPDATE Orders SET TableID = @TableID  " +
                                   " WHERE OrderID = @OrderID; " +
                                   " UPDATE Tables SET TableStatus = @TableStatus " +
                                   " WHERE TableID = @TableID; ";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TableID", tableId);
                    command.Parameters.AddWithValue("@OrderID", order.OrderID);
                    command.Parameters.AddWithValue("@TableStatus", TableStatus.Occupied.ToString());

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating order item in database", ex);
            }
        }
        public void SavePayment(Payment payment)
        {
            try
            {
                using(SqlConnection connection=new SqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO Payments ( BillID, Method, TotalAmount, Feedback, PaidAt, TipAmount, LowVAT, HighVAT, GrandTotal)
                                   VALUES (@BillID, @Method, @TotalAmount, @Feedback, @PaidAt, @TipAmount, @LowVAT, @HighVAT, @GrandTotal)";
                    SqlCommand command = new SqlCommand(query, connection);
                   
                    command.Parameters.AddWithValue("@BillID", payment.Bill.BillID);
                    command.Parameters.AddWithValue("@Method", payment.PaymentMethod.ToString());
                    command.Parameters.AddWithValue("@TotalAmount", payment.TotalAmount);
                    command.Parameters.AddWithValue("@Feedback",(object)payment.FeedBack ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PaidAt", payment.PaidAt);
                    command.Parameters.AddWithValue("@TipAmount", payment.TipAmount);
                    command.Parameters.AddWithValue("@LowVAT", payment.LowVAT);
                    command.Parameters.AddWithValue("@HighVAT", payment.HighVAT);
                    command.Parameters.AddWithValue("@GrandTotal", payment.GrandTotal);

                    connection.Open();
                    command.ExecuteNonQuery();

                }
            }
            catch(SqlException ex)
            {
                throw new Exception("Error connecting to database",ex);
            }
            catch(Exception ex)
            {
                throw new Exception("Error inserting payment information in database", ex);
            }
        }
        public void FinalizeOrder(int orderID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"UPDATE Orders SET OrderStatus=@OrderStatus " +
                                   "WHERE OrderID = @OrderID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OrderStatus", "Finalized");
                    //command.Parameters.AddWithValue("@ClosedAt", DateTime.Now.ToString());
                    command.Parameters.AddWithValue("@OrderID", orderID);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database",ex);
            }
            catch(Exception ex)
            {
                throw new Exception("Error updating table to finalize in database",ex);
            }
        }

        public Bill GetBillByOrderId(int orderId)
        {
            Bill bill = new Bill();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT B.BillID, B.OrderID, B.CreatedAt, B.ClosedAt, B.Subtotal, B.FinalizedBy, " +
                                   "O.OrderID, t.TableNumber, O.EmployeeID, O.OrderStatus, O.CreatedAt, O.ClosedAt, " +
                                   "oi.OrderItemID, oi.MenuItemID, oi.Quantity, mi.Course, mi.VAT, oi.OrderItemStatus, " +
                                   "Notes, ItemName, ItemDescription, mi.ItemPrice, mi.Stock " +
                                   "FROM Bills B " +
                                   "JOIN Orders O ON B.OrderID = O.OrderID " +
                                   "JOIN Tables t ON O.TableID = t.TableID " +
                                   "JOIN Employees e ON O.EmployeeID = e.EmployeeID " +
                                   "JOIN OrderItems oi ON O.OrderID = oi.OrderID " +
                                   "JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID " +
                                   "WHERE B.OrderID = @OrderID";



                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@OrderID", orderId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Order order = new Order();
                        bool firstRow = true;
                        while (reader.Read())
                        {
                            if (firstRow)
                            {
                                bill = ReadBill(reader);
                                order = ReadOrder(reader);
                                firstRow = false;
                            }

                            OrderItem orderItem = ReadOrderItem(reader);
                            order.OrderItems.Add(orderItem);
                        }
                        bill.Order = order; // Associate the order with the bill

                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving bill from database", ex);
            }
            return bill;
        }
    }

}
