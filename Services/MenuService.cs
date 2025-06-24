using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;

namespace ChapeauPOS.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _menuRepository;
        private List<MenuCategory> _menuCategories;

        public MenuService(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
            _menuCategories = _menuRepository.GetMenuCategories();
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            _menuRepository.AddMenuItem(menuItem);
        }

        public void DeleteMenuItem(int id)
        {
            _menuRepository.DeleteMenuItem(id); // Optional: consider soft delete
        }

        public List<MenuItem> GetAllMenuItems()
        {
            return _menuRepository.GetAllMenuItems();
        }

        public List<MenuItem> GetDinner()
        {
            return _menuRepository.GetMenuItemsByCategory(new MenuCategory { CategoryName = "Dinner" });
        }

        public List<MenuItem> GetDrinks()
        {
            return _menuRepository.GetMenuItemsByCategory(new MenuCategory { CategoryName = "Drinks" });
        }

        public List<MenuItem> GetLunch()
        {
            return _menuRepository.GetMenuItemsByCategory(new MenuCategory { CategoryName = "Lunch" });
        }

        public List<MenuCategory> GetMenuCategories()
        {
            return _menuRepository.GetMenuCategories();
        }

        public MenuItem GetMenuItemById(int id)
        {
            return _menuRepository.GetMenuItemById(id);
        }

        public List<MenuItem> GetMenuItemsByCategory(MenuCategory category)
        {
            return _menuRepository.GetMenuItemsByCategory(category);
        }

        public List<MenuItem> GetMenuItemsByCourse(MenuCourse course)
        {
            return _menuRepository.GetMenuItemsByCourse(course);
        }

        public List<MenuItem> GetMenuItemsByName(string name)
        {
            return _menuRepository.GetMenuItemsByName(name);
        }

        public void ToggleMenuItemStatus(int id, bool isActive)
        {
            _menuRepository.ToggleMenuItemStatus(id, isActive); //  Toggle between Active/Inactive
        }

        public void ActivateMenuItem(int id)
        {
            _menuRepository.ActivateMenuItem(id); //  Activate directly
        }

        //public void GetWholeMenu()
        //{
        //    List<MenuCategory> menuCategories = _menuRepository.GetMenuCategories();
        //    foreach (MenuCategory menuCategory in menuCategories)
        //    {
        //        if (menuCategory.CategoryName == "Lunch")
        //        {
        //            List<MenuItem> lunchItems = _menuRepository.GetMenuItemsByCategory(menuCategory);
        //            foreach (MenuItem menuItem in lunchItems)
        //            {
        //                Console.WriteLine($"Lunch Menu Item: {menuItem.ItemName}");
        //            }
        //        }
        //        else if (menuCategory.CategoryName == "Dinner")
        //        {
        //            List<MenuItem> dinnerItems = _menuRepository.GetMenuItemsByCategory(menuCategory);
        //            foreach (MenuItem menuItem in dinnerItems)
        //            {
        //                Console.WriteLine($"Dinner Menu Item: {menuItem.ItemName}");
        //            }
        //        }
        //        else if (menuCategory.CategoryName == "Drinks")
        //        {
        //            List<MenuItem> drinkItems = _menuRepository.GetMenuItemsByCategory(menuCategory);
        //            foreach (MenuItem menuItem in drinkItems)
        //            {
        //                Console.WriteLine($"Drink Menu Item: {menuItem.ItemName}");
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Unknown category: {menuCategory.CategoryName}");
        //        }
        //    }
        //}


        public void DeactivateMenuItem(int id)
        {
            _menuRepository.DeactivateMenuItem(id); //  Deactivate directly
        }

        public List<MenuItem> FilterMenuItems(string course, string category, bool includeInactive)
        {
            return _menuRepository.FilterMenuItems(course, category, includeInactive); //  Proper use of parameter
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            _menuRepository.UpdateMenuItem(menuItem);
        }

        public void DeductStock(Order order)
        {

            foreach(OrderItem orderItem in order.OrderItems)
            {
                if(orderItem.MenuItem.Stock < orderItem.Quantity)
                {
                    throw new Exception("Not enough stock to fullfil your order");
                }
            }
            if (order.InterumOrderItems != null)
            {
                foreach (OrderItem orderItem in order.InterumOrderItems) 
                {
                    if (orderItem.MenuItem.Stock < orderItem.Quantity)
                    {
                        throw new Exception("Not enough stock to fullfil your order");
                    }
                }
            }
            _menuRepository.DeductStock(order);

            _menuRepository.DeductStock(order); // Used in order processing
        }

        public void UpdateStock(int menuItemId, int newStock)
        {
            _menuRepository.UpdateStock(menuItemId, newStock);
        }



    }
}
