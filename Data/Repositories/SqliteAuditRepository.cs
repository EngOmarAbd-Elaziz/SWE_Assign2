using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IAuditRepository"/>.
/// Responsible for reading and writing audit logs to the database.
/// </summary>
public sealed class SqliteAuditRepository : IAuditRepository
{
    private readonly SQLiteHelper _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteAuditRepository"/> class.
    /// </summary>
    /// <param name="db">Database helper used for SQLite operations.</param>
    public SqliteAuditRepository(SQLiteHelper db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves all audit logs from the database.
    /// </summary>
    public List<AuditLog> GetAuditLogs()
        => _db.GetAuditLogs();

    /// <summary>
    /// Inserts an audit log entry using an existing database transaction.
    /// </summary>
    /// <param name="action">Action description to log.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void InsertAuditLog(string action, SqliteTransaction tx)
        => _db.InsertAuditLog(action, tx);

    /// <summary>
    /// Creates and stores an audit log entry using a new database connection.
    /// </summary>
    /// <param name="userId">ID of the user performing the action.</param>
    /// <param name="action">Description of the action performed.</param>
    public void Log(int userId, string action)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        _db.InsertAuditLog(action, tx);

        tx.Commit();
    }
}
