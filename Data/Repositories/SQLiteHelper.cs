using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data;

public sealed class SQLiteHelper
{
    public string DatabasePath { get; }
    public const string UsersSchema = DatabaseSchema.Users;
    public const string BudgetCycleSchema = DatabaseSchema.BudgetCycles;
    public const string ExpenseSchema = DatabaseSchema.Expenses;

    private readonly string _connectionString;

    public SQLiteHelper(string dbPath)
    {
        DatabasePath = dbPath;
        _connectionString = $"Data Source={dbPath}";
    }

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

    public void Backup(string backupPath)
    {
        using var source = CreateConnection();
        source.Open();
        using var destination = new SqliteConnection($"Data Source={backupPath}");
        source.BackupDatabase(destination);
    }

    public SqliteConnection CreateConnection() => new(_connectionString);

    public User? GetDefaultUser()
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, PinHash, Role FROM Users ORDER BY Id LIMIT 1;";
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new User
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            PinHash = reader.GetString(2),
            Role = reader.GetString(3)
        };
    }

    public List<User> GetUsers()
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, PinHash, Role FROM Users ORDER BY Name;";
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

    public int GetUserCount()
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public User? GetUserByName(string name)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, PinHash, Role FROM Users WHERE Name = $name LIMIT 1;";
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

    public void CreateUser(string name, string pinHash, string role)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "INSERT INTO Users(Name, PinHash, Role) VALUES($name, $pinHash, $role);";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$pinHash", pinHash);
        command.Parameters.AddWithValue("$role", role);
        command.ExecuteNonQuery();
        InsertAuditLog($"User created: {name} ({role})", tx);
        tx.Commit();
    }

    public void UpdateUserPinHash(int userId, string pinHash)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "UPDATE Users SET PinHash = $pinHash WHERE Id = $id;";
        command.Parameters.AddWithValue("$pinHash", pinHash);
        command.Parameters.AddWithValue("$id", userId);
        command.ExecuteNonQuery();
        InsertAuditLog($"PIN updated for user #{userId}", tx);
        tx.Commit();
    }

    public void UpdateAdminPassword(string adminPasswordHash)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "UPDATE Users SET AdminPasswordHash = $adminPasswordHash WHERE Role = 'Admin';";
        command.Parameters.AddWithValue("$adminPasswordHash", adminPasswordHash);
        command.ExecuteNonQuery();
        InsertAuditLog("Admin password updated", tx);
        tx.Commit();
    }

    public BudgetCycle? GetActiveCycle(int userId)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, UserId, TotalAllowance, StartDate, EndDate, RemainingBalance, LastRolloverDate
            FROM BudgetCycles
            WHERE UserId = $userId
            ORDER BY Id DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$userId", userId);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new BudgetCycle
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            TotalAllowance = reader.GetDecimal(2),
            StartDate = DateTime.Parse(reader.GetString(3)),
            EndDate = DateTime.Parse(reader.GetString(4)),
            RemainingBalance = reader.GetDecimal(5),
            LastRolloverDate = DateTime.Parse(reader.GetString(6))
        };
    }

    public int InsertCycle(BudgetCycle cycle, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = """
            INSERT INTO BudgetCycles (UserId, TotalAllowance, StartDate, EndDate, RemainingBalance, LastRolloverDate)
            VALUES ($userId, $total, $start, $end, $remaining, $rollover);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$userId", cycle.UserId);
        command.Parameters.AddWithValue("$total", cycle.TotalAllowance);
        command.Parameters.AddWithValue("$start", cycle.StartDate.ToString("O"));
        command.Parameters.AddWithValue("$end", cycle.EndDate.ToString("O"));
        command.Parameters.AddWithValue("$remaining", cycle.RemainingBalance);
        command.Parameters.AddWithValue("$rollover", cycle.LastRolloverDate.ToString("O"));
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateCycle(BudgetCycle cycle, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = """
            UPDATE BudgetCycles
            SET RemainingBalance = $remaining,
                LastRolloverDate = $rollover
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$remaining", cycle.RemainingBalance);
        command.Parameters.AddWithValue("$rollover", cycle.LastRolloverDate.ToString("O"));
        command.Parameters.AddWithValue("$id", cycle.Id);
        command.ExecuteNonQuery();
    }

    public List<Expense> GetExpenses(int cycleId)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, UserId, CycleId, Amount, Category, Date
            FROM Expenses
            WHERE CycleId = $cycleId
            ORDER BY Date DESC;
            """;
        command.Parameters.AddWithValue("$cycleId", cycleId);
        using var reader = command.ExecuteReader();
        var result = new List<Expense>();
        while (reader.Read())
        {
            result.Add(new Expense
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                CycleId = reader.GetInt32(2),
                Amount = reader.GetDecimal(3),
                Category = reader.GetString(4),
                Date = DateTime.Parse(reader.GetString(5))
            });
        }
        return result;
    }

    public List<string> GetCategories()
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name FROM Categories ORDER BY Name;";
        using var reader = command.ExecuteReader();
        var result = new List<string>();
        while (reader.Read()) result.Add(reader.GetString(0));
        return result;
    }

    public void AddCategory(string category, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "INSERT OR IGNORE INTO Categories(Name) VALUES ($name);";
        command.Parameters.AddWithValue("$name", category);
        command.ExecuteNonQuery();
    }

    public void DeleteCategory(string categoryName, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "DELETE FROM Categories WHERE Name = $name;";
        command.Parameters.AddWithValue("$name", categoryName);
        command.ExecuteNonQuery();
    }

    public List<AuditLog> GetAuditLogs()
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Action, Date FROM AuditLogs ORDER BY Id DESC LIMIT 100;";
        using var reader = command.ExecuteReader();
        var result = new List<AuditLog>();
        while (reader.Read())
        {
            result.Add(new AuditLog
            {
                Id = reader.GetInt32(0),
                Action = reader.GetString(1),
                Timestamp = DateTime.Parse(reader.GetString(2))
            });
        }
        return result;
    }

    public void InsertAuditLog(string action, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "INSERT INTO AuditLogs(Action, Date) VALUES($action, $date);";
        command.Parameters.AddWithValue("$action", action);
        command.Parameters.AddWithValue("$date", DateTime.Now.ToString("O"));
        command.ExecuteNonQuery();
    }

    public void InsertExpense(Expense expense, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = """
            INSERT INTO Expenses (UserId, CycleId, Amount, Category, Date)
            VALUES ($userId, $cycleId, $amount, $category, $date);
            """;
        command.Parameters.AddWithValue("$userId", expense.UserId);
        command.Parameters.AddWithValue("$cycleId", expense.CycleId);
        command.Parameters.AddWithValue("$amount", expense.Amount);
        command.Parameters.AddWithValue("$category", expense.Category);
        command.Parameters.AddWithValue("$date", expense.Date.ToString("O"));
        command.ExecuteNonQuery();
    }

    public void UpdateExpense(Expense expense, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = """
            UPDATE Expenses
            SET Amount = $amount, Category = $category, Date = $date
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", expense.Id);
        command.Parameters.AddWithValue("$amount", expense.Amount);
        command.Parameters.AddWithValue("$category", expense.Category);
        command.Parameters.AddWithValue("$date", expense.Date.ToString("O"));
        command.ExecuteNonQuery();
    }

    public void DeleteExpense(int expenseId, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "DELETE FROM Expenses WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", expenseId);
        command.ExecuteNonQuery();
    }

    public void DeleteAllExpensesForCycle(int cycleId, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "DELETE FROM Expenses WHERE CycleId = $cycleId;";
        command.Parameters.AddWithValue("$cycleId", cycleId);
        command.ExecuteNonQuery();
    }

    public List<DebtRecord> GetDebts(int userId)
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, UserId, Amount, Type, Note, Date
            FROM Debts
            WHERE UserId = $userId
            ORDER BY Date DESC;
            """;
        command.Parameters.AddWithValue("$userId", userId);
        using var reader = command.ExecuteReader();
        var result = new List<DebtRecord>();
        while (reader.Read())
        {
            result.Add(new DebtRecord
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Amount = reader.GetDecimal(2),
                Type = reader.GetString(3),
                Note = reader.GetString(4),
                Date = DateTime.Parse(reader.GetString(5))
            });
        }
        return result;
    }

    public void InsertDebt(DebtRecord debt, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = """
            INSERT INTO Debts(UserId, Amount, Type, Note, Date)
            VALUES ($userId, $amount, $type, $note, $date);
            """;
        command.Parameters.AddWithValue("$userId", debt.UserId);
        command.Parameters.AddWithValue("$amount", debt.Amount);
        command.Parameters.AddWithValue("$type", debt.Type);
        command.Parameters.AddWithValue("$note", debt.Note);
        command.Parameters.AddWithValue("$date", debt.Date.ToString("O"));
        command.ExecuteNonQuery();
    }

    public void UpdateDebt(DebtRecord debt, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = """
            UPDATE Debts
            SET Amount = $amount, Type = $type, Note = $note, Date = $date
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", debt.Id);
        command.Parameters.AddWithValue("$amount", debt.Amount);
        command.Parameters.AddWithValue("$type", debt.Type);
        command.Parameters.AddWithValue("$note", debt.Note);
        command.Parameters.AddWithValue("$date", debt.Date.ToString("O"));
        command.ExecuteNonQuery();
    }

    public void DeleteDebt(int debtId, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "DELETE FROM Debts WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", debtId);
        command.ExecuteNonQuery();
    }
}
