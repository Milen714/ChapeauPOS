using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;

namespace ChapeauPOS.Repositories
{
    public class TableRepository : ITableRepository
    {
        List<Table> ITableRepository.GetAllTables()
        {
            throw new NotImplementedException();
        }
    }
}
