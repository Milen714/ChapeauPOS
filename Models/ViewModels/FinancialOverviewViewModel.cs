using System;

namespace ChapeauPOS.Models.ViewModels
{
    public class FinancialOverviewViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalSalesDrinks { get; set; }
        public decimal TotalSalesLunch { get; set; }
        public decimal TotalSalesDinner { get; set; }

        public decimal TotalIncomeDrinks { get; set; }
        public decimal TotalIncomeLunch { get; set; }
        public decimal TotalIncomeDinner { get; set; }

        public decimal TotalTips { get; set; }
    }
}
