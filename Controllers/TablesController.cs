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
            new Table(1, 1, 6, TableStatus.Free),
            new Table(2, 2, 4, TableStatus.Reserved),
            new Table(3, 3, 4,  TableStatus.Occupied),
            new Table(4, 4, 8, TableStatus.Reserved),
            new Table(5, 5, 4, TableStatus.Free),
            new Table(6, 6, 2, TableStatus.Free),
            new Table(7, 7, 2, TableStatus.Occupied),
            new Table(8, 8, 2, TableStatus.Free),
            new Table(9, 9, 2, TableStatus.Occupied),
            new Table(10, 10, 2, TableStatus.Occupied)
        };
        public IActionResult Index()
        {
            
            return View(Tables);
        }
    }
}
