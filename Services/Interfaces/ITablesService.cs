using ChapeauPOS.Models;

namespace ChapeauPOS.Services.Interfaces
{
    public interface ITablesService
    {
        List<Table> GetAllTables();
        Table GetTableByID(int id);
        void UpdateTableStatus(int tableNumber, TableStatus tableStatus);
        void SynchronizeTableStatuses(List<Table> tables);
    }
}
