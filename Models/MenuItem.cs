namespace ChapeauPOS.Models
{
    public class MenuItem
    {
        public MenuItem(int menuItemID, string itemName, string itemDescription, decimal itemPrice, bool vat, MenuCategory category, MenuCourse course)
        {
            MenuItemID = menuItemID;
            ItemName = itemName;
            ItemDescription = itemDescription;
            ItemPrice = itemPrice;
            VAT = vat;
            Category = category;
            Course = course;
            IsActive = true;
        }
        public MenuItem()
        {

        }

        public int MenuItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public decimal ItemPrice { get; set; }
        public bool VAT { get; set; }
        public decimal VATPercent { get { return VAT ? 21 : 9; }  }
        public MenuCategory Category { get; set; }
        public MenuCourse Course { get; set; }
        public bool IsActive { get; set; }
    }

}
