namespace ChapeauPOS.Models
{
    public class Payment
    {
        public int PaytmentID { get; set; }
        public int BillID { get; set; }
        

        public PaymentMethod PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaidAt { get; set; }
        public decimal TipAmount { get; set; }
        public decimal LowVAT { get; set; }
        public decimal HighVAT { get; set; }    
        public string FeedBack { get; set; }

        public Payment()
        {
            
        }

        public Payment(int paytmentID, int billID, PaymentMethod paymentMethod, decimal totalAmount, DateTime paidAt, decimal tipAmount, decimal lowVAT, decimal highVAT, string feedBack)
        {
            PaytmentID = paytmentID;
            BillID = billID;
            PaymentMethod = paymentMethod;
            TotalAmount = totalAmount;
            PaidAt = paidAt;
            TipAmount = tipAmount;
            LowVAT = lowVAT;
            HighVAT = highVAT;
            FeedBack = feedBack;
        }
    }
    public enum PaymentMethod
    {
        Cash,
        Maestro,
        Voucher
    }
}
