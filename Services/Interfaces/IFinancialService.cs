using ChapeauPOS.Models.ViewModels;

public interface IFinancialService
{
    FinancialOverviewViewModel GetOverview(DateTime startDate, DateTime endDate);
}

