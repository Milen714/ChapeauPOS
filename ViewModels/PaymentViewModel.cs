using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.ViewModels
{
    public class PaymentViewModel
    {
        public int TableNumber { get; set; }
        public List<PaymentItemViewModel> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
    }

    public class PaymentItemViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public decimal VATRate { get; set; }
    }
}
