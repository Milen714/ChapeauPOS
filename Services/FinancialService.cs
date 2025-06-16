using ChapeauPOS.Models.ViewModels;
using ChapeauPOS.Repositories.Interfaces;

namespace ChapeauPOS.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly IFinancialRepository _financialRepository;

        public FinancialService(IFinancialRepository financialRepository)
        {
            _financialRepository = financialRepository;
        }

        public FinancialOverviewViewModel GetOverview(DateTime startDate, DateTime endDate)
        {
            return _financialRepository.GetFinancialOverview(startDate, endDate);
        }
    }
}
