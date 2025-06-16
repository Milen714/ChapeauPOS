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

        private MenuItem ReadMenuItem(SqlDataReader reader)
        {
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
                        query += " WHERE MI.IsActive = 1";// Filtering and taking only active items

                    SqlCommand command = new SqlCommand(query, connection);
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
                Console.WriteLine("Error in GetAllMenuItems: " + ex.Message);
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
                    command.Parameters.AddWithValue("@MenuItemID", id);
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
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetMenuItemById: " + ex.Message);
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
                                   "VALUES (@Name, @Desc, @Price, @VAT, @CategoryID, @Course, 1)"; // Always active on insert

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
                Console.WriteLine("Error in AddMenuItem: " + ex.Message);
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
            catch (Exception ex)
            {
                throw new Exception("Database update failed: " + ex.Message);
            }
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
                Console.WriteLine("Error in ToggleMenuItemStatus: " + ex.Message);
            }
        }

        public void ActivateMenuItem(int id)
        {
            // Set the menu item status to active (true)
            ToggleMenuItemStatus(id, true);
        }

        public void DeactivateMenuItem(int id)
        {
            // Set the menu item status to inactive (false)
            ToggleMenuItemStatus(id, false);
        }


        //public void DeleteMenuItem(int id) => ToggleMenuItemStatus(id, false); // Soft delete

        public void DeleteMenuItem(int id)
        {

        }

        public List<MenuItem> FilterMenuItems(string course, string category, bool includeInactive)
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT MI.MenuItemID, MI.ItemName, MI.ItemDescription, MI.ItemPrice, MI.VAT, MI.IsActive, MC.CategoryID, MC.CategoryName, MI.Course, MI.Stock " +
                                   "FROM MenuItems AS MI " +
                                   "JOIN MenuCategories AS MC ON MI.CategoryID = MC.CategoryID " +
                                   "WHERE 1=1";

                    if (!includeInactive)
                        query += " AND MI.IsActive = 1";

                    if (!string.IsNullOrEmpty(course))
                        query += " AND LOWER(MI.Course) = LOWER(@Course)";
                    if (!string.IsNullOrEmpty(category))
                        query += " AND LOWER(MC.CategoryName) = LOWER(@Category)";

                    SqlCommand command = new SqlCommand(query, connection);
                    if (!string.IsNullOrEmpty(course))
                        command.Parameters.AddWithValue("@Course", course.Trim());
                    if (!string.IsNullOrEmpty(category))
                        command.Parameters.AddWithValue("@Category", category.Trim());

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
                Console.WriteLine("Error in FilterMenuItems: " + ex.Message);
            }
            return menuItems;
        }

        public List<MenuItem> GetMenuItemsByCategory(MenuCategory category)
        {
            return FilterMenuItems(null, category.CategoryName, true);
        }

        public List<MenuItem> GetMenuItemsByCourse(MenuCourse course)
        {
            return FilterMenuItems(course.ToString(), null, true);
        }

        public List<MenuItem> GetMenuItemsByName(string name)
        {
            return GetAllMenuItems(true).Where(item => item.ItemName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
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
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetMenuCategories: " + ex.Message);
            }
            return menuCategories;
        }
        public void UpdateStock(int menuItemId, int newStock)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE MenuItems SET Stock = @Stock WHERE MenuItemID = @MenuItemID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Stock", newStock);
                    command.Parameters.AddWithValue("@MenuItemID", menuItemId);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateStock: " + ex.Message);
            }
        }

        public void DeductStock(Order order)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var items = order.InterumOrderItems.Count() == 0 ? order.OrderItems : order.InterumOrderItems;

                    foreach (var orderItem in items)
                    {
                        string query = "UPDATE MenuItems SET Stock = Stock - @Quantity WHERE MenuItemID = @MenuItemID";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Quantity", orderItem.Quantity);
                        command.Parameters.AddWithValue("@MenuItemID", orderItem.MenuItem.MenuItemID);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in DeductStock: " + ex.Message);
            }
        }
    }
}
