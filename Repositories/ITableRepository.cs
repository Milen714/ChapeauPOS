using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories
{
    public interface ITableRepository
    {
        List<Table> GetAllTables();
    }
}
