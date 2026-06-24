using Microsoft.Data.Sqlite;
using NestFinder.Models;

namespace NestFinder.Services;

public class FavoriteRepository
{
    public bool Add(int userId, int propertyId)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("INSERT INTO Favorites (UserId, PropertyId) VALUES (@u, @p)", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@p", propertyId);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    public bool Remove(int userId, int propertyId)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM Favorites WHERE UserId = @u AND PropertyId = @p", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@p", propertyId);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    public List<Favorite> GetByUser(int userId)
    {
        var list = new List<Favorite>();
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "SELECT f.Id, f.UserId, f.PropertyId, f.CreatedAt, p.Title, p.City, p.Price, p.Status " +
                "FROM Favorites f JOIN Properties p ON f.PropertyId = p.Id WHERE f.UserId = @u ORDER BY f.CreatedAt DESC", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Favorite
                {
                    Id = r.GetInt32(0), UserId = r.GetInt32(1), PropertyId = r.GetInt32(2),
                    CreatedAt = r.GetDateTime(3), PropertyTitle = r.GetString(4),
                    PropertyCity = r.GetString(5), PropertyPrice = r.GetDecimal(6),
                    PropertyStatus = r.GetString(7)
                });
            }
        }
        catch { }
        return list;
    }

    public bool IsFavorite(int userId, int propertyId)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("SELECT COUNT(*) FROM Favorites WHERE UserId = @u AND PropertyId = @p", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@p", propertyId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
        catch { return false; }
    }
}

public class ReviewRepository
{
    public bool Add(Review review)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "INSERT INTO Reviews (PropertyId, UserId, Rating, Comment) VALUES (@pid, @uid, @r, @c)", conn);
            cmd.Parameters.AddWithValue("@pid", review.PropertyId);
            cmd.Parameters.AddWithValue("@uid", review.UserId);
            cmd.Parameters.AddWithValue("@r", review.Rating);
            cmd.Parameters.AddWithValue("@c", (object?)review.Comment ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    public List<Review> GetByProperty(int propertyId)
    {
        var list = new List<Review>();
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "SELECT r.Id, r.PropertyId, r.UserId, r.Rating, r.Comment, r.CreatedAt, u.FullName " +
                "FROM Reviews r JOIN Users u ON r.UserId = u.Id WHERE r.PropertyId = @pid ORDER BY r.CreatedAt DESC", conn);
            cmd.Parameters.AddWithValue("@pid", propertyId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Review
                {
                    Id = r.GetInt32(0), PropertyId = r.GetInt32(1), UserId = r.GetInt32(2),
                    Rating = r.GetInt32(3), Comment = r.IsDBNull(4) ? "" : r.GetString(4),
                    CreatedAt = r.GetDateTime(5), ReviewerName = r.GetString(6)
                });
            }
        }
        catch { }
        return list;
    }
}

public class ImageRepository
{
    public bool Add(int propertyId, string imagePath)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("INSERT INTO PropertyImages (PropertyId, ImagePath) VALUES (@pid, @ip)", conn);
            cmd.Parameters.AddWithValue("@pid", propertyId);
            cmd.Parameters.AddWithValue("@ip", imagePath);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    public List<PropertyImage> GetByProperty(int propertyId)
    {
        var list = new List<PropertyImage>();
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("SELECT Id, PropertyId, ImagePath FROM PropertyImages WHERE PropertyId = @pid", conn);
            cmd.Parameters.AddWithValue("@pid", propertyId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new PropertyImage { Id = r.GetInt32(0), PropertyId = r.GetInt32(1), ImagePath = r.GetString(2) });
        }
        catch { }
        return list;
    }

    public bool Delete(int id)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("DELETE FROM PropertyImages WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }
}
