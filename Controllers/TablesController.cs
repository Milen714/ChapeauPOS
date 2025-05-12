using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class TablesController : BaseController
    {
        private readonly ITablesService _tablesService;
        public TablesController(ITablesService tablesService)
        {
            _tablesService = tablesService;
        }
      
        public IActionResult Index()
        {
            var tables = _tablesService.GetAllTables();
            return View(tables);
        }
    }
}
