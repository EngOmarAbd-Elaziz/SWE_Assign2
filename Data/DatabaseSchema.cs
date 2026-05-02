namespace Masroofy.App.Data;

public static class DatabaseSchema
{
    public const string Users = """
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL UNIQUE,
            PinHash TEXT NOT NULL,
            Role TEXT NOT NULL,
            AdminPasswordHash TEXT
        );
        """;

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

    public const string Categories = """
        CREATE TABLE IF NOT EXISTS Categories (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT UNIQUE NOT NULL
        );
        """;

    public const string Settings = """
        CREATE TABLE IF NOT EXISTS Settings (
            Key TEXT PRIMARY KEY,
            Value TEXT NOT NULL
        );
        """;

    public const string AuditLogs = """
        CREATE TABLE IF NOT EXISTS AuditLogs (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Action TEXT NOT NULL,
            Date TEXT NOT NULL
        );
        """;
}
