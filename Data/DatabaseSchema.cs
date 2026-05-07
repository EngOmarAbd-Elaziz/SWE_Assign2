namespace Masroofy.App.Data;

/// <summary>
/// Contains all SQL schema definitions used to initialize the SQLite database.
/// Each constant represents a table creation script used during database setup.
/// </summary>
public static class DatabaseSchema
{
    /// <summary>
    /// SQL schema for the Users table.
    /// Stores user authentication and role information.
    /// </summary>
    public const string Users = """
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL UNIQUE,
            PinHash TEXT NOT NULL,
            Role TEXT NOT NULL,
            AdminPasswordHash TEXT
        );
        """;

    /// <summary>
    /// SQL schema for BudgetCycles table.
    /// Stores budget cycle information for each user.
    /// </summary>
    public const string BudgetCycles = """
        CREATE TABLE IF NOT EXISTS BudgetCycles (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserId INTEGER NOT NULL,
            TotalAllowance REAL NOT NULL,
            StartDate TEXT NOT NULL,
            EndDate TEXT NOT NULL,
            RemainingBalance REAL NOT NULL,
            LastRolloverDate TEXT NOT NULL,
            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
        );
        """;

    /// <summary>
    /// SQL schema for Expenses table.
    /// Stores all expense transactions linked to budget cycles.
    /// </summary>
    public const string Expenses = """
        CREATE TABLE IF NOT EXISTS Expenses (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserId INTEGER NOT NULL,
            CycleId INTEGER NOT NULL,
            Amount REAL NOT NULL,
            Category TEXT NOT NULL,
            Date TEXT NOT NULL,
            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
            FOREIGN KEY (CycleId) REFERENCES BudgetCycles(Id) ON DELETE CASCADE
        );
        """;

    /// <summary>
    /// SQL schema for Debts table.
    /// Stores user debt records such as borrowing or repayments.
    /// </summary>
    public const string Debts = """
        CREATE TABLE IF NOT EXISTS Debts (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserId INTEGER NOT NULL,
            Amount REAL NOT NULL,
            Type TEXT NOT NULL,
            Note TEXT NOT NULL,
            Date TEXT NOT NULL,
            FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
        );
        """;

    /// <summary>
    /// SQL schema for Categories table.
    /// Stores predefined and user-defined expense categories.
    /// </summary>
    public const string Categories = """
        CREATE TABLE IF NOT EXISTS Categories (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT UNIQUE NOT NULL
        );
        """;

    /// <summary>
    /// SQL schema for Settings table.
    /// Stores key-value configuration settings for the application.
    /// </summary>
    public const string Settings = """
        CREATE TABLE IF NOT EXISTS Settings (
            Key TEXT PRIMARY KEY,
            Value TEXT NOT NULL
        );
        """;

    /// <summary>
    /// SQL schema for AuditLogs table.
    /// Stores system activity logs for tracking user actions.
    /// </summary>
    public const string AuditLogs = """
        CREATE TABLE IF NOT EXISTS AuditLogs (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Action TEXT NOT NULL,
            Date TEXT NOT NULL
        );
        """;
}
