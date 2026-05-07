using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data;

/// <summary>
/// Provides a data access layer for the Masroofy application, encapsulating all SQLite
/// database operations including user management, budget cycles, expenses, debts,
/// categories, and audit logging.
/// </summary>
public sealed class SQLiteHelper
{
    /// <summary>
    /// Gets the absolute file-system path of the SQLite database file.
    /// </summary>
    public string DatabasePath { get; }

    /// <summary>
    /// The DDL statement used to create the Users table.
    /// </summary>
    public const string UsersSchema = DatabaseSchema.Users;

    /// <summary>
    /// The DDL statement used to create the BudgetCycles table.
    /// </summary>
    public const string BudgetCycleSchema = DatabaseSchema.BudgetCycles;

    /// <summary>
    /// The DDL statement used to create the Expenses table.
    /// </summary>
    public const string ExpenseSchema = DatabaseSchema.Expenses;

    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of <see cref="SQLiteHelper"/> targeting the specified database file.
    /// The file does not need to exist yet; it will be created on first connection.
    /// </summary>
    /// <param name="dbPath">The absolute or relative path to the SQLite .db file.</param>
    public SQLiteHelper(string dbPath)
    {
        DatabasePath = dbPath;
        _connectionString = $"Data Source={dbPath}";
    }

    /// <summary>
    /// Creates all application tables if they do not already exist and seeds the default
    /// expense categories, all within a single atomic transaction.
    /// Safe to call on an already-initialized database as all statements use
    /// CREATE TABLE IF NOT EXISTS and INSERT OR IGNORE.
    /// </summary>
    /// <param name="defaultPinHash">The hashed PIN used when creating the initial admin user record.</param>
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
    /// Creates a full online backup of the current database to the specified path using
    /// the SQLite Online Backup API, which is safe to call while the database is in use.
    /// </summary>
    /// <param name="backupPath">The destination file path for the backup. The file will be created or overwritten.</param>
    public void Backup(string backupPath)
    {
        using var source = CreateConnection();
        source.Open();
        using var destination = new SqliteConnection($"Data Source={backupPath}");
        source.BackupDatabase(destination);
    }

    /// <summary>
    /// Creates and returns a new, unopened <see cref="SqliteConnection"/> configured for this database.
    /// The caller is responsible for opening and disposing it.
    /// </summary>
    /// <returns>A new <see cref="SqliteConnection"/> instance.</returns>
    public SqliteConnection CreateConnection() => new(_connectionString);

    /// <summary>
    /// Retrieves the first user record ordered by ascending ID, typically the initial admin account.
    /// </summary>
    /// <returns>A <see cref="User"/> populated with Id, Name, PinHash, and Role; or null if the Users table is empty.</returns>
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

    /// <summary>
    /// Retrieves all users from the database, ordered alphabetically by name.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="User"/> objects, or an empty list if no users exist.</returns>
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

