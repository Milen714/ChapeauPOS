using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class TablesController : BaseController
    {
        private readonly ITablesService _tablesService;
        private readonly IOrdersService _ordersService;
        public TablesController(ITablesService tablesService, IOrdersService ordersService)
        {
            _tablesService = tablesService;
            _ordersService = ordersService;
        }
      
        public IActionResult Index()
        {
            var tables = _tablesService.GetAllTables();
            _tablesService.SynchronizeTableStatuses(tables);
            return View(tables);
        }

        
    }
}
