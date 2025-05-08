namespace ChapeauPOS.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemDescription { get; set; }
        public string? ItemPrice { get; set; }
        public bool VAT {  get; set; }
        public string? Course { get; set; }
        public MenuCategories? Category { get; set; }
    }
}
