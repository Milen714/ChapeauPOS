using System;
using Microsoft.Data.SqlClient;
using ChapeauPOS.Models.ViewModels;
using ChapeauPOS.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ChapeauPOS.Repositories
{
    public class FinancialRepository : IFinancialRepository
    {
        private readonly string _connectionString;

        public FinancialRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChapeauDB");
        }

        public FinancialOverviewViewModel GetFinancialOverview(DateTime startDate, DateTime endDate)
        {
            FinancialOverviewViewModel overview = new FinancialOverviewViewModel();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        mi.Course,
                        SUM(oi.Quantity * mi.ItemPrice) AS TotalSales
                    FROM OrderItems oi
                    INNER JOIN MenuItems mi ON oi.MenuItemID = mi.MenuItemID
                    INNER JOIN Orders o ON oi.OrderID = o.OrderID
                    WHERE o.ClosedAt BETWEEN @start AND @end
                    GROUP BY mi.Course;

                    SELECT 
                        SUM(TipAmount) AS TotalTips
                    FROM Payments   
                    WHERE PaidAt BETWEEN @start AND @end;
                ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@start", startDate);
                    cmd.Parameters.AddWithValue("@end", endDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string course = reader["Course"]?.ToString()?.Trim().ToLower() ?? "";
                            decimal totalSales = reader["TotalSales"] != DBNull.Value ? (decimal)reader["TotalSales"] : 0;

                            // Map real DB course names to existing view model
                            switch (course)
                            {
                                case "drink":
                                    overview.TotalSalesDrinks = totalSales;
                                    overview.TotalIncomeDrinks = totalSales;
                                    break;
                                case "starter":
                                    overview.TotalSalesLunch = totalSales;  // mapping Starter to Lunch
                                    overview.TotalIncomeLunch = totalSales;
                                    break;
                                case "main":
                                    overview.TotalSalesDinner = totalSales;  // mapping Main to Dinner
                                    overview.TotalIncomeDinner = totalSales;
                                    break;
                                // Optional: add Dessert if needed
                                default:
                                    break;
                            }
                        }

                        if (reader.NextResult() && reader.Read())
                        {
                            overview.TotalTips = reader["TotalTips"] != DBNull.Value ? (decimal)reader["TotalTips"] : 0;
                        }
                    }
                }
            }

            overview.StartDate = startDate;
            overview.EndDate = endDate;

            return overview;
        }
    }
}
