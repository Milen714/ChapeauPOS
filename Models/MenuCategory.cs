namespace ChapeauPOS.Models
{
    public class MenuCategory
    {
        public MenuCategory(int categoryID, string categoryName)
        {
            CategoryID = categoryID;
            CategoryName = categoryName;
        }
        public MenuCategory()
        {
            
        }

        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }
}
