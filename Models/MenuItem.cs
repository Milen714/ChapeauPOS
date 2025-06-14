using Microsoft.IdentityModel.Logging;

namespace ChapeauPOS.Models
{
    public class MenuItem
    {
        //  Full constructor (including IsActive status)
        public MenuItem(int menuItemID, string itemName, string itemDescription, decimal itemPrice, bool vat, MenuCategory category, MenuCourse course, bool isActive)
        {
            MenuItemID = menuItemID;
            ItemName = itemName;
            ItemDescription = itemDescription;
            ItemPrice = itemPrice;
            VAT = vat;
            Category = category;
            Course = course;
            IsActive = isActive; //  Fix: assign incoming parameter
        }

        //  Constructor overloading for creating a new MenuItem
        public MenuItem()
        {
        }

        //  Overloaded constructor without IsActive (defaults to true in DB)
        public MenuItem(int menuItemID, string itemName, string itemDescription, decimal itemPrice, bool vAT, MenuCategory category, MenuCourse course)
        {
            MenuItemID = menuItemID;
            ItemName = itemName;
            ItemDescription = itemDescription;
            ItemPrice = itemPrice;
            VAT = vAT;
            Category = category;
            Course = course;
        }

        public int MenuItemID { get; set; }

        public string ItemName { get; set; }

        public string ItemDescription { get; set; }

        public decimal ItemPrice { get; set; }

        public bool VAT { get; set; }

        public decimal VATPercent { get { return VAT ? 21 : 9; } }

        public MenuCategory Category { get; set; }

        public MenuCourse Course { get; set; }

        public bool IsActive { get; set; } //  Indicates if the item is active or inactive

        public int Stock { get; set; } //  Used for deducting stock in orders
    }
}
