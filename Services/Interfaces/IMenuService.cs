using ChapeauPOS.Models;

namespace ChapeauPOS.Services.Interfaces
{
    public interface IMenuService
    {
        List<MenuItem> GetAllMenuItems();
        MenuItem GetMenuItemById(int id);
        void AddMenuItem(MenuItem menuItem);
        void UpdateMenuItem(MenuItem menuItem);

        List<MenuCategory> GetMenuCategories();
        List<MenuItem> GetMenuItemsByCategory(MenuCategory category);
        List<MenuItem> GetMenuItemsByCourse(MenuCourse course);
        List<MenuItem> GetMenuItemsByName(string name);
        List<MenuItem> GetLunch();
        List<MenuItem> GetDinner();
        List<MenuItem> GetDrinks();
        List<MenuItem> FilterMenuItems(string course, string category);
        void DeductStock(Order order);
    }
}
