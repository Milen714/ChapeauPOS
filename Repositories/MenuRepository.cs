using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ChapeauPOS.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly string? _connectionString;
        public MenuRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDB");
        }

        //data mapping method for reading a row of data from the SQL database 
        private MenuItem ReadMenuItem(SqlDataReader reader)
        {
            //int MenuItemID = reader.GetInt32(0);
            //string ItemName = reader.GetString(1);
            //string ItemDescription = reader.GetString(2);
            //decimal ItemPrice = reader.GetDecimal(3);
            //bool VAT = !reader.IsDBNull(4) && reader.GetBoolean(4);
            //int CategoryID = reader.GetInt32(5);
            //string CategoryName = reader.GetString(6);
            //MenuCourse Course = (MenuCourse)Enum.Parse(typeof(MenuCourse), reader.GetString(7));

            int MenuItemID = reader.GetInt32(reader.GetOrdinal("MenuItemID"));
            string ItemName = reader.GetString(reader.GetOrdinal("ItemName"));
            string ItemDescription = reader["ItemDescription"] == DBNull.Value ? "No Description" : (string)reader["ItemDescription"];
            decimal ItemPrice = reader.GetDecimal(reader.GetOrdinal("ItemPrice"));
            bool VAT = reader.GetBoolean(reader.GetOrdinal("VAT"));
            int CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID"));
            string CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"));
            MenuCourse Course = Enum.Parse<MenuCourse>(reader.GetString(reader.GetOrdinal("Course")));
            bool IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
            int Stock = reader.IsDBNull(reader.GetOrdinal("Stock")) ? 0 : reader.GetInt32(reader.GetOrdinal("Stock"));

            MenuCategory category = new MenuCategory(CategoryID, CategoryName);
            var item = new MenuItem(MenuItemID, ItemName, ItemDescription, ItemPrice, VAT, category, Course);
            item.Stock = Stock;
            item.IsActive = IsActive;
            return item;
        }

        public List<MenuItem> GetAllMenuItems(bool includeInactive = false)
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT MI.MenuItemID, MI.ItemName, MI.ItemDescription, MI.ItemPrice, MI.VAT, MI.IsActive, MC.CategoryID, MC.CategoryName, MI.Course, MI.Stock " +
                                   "FROM MenuItems AS MI " +
                                   "JOIN MenuCategories AS MC ON MI.CategoryID = MC.CategoryID";

                    if (!includeInactive)
                        query += " WHERE MI.IsActive = 1";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MenuItem menuItem = ReadMenuItem(reader);// helpeing function to read the data
                            menuItems.Add(menuItem);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return menuItems;
        }

        public MenuItem GetMenuItemById(int id)
        {
            MenuItem menuItem = new MenuItem();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT MI.MenuItemID, MI.ItemName, MI.ItemDescription, MI.ItemPrice, MI.VAT, MI.IsActive, MC.CategoryID, MC.CategoryName, MI.Course, MI.Stock " +
                                   "FROM MenuItems AS MI " +
                                   "JOIN MenuCategories AS MC ON MI.CategoryID = MC.CategoryID " +
                                   "WHERE MI.MenuItemID = @MenuItemID";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@MenuItemID", id);//parameterized SQL,to prevent SQL injection attacks (security risk).
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            menuItem = ReadMenuItem(reader);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return menuItem;
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO MenuItems (ItemName, ItemDescription, ItemPrice, VAT, CategoryID, Course, IsActive) " +
                                   "VALUES (@Name, @Desc, @Price, @VAT, @CategoryID, @Course, 1)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Name", menuItem.ItemName);
                    command.Parameters.AddWithValue("@Desc", menuItem.ItemDescription ?? "");
                    command.Parameters.AddWithValue("@Price", menuItem.ItemPrice);
                    command.Parameters.AddWithValue("@VAT", menuItem.VAT);
                    command.Parameters.AddWithValue("@CategoryID", menuItem.Category.CategoryID);
                    command.Parameters.AddWithValue("@Course", menuItem.Course.ToString());

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE MenuItems SET ItemName = @Name, ItemDescription = @Desc, ItemPrice = @Price, VAT = @VAT, CategoryID = @CategoryID, Course = @Course, IsActive = @IsActive WHERE MenuItemID = @ID";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Name", menuItem.ItemName);
                    command.Parameters.AddWithValue("@Desc", menuItem.ItemDescription ?? "");
                    command.Parameters.AddWithValue("@Price", menuItem.ItemPrice);
                    command.Parameters.AddWithValue("@VAT", menuItem.VAT);
                    command.Parameters.AddWithValue("@CategoryID", menuItem.Category.CategoryID);
                    command.Parameters.AddWithValue("@Course", menuItem.Course.ToString());
                    command.Parameters.AddWithValue("@IsActive", menuItem.IsActive); 
                    command.Parameters.AddWithValue("@ID", menuItem.MenuItemID);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

            catch (Exception ex)
            {
                throw new Exception("Database update failed: " + ex.Message); 
            }
        }


        public void DeleteMenuItem(int id)
        {
            ToggleMenuItemStatus(id, false);
        }

        public void ToggleMenuItemStatus(int id, bool isActive)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE MenuItems SET IsActive = @IsActive WHERE MenuItemID = @ID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public List<MenuItem> FilterMenuItems(string course, string category)
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT MI.MenuItemID, MI.ItemName, MI.ItemDescription, MI.ItemPrice, MI.VAT, MI.IsActive, MC.CategoryID, MC.CategoryName, MI.Course, MI.Stock " +
                                   "FROM MenuItems AS MI " +
                                   "JOIN MenuCategories AS MC ON MI.CategoryID = MC.CategoryID " +
                                   "WHERE MI.IsActive = 1";

                    if (!string.IsNullOrEmpty(course))
                        query += " AND LOWER(MI.Course) = LOWER(@Course)";
                    if (!string.IsNullOrEmpty(category))
                        query += " AND LOWER(MC.CategoryName) = LOWER(@Category)";

                    SqlCommand command = new SqlCommand(query, connection);

                    if (!string.IsNullOrEmpty(course))
                        command.Parameters.AddWithValue("@Course", course.Trim());
                    if (!string.IsNullOrEmpty(category))
                        command.Parameters.AddWithValue("@Category", category.Trim());

                    //Console.WriteLine($"[DEBUG] Filtering by: Course = '{course}', Category = '{category}'");

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MenuItem menuItem = ReadMenuItem(reader);
                         
                            menuItems.Add(menuItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return menuItems;
        }

        public List<MenuItem> GetMenuItemsByCategory(MenuCategory category)
        {
            return FilterMenuItems(null, category.CategoryName);
        }

        public List<MenuItem> GetMenuItemsByCourse(MenuCourse course)
        {
            return FilterMenuItems(course.ToString(), null);
        }

        public List<MenuItem> GetMenuItemsByName(string name)
        {
            return GetAllMenuItems().Where(item => item.ItemName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<MenuCategory> GetMenuCategories()
        {
            List<MenuCategory> menuCategories = new List<MenuCategory>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT CategoryID, CategoryName FROM MenuCategories";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID"));
                            string CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"));
                            menuCategories.Add(new MenuCategory(CategoryID, CategoryName));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return menuCategories;
        }

        public void DeductStock(Order order)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    if (order.InterumOrderItems.Count() == 0)
                    {
                        foreach (var orderItem in order.OrderItems)
                        {
                            string query = "UPDATE MenuItems SET Stock = Stock - @Quantity WHERE MenuItemID = @MenuItemID";
                            SqlCommand command = new SqlCommand(query, connection);
                            command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);
                            command.Parameters.AddWithValue("@MenuItemID", orderItem.MenuItem.MenuItemID);
                            command.ExecuteNonQuery();
                        }
                    }
                    else if (order.InterumOrderItems.Count() != 0)
                    {
                        foreach (var orderItem in order.InterumOrderItems)
                        {
                            string query = "UPDATE MenuItems SET Stock = Stock - @Quantity WHERE MenuItemID = @MenuItemID";
                            SqlCommand command = new SqlCommand(query, connection);
                            command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);
                            command.Parameters.AddWithValue("@MenuItemID", orderItem.MenuItem.MenuItemID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
