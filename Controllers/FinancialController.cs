using ChapeauPOS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

public class FinancialController : BaseController
{
    private readonly IFinancialService _financialService;

    public FinancialController(IFinancialService financialService)
    {
        _financialService = financialService;
    }

    public IActionResult Index(DateTime? start, DateTime? end)
    {
        DateTime from = start ?? new DateTime(DateTime.Now.Year, 1, 1);
        DateTime to = (end ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);


        FinancialOverviewViewModel model = _financialService.GetOverview(from, to);
        return View(model);
    }
}