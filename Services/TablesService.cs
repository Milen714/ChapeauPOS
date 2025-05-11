using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;

namespace ChapeauPOS.Services
{
    public class TablesService : ITablesService
    {
        private readonly ITableRepository _tableRepository;
        public TablesService(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }
        public List<Table> GetAllTables()
        {
            return _tableRepository.GetAllTables();
        }
    }
}
