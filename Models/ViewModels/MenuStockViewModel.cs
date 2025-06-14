namespace ChapeauPOS.Models.ViewModels
{
    public class MenuStockViewModel
    {
        public int MenuItemID { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public string Course { get; set; }
        public int Stock { get; set; }

        public string StockStatus
        {
            get
            {
                if (Stock == 0)
                {
                    return "Out of Stock";
                }
                else if (Stock <= 10)
                {
                    return "Almost Out of Stock";
                }
                else
                {
                    return "In Stock";
                }
            }
        }
    }
}
