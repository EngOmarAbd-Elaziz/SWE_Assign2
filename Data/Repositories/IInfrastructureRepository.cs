using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// Provides low-level infrastructure operations for database management,
/// including initialization, connection handling, and backups.
/// </summary>
public interface IInfrastructureRepository
{
    /// <summary>
    /// Gets the current database file path.
    /// </summary>
    string DatabasePath { get; }

    /// <summary>
    /// Initializes the database schema and seeds default data if needed.
    /// </summary>
    /// <param name="defaultPinHash">Default hashed PIN used for initial setup.</param>
    void InitializeDatabase(string defaultPinHash);

    /// <summary>
    /// Creates and returns a new SQLite database connection.
    /// </summary>
    /// <returns>A SQLite connection instance.</returns>
    SqliteConnection CreateConnection();

    /// <summary>
    /// Creates a backup copy of the database at the specified path.
    /// </summary>
    /// <param name="backupPath">Destination path for the backup file.</param>
    void Backup(string backupPath);
}
