using ChapeauPOS.Models;

namespace ChapeauPOS.Repositories.Interfaces
{
    public interface ITableRepository
    {
        List<Table> GetAllTables();
        Table GetTableByID(int id);
        void UpdateTableStatus(int tableNumber, TableStatus tableStatus);
        List<Table> GetAllUnoccupiedTables();
    }
}
