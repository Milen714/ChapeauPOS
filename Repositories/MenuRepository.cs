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
            bool VAT = reader.GetBoolean(reader.GetOrdinal("VAT")); // still works fine
            int CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID"));
            string CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"));
            MenuCourse Course = Enum.Parse<MenuCourse>(reader.GetString(reader.GetOrdinal("Course")));
            MenuCategory category = new MenuCategory(CategoryID, CategoryName);
            return new MenuItem(MenuItemID, ItemName, ItemDescription, ItemPrice, VAT, category, Course);
        }
        public List<MenuItem> GetAllMenuItems()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT MI.MenuItemID, MI.ItemName, MI.ItemDescription, MI.ItemPrice, MI.VAT, MC.CategoryID, MC.CategoryName, MI.Course " +
                                   "FROM MenuItems AS MI " +
                                   "JOIN MenuCategories AS MC ON MI.CategoryID = MC.CategoryID ";

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
            throw new NotImplementedException();
        }
        public void AddMenuItem(MenuItem menuItem)
        {
            throw new NotImplementedException();
        }
        public void UpdateMenuItem(MenuItem menuItem)
        {
            throw new NotImplementedException();
        }
        public void DeleteMenuItem(int id)
        {
            throw new NotImplementedException();
        }
        public List<MenuItem> GetMenuItemsByCategory(MenuCategory category)
        { 
            List<MenuItem> menuItems = new List<MenuItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT MI.MenuItemID, MI.ItemName, MI.ItemDescription, MI.ItemPrice, MI.VAT, MC.CategoryID, MC.CategoryName, MI.Course " +
                                   "FROM MenuItems AS MI " +
                                   "JOIN MenuCategories AS MC ON MI.CategoryID = MC.CategoryID " +
                                   "WHERE MC.CategoryName = @CategoryName ";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
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
        public List<MenuItem> GetMenuItemsByCourse(MenuCourse course)
        {
            throw new NotImplementedException();
        }
        public List<MenuItem> GetMenuItemsByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
    
    
    

