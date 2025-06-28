using ChapeauPOS.Commons;
using ChapeauPOS.Models;
using ChapeauPOS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

public class FinancialController : BaseController
{
    private readonly IFinancialService _financialService;

    public FinancialController(IFinancialService financialService)
    {
        _financialService = financialService;
    }
    [HttpGet]
    [SessionAuthorize(Roles.Manager)]
    public IActionResult Index(DateTime? start, DateTime? end)
    {
        DateTime from = start ?? new DateTime(DateTime.Now.Year, 1, 1);
        DateTime to = (end ?? DateTime.Now).Date.AddDays(1).AddTicks(-1);

        FinancialOverviewViewModel model = _financialService.GetFinancialOverview(from, to);
        return View(model);
    }

}