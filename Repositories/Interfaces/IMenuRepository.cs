using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface IMenuRepository
    {
        List<MenuItem> GetAllMenuItems(bool includeInactive = false);

        MenuItem GetMenuItemById(int id);

        void AddMenuItem(MenuItem menuItem);

        void UpdateMenuItem(MenuItem menuItem);

        void DeleteMenuItem(int id); // Soft delete or legacy removal if implemented

        void ToggleMenuItemStatus(int id, bool isActive); // Used to switch active/inactive

        void ActivateMenuItem(int id);  // Shortcut method to activate

        void DeactivateMenuItem(int id); //  Shortcut method to deactivate

        List<MenuItem> FilterMenuItems(string course, string category, bool includeInactive = false); // Filtering with optional inactive items

        List<MenuCategory> GetMenuCategories();

        List<MenuItem> GetMenuItemsByCategory(MenuCategory category);

        List<MenuItem> GetMenuItemsByCourse(MenuCourse course);

        List<MenuItem> GetMenuItemsByName(string name);

        void DeductStock(Order order); //  Stock update during order processing
        void UpdateStock(int menuItemId, int newStock);
    }
}
