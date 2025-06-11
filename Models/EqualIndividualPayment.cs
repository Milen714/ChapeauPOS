using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Models
{
    public class   EqualIndividualPayment
    {
        public decimal AmountPaid { get; set; }
        public string? Feedback { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}
