using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using ChapeauPOS.Services.Interfaces;

namespace ChapeauPOS.Services
{
    public class TablesService : ITablesService
    {
        private readonly ITableRepository _tableRepository;
        private readonly IOrdersRepository _ordersRepository;
        public TablesService(ITableRepository tableRepository, IOrdersRepository ordersRepository)
        {
            _tableRepository = tableRepository;
            _ordersRepository = ordersRepository;
        }
        public List<Table> GetAllTables()
        {
            return _tableRepository.GetAllTables();
        }

        public List<Table> GetAllUnoccupiedTables()
        {
            return _tableRepository.GetAllUnoccupiedTables();
        }

        public Table GetTableByID(int id)
        {
            return _tableRepository.GetTableByID(id);
        }

        public void SynchronizeTableStatuses(List<Table> tables)
        {
            foreach (var table in tables)
            {
                var orders = _ordersRepository.GetOrdersByTableId(table.TableNumber);

                if (orders.Count == 0)
                {
                    // No order history – assume free THIS IS BECAUSE GetOrdersByTableId RETURNS ONLY ORDERS THAT ARE
                    // 'Ordered', 'Served', 'Ready', 'Preparing' SO NO "HISTORIC DATA" WITHIN THE LOGIC OF THIS DB
                    if (table.TableStatus != TableStatus.Free)
                    {
                        table.TableStatus = TableStatus.Free;
                        _tableRepository.UpdateTableStatus(table.TableNumber, TableStatus.Free);
                    }
                }
                else
                {
                    // Check if all orders are finalized
                    bool allFinalized = orders.All(o => o.OrderStatus == OrderStatus.Finalized);
                    if (allFinalized && table.TableStatus != TableStatus.Free)
                    {
                        table.TableStatus = TableStatus.Free;
                        _tableRepository.UpdateTableStatus(table.TableNumber, TableStatus.Free);
                    }
                }
            }
        }

        public void UpdateTableStatus(int tableNumber, TableStatus tableStatus)
        {
            _tableRepository.UpdateTableStatus(tableNumber, tableStatus);
        }
    }
}
