using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface IMenuRepository
    {
        List<MenuItem> GetAllMenuItems(bool includeInactive = false);
        MenuItem GetMenuItemById(int id);
        void AddMenuItem(MenuItem menuItem);
        void UpdateMenuItem(MenuItem menuItem);
        void DeleteMenuItem(int id);
        List<MenuCategory> GetMenuCategories();
        List<MenuItem> GetMenuItemsByCategory(MenuCategory category);
        List<MenuItem> GetMenuItemsByCourse(MenuCourse course);
        List<MenuItem> GetMenuItemsByName(string name);


    }
}
