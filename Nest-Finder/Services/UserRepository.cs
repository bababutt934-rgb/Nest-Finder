using Microsoft.Data.Sqlite;
using NestFinder.Models;

namespace NestFinder.Services;

public class UserRepository
{
    public User? Authenticate(string username, string passwordHash)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "SELECT Id, FullName, Email, Phone, Username, PasswordHash, Role, IsActive, CreatedAt " +
                "FROM Users WHERE Username = @u AND PasswordHash = @p AND IsActive = 1", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", passwordHash);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return MapUser(reader);
            return null;
        }
        catch { return null; }
    }

    public bool Register(User user)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "INSERT INTO Users (FullName, Email, Phone, Username, PasswordHash, Role) " +
                "VALUES (@fn, @em, @ph, @un, @pw, @ro)", conn);
            cmd.Parameters.AddWithValue("@fn", user.FullName);
            cmd.Parameters.AddWithValue("@em", user.Email);
            cmd.Parameters.AddWithValue("@ph", user.Phone);
            cmd.Parameters.AddWithValue("@un", user.Username);
            cmd.Parameters.AddWithValue("@pw", user.PasswordHash);
            cmd.Parameters.AddWithValue("@ro", user.Role);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    public List<User> GetAll()
    {
        var list = new List<User>();
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("SELECT Id, FullName, Email, Phone, Username, PasswordHash, Role, IsActive, CreatedAt FROM Users", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(MapUser(reader));
        }
        catch { }
        return list;
    }

    public User? GetById(int id)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand(
                "SELECT Id, FullName, Email, Phone, Username, PasswordHash, Role, IsActive, CreatedAt FROM Users WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return MapUser(reader);
            return null;
        }
        catch { return null; }
    }

    public bool ToggleActive(int id)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("UPDATE Users SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    public bool UpdateProfile(User user)
    {
        try
        {
            using var conn = DbHelper.GetConnection();
            conn.Open();
            string sql = "UPDATE Users SET FullName = @fn, Phone = @ph";
            if (!string.IsNullOrEmpty(user.PasswordHash))
                sql += ", PasswordHash = @pw";
            sql += " WHERE Id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@fn", user.FullName);
            cmd.Parameters.AddWithValue("@ph", user.Phone);
            cmd.Parameters.AddWithValue("@id", user.Id);
            if (!string.IsNullOrEmpty(user.PasswordHash))
                cmd.Parameters.AddWithValue("@pw", user.PasswordHash);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch { return false; }
    }

    private static User MapUser(SqliteDataReader r)
    {
        var role = r.GetString(6);
        User user = role == "Admin" ? new Admin() : new Customer();
        user.Id = r.GetInt32(0);
        user.FullName = r.GetString(1);
        user.Email = r.GetString(2);
        user.Phone = r.GetString(3);
        user.Username = r.GetString(4);
        user.PasswordHash = r.GetString(5);
        user.Role = role;
        user.IsActive = r.GetBoolean(7);
        user.CreatedAt = r.GetDateTime(8);
        return user;
    }
}
