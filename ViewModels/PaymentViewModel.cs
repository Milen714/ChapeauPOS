using Microsoft.AspNetCore.Mvc;
using ChapeauPOS.Models;

namespace ChapeauPOS.ViewModels
{
    public class PaymentViewModel
    {
        public int TableNumber { get; set; }
        public List<PaymentItemViewModel> Items { get; set; }

        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);

        public decimal LowVAT => Items
            .Where(i => i.MenuItem.VATPercent == 9)
            .Sum(i => i.TotalPrice * 0.09m);

        public decimal HighVAT => Items
            .Where(i => i.MenuItem.VATPercent == 21)
            .Sum(i => i.TotalPrice * 0.21m);
    }

    public class PaymentItemViewModel
    {
        public MenuItem MenuItem { get; set; }
        public int Quantity { get; set; }

        public decimal TotalPrice => MenuItem.ItemPrice * Quantity;
    }
}
