namespace ChapeauPOS.Models
{
    public class Bill
    {
        public Order Order { get; set; }
        public int BillID { get; set; }
        public decimal Subtotal { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public Employee? FinalizedBy { get; set; }
        public Bill()
        {
            
        }
    }
}
