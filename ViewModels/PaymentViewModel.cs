using Microsoft.AspNetCore.Mvc;
using ChapeauPOS.Models;

namespace ChapeauPOS.ViewModels
{
    public class PaymentViewModel
    {
        public Order Order { get; set; }

        public decimal TotalAmount
        {
            get
            {
                { return Order.TotalAmount; }
            }
        }

        public decimal LowVAT => Order.OrderItems.Where(i => i.MenuItem.VATPercent == 9).Sum(i => i.MenuItem.ItemPrice * i.Quantity * 0.09m);

        public decimal HighVAT => Order.OrderItems.Where(i => i.MenuItem.VATPercent == 21).Sum(i => i.MenuItem.ItemPrice * i.Quantity * 0.21m);

        public PaymentViewModel()
        {

        }

    }


}