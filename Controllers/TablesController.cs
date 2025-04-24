using ChapeauPOS.Models;
using ChapeauPOS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class TablesController : BaseController
    {
        private readonly ITableRepository _tableRepository;
        public TablesController(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }
        //  testing data
        private static List<Table> Tables = new List<Table>
        {
            new Table(1, 4, 6, TableStatus.Free),
            new Table(2, 2, 4, TableStatus.Free),
            new Table(3, 6, 4,  TableStatus.Free),
            new Table(4, 8, 8, TableStatus.Free),
            new Table(5, 10, 2, TableStatus.Free)
        };
        public IActionResult Index()
        {
            
            return View(Tables);
        }
    }
}
