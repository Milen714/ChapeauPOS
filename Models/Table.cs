namespace ChapeauPOS.Models
{
    public class Table
    {
        public Table(int tableId, int tableNumber, int numberOfSeats, TableStatus tableStatus)
        {
            TableId = tableId;
            TableNumber = tableNumber;
            NumberOfSeats = numberOfSeats;
            TableStatus = tableStatus;
        }
        public Table()
        {
            
        }

        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public int NumberOfSeats { get; set; }
        public TableStatus TableStatus { get; set; }
    }
}
