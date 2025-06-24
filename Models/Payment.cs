namespace ChapeauPOS.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public Bill Bill { get; set; }
        
        public PaymentMethod PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime PaidAt { get; set; }

        private decimal? _tipAmount;

        public decimal TipAmount
        {
            get
            {
                return _tipAmount ?? (GrandTotal - TotalAmount);
            }
            set
            {
                _tipAmount = value;
            }
        }

        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }    
        public string FeedBack { get; set; }

        public Payment()
        {
            
        }

        public Payment(int paymentID, PaymentMethod paymentMethod, decimal totalAmount, DateTime paidAt, decimal lowVAT, decimal highVAT, string feedBack)
        {
            PaymentID = paymentID;
            PaymentMethod = paymentMethod;
            TotalAmount = totalAmount;
            PaidAt = paidAt;
            LowVAT = lowVAT;
            HighVAT = highVAT;
            FeedBack = feedBack;
        }
    }
  
}