    /// <summary>
    /// Returns the total number of user records in the database.
    /// </summary>
    /// <returns>An <see cref="int"/> representing the current user count.</returns>
    public int GetUserCount()
    {
        using var connection = CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    /// <summary>
    /// Looks up a single user by their exact display name.
    /// </summary>
    /// <param name="name">The username to search for.</param>
    /// <returns>A <see cref="User"/> if found; otherwise null.</returns>
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

    /// <summary>
    /// Inserts a new user record and writes a corresponding audit log entry,
    /// both within a single atomic transaction.
    /// </summary>
    /// <param name="name">The display name for the new user.</param>
    /// <param name="pinHash">The hashed PIN credential for the new user.</param>
    /// <param name="role">The role assigned to the user, e.g. Admin or User.</param>
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

    /// <summary>
    /// Updates the stored PIN hash for a specific user and records the change
    /// in the audit log, both within a single atomic transaction.
    /// </summary>
    /// <param name="userId">The primary key of the user whose PIN is being changed.</param>
    /// <param name="pinHash">The new hashed PIN value to store.</param>
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

    /// <summary>
    /// Updates the admin password hash for all users with the Admin role
    /// and records the change in the audit log, both within a single atomic transaction.
    /// </summary>
    /// <param name="adminPasswordHash">The new hashed admin password to store.</param>
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

    /// <summary>
    /// Retrieves the most recently created budget cycle for a given user,
    /// which is treated as the currently active cycle.
    /// </summary>
    /// <param name="userId">The primary key of the user whose cycle is requested.</param>
    /// <returns>A <see cref="BudgetCycle"/> populated with all fields, or null if no cycle exists for the user.</returns>
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

    /// <summary>
    /// Inserts a new <see cref="BudgetCycle"/> row within an existing transaction
    /// and returns the auto-generated row ID.
    /// </summary>
    /// <param name="cycle">The budget cycle data to persist.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the insert in. The caller is responsible for committing or rolling back.</param>
    /// <returns>The ROWID of the newly inserted cycle record.</returns>
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

    /// <summary>
    /// Updates the RemainingBalance and LastRolloverDate fields of an existing
    /// budget cycle within an active transaction.
    /// </summary>
    /// <param name="cycle">The cycle to update. Only Id, RemainingBalance, and LastRolloverDate are used.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the update in.</param>
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

    /// <summary>
    /// Retrieves all expenses belonging to a specific budget cycle,
    /// ordered by date descending with the most recent first.
    /// </summary>
    /// <param name="cycleId">The primary key of the budget cycle to query.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="Expense"/> objects, or an empty list if no expenses exist for the cycle.</returns>
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

    /// <summary>
    /// Retrieves all expense category names ordered alphabetically.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of category name strings, or an empty list if no categories exist.</returns>
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

    /// <summary>
    /// Inserts a new category name within an existing transaction,
    /// silently ignoring duplicates via INSERT OR IGNORE.
    /// </summary>
    /// <param name="category">The category name to add.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the insert in.</param>
    public void AddCategory(string category, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "INSERT OR IGNORE INTO Categories(Name) VALUES ($name);";
        command.Parameters.AddWithValue("$name", category);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Deletes a category by name within an existing transaction.
    /// Note: this does not cascade to existing expenses referencing the deleted category name.
    /// </summary>
    /// <param name="categoryName">The exact name of the category to delete.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the delete in.</param>
    public void DeleteCategory(string categoryName, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "DELETE FROM Categories WHERE Name = $name;";
        command.Parameters.AddWithValue("$name", categoryName);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Retrieves the most recent audit log entries ordered by descending ID,
    /// capped at 100 records.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="AuditLog"/> objects, or an empty list if no log entries exist.</returns>
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

    /// <summary>
    /// Inserts a timestamped audit log entry within an existing transaction,
    /// ensuring the entry is committed atomically with the operation it describes.
    /// The timestamp is recorded in ISO 8601 round-trip format using local time.
    /// </summary>
    /// <param name="action">A human-readable description of the audited action.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the insert in.</param>
    public void InsertAuditLog(string action, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "INSERT INTO AuditLogs(Action, Date) VALUES($action, $date);";
        command.Parameters.AddWithValue("$action", action);
        command.Parameters.AddWithValue("$date", DateTime.Now.ToString("O"));
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Inserts a new expense record within an existing transaction.
    /// </summary>
    /// <param name="expense">The expense data to persist.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the insert in.</param>
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

    /// <summary>
    /// Updates the amount, category, and date of an existing expense record within an active transaction.
    /// </summary>
    /// <param name="expense">The expense to update. The Id field identifies the row; all other mutable fields are overwritten.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the update in.</param>
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

    /// <summary>
    /// Deletes a single expense record by its primary key within an active transaction.
    /// </summary>
    /// <param name="expenseId">The primary key of the expense to delete.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the delete in.</param>
    public void DeleteExpense(int expenseId, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "DELETE FROM Expenses WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", expenseId);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Deletes all expense records associated with a specific budget cycle within an active transaction.
    /// Typically used when resetting or archiving a cycle.
    /// </summary>
    /// <param name="cycleId">The primary key of the budget cycle whose expenses should be removed.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the delete in.</param>
    public void DeleteAllExpensesForCycle(int cycleId, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "DELETE FROM Expenses WHERE CycleId = $cycleId;";
        command.Parameters.AddWithValue("$cycleId", cycleId);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Retrieves all debt records for a specific user, ordered by date descending with the most recent first.
    /// </summary>
    /// <param name="userId">The primary key of the user whose debts are requested.</param>
    /// <returns>A <see cref="List{T}"/> of <see cref="DebtRecord"/> objects, or an empty list if no debts exist for the user.</returns>
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

    /// <summary>
    /// Inserts a new debt record within an existing transaction.
    /// </summary>
    /// <param name="debt">The debt data to persist.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the insert in.</param>
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

    /// <summary>
    /// Updates the amount, type, note, and date of an existing debt record within an active transaction.
    /// </summary>
    /// <param name="debt">The debt to update. The Id field identifies the row; all other mutable fields are overwritten.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the update in.</param>
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

    /// <summary>
    /// Deletes a single debt record by its primary key within an active transaction.
    /// </summary>
    /// <param name="debtId">The primary key of the debt record to delete.</param>
    /// <param name="tx">An active <see cref="SqliteTransaction"/> to enlist the delete in.</param>
    public void DeleteDebt(int debtId, SqliteTransaction tx)
    {
        using var command = tx.Connection!.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "DELETE FROM Debts WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", debtId);
        command.ExecuteNonQuery();
    }
}
