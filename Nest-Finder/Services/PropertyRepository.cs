using Microsoft.Data.Sqlite;
using NestFinder.Models;

namespace NestFinder.Services;

public class PropertyRepository
{
    public List<Property> GetAll(string? search = null, string? typeFilter = null, string? statusFilter = null)
    {
        var list = new List<Property>();
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            var sql = "SELECT Id, Title, City, Type, Price, Status, Bedrooms, Bathrooms, AreaSqFt, Description, UserId, CreatedAt FROM Properties WHERE Status != 'Deleted'";
            var parms = new List<SqliteParameter>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (Title LIKE @s OR City LIKE @s)";
                parms.Add(new SqliteParameter("@s", $"%{search}%"));
            }
            if (!string.IsNullOrWhiteSpace(typeFilter) && typeFilter != "All")
            {
                sql += " AND Type = @t";
                parms.Add(new SqliteParameter("@t", typeFilter));
            }
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                sql += " AND Status = @st";
                parms.Add(new SqliteParameter("@st", statusFilter));
            }
            sql += " ORDER BY CreatedAt DESC";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddRange(parms.ToArray());
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapProperty(r));
        }
        catch { }
        return list;
    }

    public Property? GetById(int id)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("SELECT Id, Title, City, Type, Price, Status, Bedrooms, Bathrooms, AreaSqFt, Description, UserId, CreatedAt FROM Properties WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (r.Read()) return MapProperty(r);
            return null;
        }
        catch { return null; }
    }

    public int Add(Property p)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "INSERT INTO Properties (Title, City, Type, Price, Status, Bedrooms, Bathrooms, AreaSqFt, Description, UserId) " +
                "VALUES (@t, @c, @ty, @p, @s, @bd, @bt, @a, @d, @u); SELECT last_insert_rowid();", conn);
            cmd.Parameters.AddWithValue("@t", p.Title);
            cmd.Parameters.AddWithValue("@c", p.City);
            cmd.Parameters.AddWithValue("@ty", p.Type);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@s", p.Status);
            cmd.Parameters.AddWithValue("@bd", p.Bedrooms);
            cmd.Parameters.AddWithValue("@bt", p.Bathrooms);
            cmd.Parameters.AddWithValue("@a", p.AreaSqFt);
            cmd.Parameters.AddWithValue("@d", (object?)p.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@u", (object?)p.UserId ?? DBNull.Value);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
        catch { return 0; }
    }

    public bool Update(Property p)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "UPDATE Properties SET Title=@t, City=@c, Type=@ty, Price=@p, Status=@s, Bedrooms=@bd, Bathrooms=@bt, AreaSqFt=@a, Description=@d WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@t", p.Title);
            cmd.Parameters.AddWithValue("@c", p.City);
            cmd.Parameters.AddWithValue("@ty", p.Type);
            cmd.Parameters.AddWithValue("@p", p.Price);
            cmd.Parameters.AddWithValue("@s", p.Status);
            cmd.Parameters.AddWithValue("@bd", p.Bedrooms);
            cmd.Parameters.AddWithValue("@bt", p.Bathrooms);
            cmd.Parameters.AddWithValue("@a", p.AreaSqFt);
            cmd.Parameters.AddWithValue("@d", (object?)p.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", p.Id);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    public bool Delete(int id)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("UPDATE Properties SET Status = 'Deleted' WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    private static Property MapProperty(SqliteDataReader r)
    {
        var type = r.GetString(3);
        Property p = type == "Commercial" ? new CommercialProperty() : new ResidentialProperty();
        p.Id = r.GetInt32(0);
        p.Title = r.GetString(1);
        p.City = r.GetString(2);
        p.Type = type;
        p.Price = r.GetDecimal(4);
        p.Status = r.GetString(5);
        p.Bedrooms = r.GetInt32(6);
        p.Bathrooms = r.GetInt32(7);
        p.AreaSqFt = r.GetInt32(8);
        p.Description = r.IsDBNull(9) ? "" : r.GetString(9);
        p.UserId = r.IsDBNull(10) ? null : r.GetInt32(10);
        p.CreatedAt = r.GetDateTime(11);
        return p;
    }
}
