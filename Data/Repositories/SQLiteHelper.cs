using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data;

/// <summary>
/// Handles all low-level SQLite database operations
/// including users, expenses, budget cycles,
/// categories, debts, and audit logs.
/// </summary>
public sealed class SQLiteHelper
{
    /// <summary>
    /// Gets the database file path.
    /// </summary>
    public string DatabasePath { get; }

    /// <summary>
    /// SQL schema for Users table.
    /// </summary>
    public const string UsersSchema = DatabaseSchema.Users;

    /// <summary>
    /// SQL schema for BudgetCycles table.
    /// </summary>
    public const string BudgetCycleSchema = DatabaseSchema.BudgetCycles;

    /// <summary>
    /// SQL schema for Expenses table.
    /// </summary>
    public const string ExpenseSchema = DatabaseSchema.Expenses;

    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new SQLite helper instance.
    /// </summary>
    /// <param name="dbPath">Database file path.</param>
    public SQLiteHelper(string dbPath)
    {
        DatabasePath = dbPath;
        _connectionString = $"Data Source={dbPath}";
    }

    /// <summary>
    /// Creates all required database tables
    /// and inserts default categories.
    /// </summary>
    /// <param name="defaultPinHash">Default PIN hash value.</param>
    public void InitializeDatabase(string defaultPinHash)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var tx = connection.BeginTransaction();

        using var command = connection.CreateCommand();
        command.Transaction = tx;

        command.CommandText = string.Join(
            Environment.NewLine,
            UsersSchema,
            BudgetCycleSchema,
            ExpenseSchema,
            DatabaseSchema.Settings,
            DatabaseSchema.Categories,
            DatabaseSchema.AuditLogs,
            DatabaseSchema.Debts);

        command.ExecuteNonQuery();

        using var seed = connection.CreateCommand();
        seed.Transaction = tx;

        seed.CommandText = """
            INSERT OR IGNORE INTO Categories(Name) VALUES ('Food');
            INSERT OR IGNORE INTO Categories(Name) VALUES ('Transport');
            INSERT OR IGNORE INTO Categories(Name) VALUES ('Bills');
            INSERT OR IGNORE INTO Categories(Name) VALUES ('Fun');
            INSERT OR IGNORE INTO Categories(Name) VALUES ('Other');
            """;

        seed.ExecuteNonQuery();

        tx.Commit();
    }

    /// <summary>
    /// Creates a backup copy of the database.
    /// </summary>
    /// <param name="backupPath">Backup file path.</param>
    public void Backup(string backupPath)
    {
        using var source = CreateConnection();
        source.Open();

        using var destination =
            new SqliteConnection($"Data Source={backupPath}");

        source.BackupDatabase(destination);
    }

    /// <summary>
    /// Creates and returns a new SQLite connection.
    /// </summary>
    /// <returns>SQLite database connection.</returns>
    public SqliteConnection CreateConnection() => new(_connectionString);

    /// <summary>
    /// Retrieves the first registered user.
    /// </summary>
    /// <returns>Default user object or null.</returns>
    public User? GetDefaultUser()
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
            "SELECT Id, Name, PinHash, Role FROM Users ORDER BY Id LIMIT 1;";

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new User
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            PinHash = reader.GetString(2),
            Role = reader.GetString(3)
        };
    }

    /// <summary>
    /// Retrieves all registered users.
    /// </summary>
    /// <returns>List of users.</returns>
    public List<User> GetUsers()
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
            "SELECT Id, Name, PinHash, Role FROM Users ORDER BY Name;";

        using var reader = command.ExecuteReader();

        var users = new List<User>();

        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                PinHash = reader.GetString(2),
                Role = reader.GetString(3)
            });
        }

        return users;
    }

    /// <summary>
    /// Gets the total number of users.
    /// </summary>
    /// <returns>User count.</returns>
    public int GetUserCount()
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT COUNT(*) FROM Users;";

        return Convert.ToInt32(command.ExecuteScalar());
    }

    /// <summary>
    /// Finds a user by username.
    /// </summary>
    /// <param name="name">Username.</param>
    /// <returns>User object or null.</returns>
    public User? GetUserByName(string name)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
            "SELECT Id, Name, PinHash, Role FROM Users WHERE Name = $name LIMIT 1;";

        command.Parameters.AddWithValue("$name", name);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return new User
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            PinHash = reader.GetString(2),
            Role = reader.GetString(3)
        };
    }

    /// <summary>
    /// Creates a new application user.
    /// </summary>
    /// <param name="name">Username.</param>
    /// <param name="pinHash">Hashed PIN.</param>
    /// <param name="role">User role.</param>
    public void CreateUser(string name, string pinHash, string role)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        using var command = connection.CreateCommand();

        command.Transaction = tx;

        command.CommandText =
            "INSERT INTO Users(Name, PinHash, Role) VALUES($name, $pinHash, $role);";

        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$pinHash", pinHash);
        command.Parameters.AddWithValue("$role", role);

        command.ExecuteNonQuery();

        InsertAuditLog($"User created: {name} ({role})", tx);

        tx.Commit();
    }

    /// <summary>
    /// Updates the PIN hash of a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="pinHash">New hashed PIN.</param>
    public void UpdateUserPinHash(int userId, string pinHash)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        using var command = connection.CreateCommand();

        command.Transaction = tx;

        command.CommandText =
            "UPDATE Users SET PinHash = $pinHash WHERE Id = $id;";

        command.Parameters.AddWithValue("$pinHash", pinHash);
        command.Parameters.AddWithValue("$id", userId);

        command.ExecuteNonQuery();

        InsertAuditLog($"PIN updated for user #{userId}", tx);

        tx.Commit();
    }

    /// <summary>
    /// Updates admin password hash.
    /// </summary>
    /// <param name="adminPasswordHash">New password hash.</param>
    public void UpdateAdminPassword(string adminPasswordHash)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        using var command = connection.CreateCommand();

        command.Transaction = tx;

        command.CommandText =
            "UPDATE Users SET AdminPasswordHash = $adminPasswordHash WHERE Role = 'Admin';";

        command.Parameters.AddWithValue(
            "$adminPasswordHash",
            adminPasswordHash);

        command.ExecuteNonQuery();

        InsertAuditLog("Admin password updated", tx);

        tx.Commit();
    }
}
