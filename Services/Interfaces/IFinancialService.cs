using ChapeauPOS.Models.ViewModels;

public interface IFinancialService
{
    FinancialOverviewViewModel GetFinancialOverview(DateTime startDate, DateTime endDate);
}

