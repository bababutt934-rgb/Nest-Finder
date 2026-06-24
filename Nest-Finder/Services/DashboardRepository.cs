using Microsoft.Data.Sqlite;
using NestFinder.Models;

namespace NestFinder.Services;

public class DashboardRepository
{
    public DashboardData GetStats()
    {
        var data = new DashboardData();
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            
            string sql = @"
                SELECT 
                    (SELECT COUNT(*) FROM Properties WHERE Status != 'Deleted') AS TotalProperties,
                    (SELECT COUNT(*) FROM Properties WHERE Status = 'Available') AS Available,
                    (SELECT COUNT(*) FROM Properties WHERE Status IN ('Sold', 'Rented')) AS SoldRented;

                SELECT 
                    t.Id, p.Title AS PropertyTitle, t.Amount, u.FullName AS CustomerName, t.Date, t.Status
                FROM Transactions t
                JOIN Properties p ON t.PropertyId = p.Id
                JOIN Users u ON t.CustomerId = u.Id
                ORDER BY t.Date DESC
                LIMIT 5;

                SELECT 
                    v.Id, p.Title AS PropertyTitle, u.FullName AS CustomerName, v.ViewingTime, v.Status
                FROM Viewings v
                JOIN Properties p ON v.PropertyId = p.Id
                JOIN Users u ON v.CustomerId = u.Id
                WHERE v.ViewingTime >= datetime('now', 'localtime') AND v.Status = 'Scheduled'
                ORDER BY v.ViewingTime ASC
                LIMIT 3;
            ";

            using var cmd = new SqliteCommand(sql, conn);
            using var r = cmd.ExecuteReader();

            // Result 1: Counts
            if (r.Read())
            {
                data.TotalProperties = r.GetInt32(0);
                data.Available = r.GetInt32(1);
                data.SoldRented = r.GetInt32(2);
            }

            // Result 2: Recent Transactions
            if (r.NextResult())
            {
                while (r.Read())
                {
                    data.RecentTransactions.Add(new Transaction
                    {
                        Id = r.GetInt32(0),
                        PropertyTitle = r.GetString(1),
                        Amount = r.GetDecimal(2),
                        CustomerName = r.GetString(3),
                        Date = r.GetDateTime(4),
                        Status = r.GetString(5)
                    });
                }
            }

            // Result 3: Upcoming Viewings
            if (r.NextResult())
            {
                while (r.Read())
                {
                    data.UpcomingViewings.Add(new Viewing
                    {
                        Id = r.GetInt32(0),
                        PropertyTitle = r.GetString(1),
                        CustomerName = r.GetString(2),
                        ViewingTime = r.GetDateTime(3),
                        Status = r.GetString(4)
                    });
                }
            }
        }
        catch { }
        return data;
    }
}
