using ChapeauPOS.Models;

namespace ChapeauPOS.Services.Interfaces
{
    public interface IMenuService
    {
        List<MenuItem> GetAllMenuItems();

        MenuItem GetMenuItemById(int id);

        void AddMenuItem(MenuItem menuItem);

        void UpdateMenuItem(MenuItem menuItem);

        //void DeleteMenuItem(int id); // Soft delete (commented by design)

        void ToggleMenuItemStatus(int id, bool isActive); //  Toggle Active/Inactive status

        List<MenuCategory> GetMenuCategories();

        List<MenuItem> GetMenuItemsByCategory(MenuCategory category);

        List<MenuItem> GetMenuItemsByCourse(MenuCourse course);

        List<MenuItem> GetMenuItemsByName(string name);

        List<MenuItem> GetLunch();

        List<MenuItem> GetDinner();

        List<MenuItem> GetDrinks();

        List<MenuItem> FilterMenuItems(string course, string category, bool includeInactive); //  Include inactive toggle

        void DeductStock(Order order); //  Inventory stock management

        void ActivateMenuItem(int id);   //  Shortcut method for activation

        void DeactivateMenuItem(int id); //  Shortcut method for deactivation
        void UpdateStock(int menuItemId, int newStock);

    }
}
