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

        //  constructor overloading
        public MenuCategory()
        {
            
        }

        public int CategoryID { get; set; }
        [ValidateNever] 
        public string CategoryName { get; set; }
    }
}
