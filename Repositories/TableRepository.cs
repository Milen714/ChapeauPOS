using ChapeauPOS.Models;
using ChapeauPOS.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ChapeauPOS.Repositories
{
    public class TableRepository : ITableRepository
    {
        private readonly string? _connectionString;
        public TableRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDB");
        }
        private Table ReadTable(SqlDataReader reader)
        {
            return new Table
            {
                TableID = reader.GetInt32(0),
                TableNumber = reader.GetInt32(1),
                NumberOfSeats = reader.GetInt32(2),
                TableStatus = (TableStatus)Enum.Parse(typeof(TableStatus), reader.GetString(3))
            };
        }
        Table ITableRepository.GetTableByID(int id)
        {
            Table table = new Table();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT TableID, TableNumber, NumberOfSeats, TableStatus " +
                                   " FROM [Tables] " +
                                   " WHERE TableID = @TableID; ";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TableID", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            table = ReadTable(reader);
                        }
                    }
                }
            }
            catch(SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving table from database", ex);
            }
            return table;
        }
        List<Table> ITableRepository.GetAllTables()
        {
            List<Table> tables = new List<Table>();
            try {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT TableID, TableNumber, NumberOfSeats, TableStatus " +
                                   " FROM [Tables]; ";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Table table = ReadTable(reader);
                            tables.Add(table);
                        }
                    }

                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error connecting to database", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving tables from database", ex);
            }
            return tables;
        }
    }
}
