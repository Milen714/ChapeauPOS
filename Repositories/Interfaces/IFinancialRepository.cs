using ChapeauPOS.Models.ViewModels;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface IFinancialRepository
    {
        FinancialOverviewViewModel GetFinancialOverview(DateTime startDate, DateTime endDate);
    }
}
