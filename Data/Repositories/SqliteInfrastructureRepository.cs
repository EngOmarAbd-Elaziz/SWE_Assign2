using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IInfrastructureRepository"/>.
/// Acts as a bridge between the application and the low-level SQLiteHelper,
/// providing database initialization, connection creation, and backup operations.
/// </summary>
public sealed class SqliteInfrastructureRepository : IInfrastructureRepository
{
    private readonly SQLiteHelper _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteInfrastructureRepository"/> class.
    /// </summary>
    /// <param name="db">Database helper used for SQLite operations.</param>
    public SqliteInfrastructureRepository(SQLiteHelper db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets the file path of the SQLite database.
    /// </summary>
    public string DatabasePath => _db.DatabasePath;

    /// <summary>
    /// Initializes the database by creating required tables and seed data.
    /// </summary>
    /// <param name="defaultPinHash">Default PIN hash used during initialization.</param>
    public void InitializeDatabase(string defaultPinHash)
        => _db.InitializeDatabase(defaultPinHash);

    /// <summary>
    /// Creates a new SQLite database connection.
    /// </summary>
    /// <returns>A SQLite connection instance.</returns>
    public SqliteConnection CreateConnection()
        => _db.CreateConnection();

    /// <summary>
    /// Creates a backup of the current database at the specified path.
    /// </summary>
    /// <param name="backupPath">Destination path for the backup file.</param>
    public void Backup(string backupPath)
        => _db.Backup(backupPath);
}
