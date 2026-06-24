using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace NestFinder.Services;

public static class DbHelper
{
    private static readonly string _dbPath;
    private static readonly string _connectionString;

    static DbHelper()
    {
        _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NestFinder.db");
        _connectionString = $"Data Source={_dbPath}";

        InitializeDatabase();
    }

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    private static void InitializeDatabase()
    {
        if (File.Exists(_dbPath))
        {
            return;
        }

        try
        {
            var dir = Path.GetDirectoryName(_dbPath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var conn = GetConnection();
            conn.Open();

            string sqlScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NestFinder.sql");
            if (File.Exists(sqlScriptPath))
            {
                string script = File.ReadAllText(sqlScriptPath);
                using var cmd = new SqliteCommand(script, conn);
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize database: {ex.Message}");
        }
    }
}
