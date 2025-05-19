using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
        [ValidateNever] // ✅ Add this line
        public string CategoryName { get; set; }
    }
}
