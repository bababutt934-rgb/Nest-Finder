using Microsoft.Data.Sqlite;
using NestFinder.Models;
using System.Text;
using System.IO;

namespace NestFinder.Services;

public class TransactionRepository
{
    public List<Transaction> GetAll(string? typeFilter = null, string? statusFilter = null, DateTime? from = null, DateTime? to = null)
    {
        var list = new List<Transaction>();
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            var sql = "SELECT t.Id, t.PropertyId, t.CustomerId, t.Type, t.Amount, t.PaymentMethod, t.Notes, t.Date, t.Status, " +
                      "p.Title AS PropertyTitle, u.FullName AS CustomerName, t.PaymentStatus " +
                      "FROM Transactions t " +
                      "JOIN Properties p ON t.PropertyId = p.Id " +
                      "JOIN Users u ON t.CustomerId = u.Id WHERE 1=1";
            var parms = new List<SqliteParameter>();
            if (!string.IsNullOrWhiteSpace(typeFilter) && typeFilter != "All")
            {
                sql += " AND t.Type = @ty";
                parms.Add(new SqliteParameter("@ty", typeFilter));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                sql += " AND t.Status = @st";
                parms.Add(new SqliteParameter("@st", statusFilter));
            }
            if (from.HasValue)
            {
                sql += " AND t.Date >= @from";
                parms.Add(new SqliteParameter("@from", from.Value));
            }
            if (to.HasValue)
            {
                sql += " AND t.Date <= @to";
                parms.Add(new SqliteParameter("@to", to.Value));
            }
            sql += " ORDER BY t.Date DESC";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddRange(parms.ToArray());
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Transaction
                {
                    Id = r.GetInt32(0),
                    PropertyId = r.GetInt32(1),
                    CustomerId = r.GetInt32(2),
                    TransactionType = r.GetString(3),
                    Amount = r.GetDecimal(4),
                    PaymentMethod = r.IsDBNull(5) ? "" : r.GetString(5),
                    Notes = r.IsDBNull(6) ? "" : r.GetString(6),
                    Date = r.GetDateTime(7),
                    Status = r.GetString(8),
                    PropertyTitle = r.GetString(9),
                    CustomerName = r.GetString(10),
                    PaymentStatus = r.IsDBNull(11) ? "Unpaid" : r.GetString(11)
                });
            }
        }
        catch { }
        return list;
    }

    public bool Add(Transaction t)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "INSERT INTO Transactions (PropertyId, CustomerId, Type, Amount, PaymentMethod, Notes, Status, PaymentStatus) " +
                "VALUES (@pid, @cid, @ty, @amt, @pm, @n, @st, @pstatus)", conn);
            cmd.Parameters.AddWithValue("@pid", t.PropertyId);
            cmd.Parameters.AddWithValue("@cid", t.CustomerId);
            cmd.Parameters.AddWithValue("@ty", t.TransactionType);
            cmd.Parameters.AddWithValue("@amt", t.Amount);
            cmd.Parameters.AddWithValue("@pm", t.PaymentMethod);
            cmd.Parameters.AddWithValue("@n", (object?)t.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@st", t.Status);
            cmd.Parameters.AddWithValue("@pstatus", t.PaymentStatus);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    public List<Transaction> GetByCustomerId(int customerId)
    {
        var list = new List<Transaction>();
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "SELECT t.Id, t.PropertyId, t.CustomerId, t.Type, t.Amount, t.PaymentMethod, t.Notes, t.Date, t.Status, " +
                "p.Title, u.FullName, t.PaymentStatus FROM Transactions t JOIN Properties p ON t.PropertyId = p.Id JOIN Users u ON t.CustomerId = u.Id " +
                "WHERE t.CustomerId = @cid ORDER BY t.Date DESC", conn);
            cmd.Parameters.AddWithValue("@cid", customerId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Transaction
                {
                    Id = r.GetInt32(0), PropertyId = r.GetInt32(1), CustomerId = r.GetInt32(2),
                    TransactionType = r.GetString(3), Amount = r.GetDecimal(4),
                    PaymentMethod = r.IsDBNull(5) ? "" : r.GetString(5),
                    Notes = r.IsDBNull(6) ? "" : r.GetString(6),
                    Date = r.GetDateTime(7), Status = r.GetString(8),
                    PropertyTitle = r.GetString(9), CustomerName = r.GetString(10),
                    PaymentStatus = r.IsDBNull(11) ? "Unpaid" : r.GetString(11)
                });
            }
        }
        catch { }
        return list;
    }

    public bool CustomerHasTransaction(int customerId, int propertyId)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("SELECT COUNT(*) FROM Transactions WHERE CustomerId = @cid AND PropertyId = @pid AND Status = 'Completed'", conn);
            cmd.Parameters.AddWithValue("@cid", customerId);
            cmd.Parameters.AddWithValue("@pid", propertyId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
        catch { return false; }
    }

    public void ExportToCsv(List<Transaction> transactions, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,Property,Customer,Type,Amount,Payment Method,Date,Status");
        foreach (var t in transactions)
        {
            sb.AppendLine($"{t.Id},\"{t.PropertyTitle}\",\"{t.CustomerName}\",{t.TransactionType},{t.Amount},{t.PaymentMethod},{t.Date:yyyy-MM-dd},{t.Status}");
        }
        File.WriteAllText(filePath, sb.ToString());
    }

    public bool MarkAsPaid(int transactionId, string paymentMethod)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "UPDATE Transactions SET PaymentStatus = 'Paid', PaymentMethod = @Method WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Method", paymentMethod);
            cmd.Parameters.AddWithValue("@Id", transactionId);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch (System.Exception ex)
        {
            throw new System.Exception("Error in MarkAsPaid repository call: " + ex.Message, ex);
        }
    }
}
