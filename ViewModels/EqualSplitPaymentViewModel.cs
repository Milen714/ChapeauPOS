
using ChapeauPOS.Models;

namespace ChapeauPOS.ViewModels
{
    public class EqualSplitPaymentViewModel
    {
        public int TableId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }
        public int NumberOfPeople { get; set; }
        public List<IndividualPayment> Payments { get; set; }
        public EqualSplitPaymentViewModel()
        {
            
        }
    }
    

    
}
