
using ChapeauPOS.Models;

namespace ChapeauPOS.ViewModels
{
    public class PartialPaymentViewModel
    {
        public int TableId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<EqualIndividualPayment> Payments { get; set; }= new List<EqualIndividualPayment>();
        public PartialPaymentViewModel()
        {
            
        }
    }

}
