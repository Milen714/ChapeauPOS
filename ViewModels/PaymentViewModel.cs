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
        
        public decimal LowVAT => Order.OrderItems
            .Max(i => i.MenuItem.VATPercent) == 9 ? Order.TotalAmount * 0.09m : 0m;


        public decimal HighVAT => Order.OrderItems
            .Max(i => i.MenuItem.VATPercent) == 21 ? Order.TotalAmount * 0.21m : 0m;

        public PaymentViewModel()
        {

        }

    }


}