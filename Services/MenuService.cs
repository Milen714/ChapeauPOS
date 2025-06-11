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
            _menuRepository.DeleteMenuItem(id);
        }

        public List<MenuItem> GetAllMenuItems()
        {
            return _menuRepository.GetAllMenuItems();
        }

        public List<MenuItem> GetDinner()
        {
            List<MenuItem> dinnerItems = new List<MenuItem>();
            foreach (MenuCategory menuCategory in _menuCategories)
            {
                if (menuCategory.CategoryName == "Dinner")
                {
                    dinnerItems = _menuRepository.GetMenuItemsByCategory(menuCategory);
                }
            }

            return dinnerItems;
        }

        public List<MenuItem> GetDrinks()
        {
            List<MenuItem> drinkItems = new List<MenuItem>();
            foreach (MenuCategory menuCategory in _menuCategories)
            {
                if (menuCategory.CategoryName == "Drinks")
                {
                    drinkItems = _menuRepository.GetMenuItemsByCategory(menuCategory);
                }
            }

            return drinkItems;
        }

        public List<MenuItem> GetLunch()
        {
            List<MenuItem> lunchItems = new List<MenuItem>();
            foreach (MenuCategory menuCategory in _menuCategories)
            {
                if (menuCategory.CategoryName == "Lunch")
                {
                    lunchItems = _menuRepository.GetMenuItemsByCategory(menuCategory);
                }
            }

            return lunchItems;
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
            _menuRepository.ToggleMenuItemStatus(id, isActive);
        }
        public List<MenuItem> FilterMenuItems(string course, string category)
        {
            var allItems = _menuRepository.GetAllMenuItems();

            var filtered = allItems.Where(i =>
                (string.IsNullOrEmpty(course) ||
                 i.Course.ToString().Equals(course, StringComparison.OrdinalIgnoreCase)) &&

                (string.IsNullOrEmpty(category) ||
                 (i.Category != null &&
                  !string.IsNullOrEmpty(i.Category.CategoryName) &&
                  i.Category.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase)))
            ).ToList();

       

            return filtered;
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            _menuRepository.UpdateMenuItem(menuItem);
        }

        public void DeductStock(Order order)
        {
            _menuRepository.DeductStock(order);
        }
    }
}