using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// Defines operations for managing audit logs in the system.
/// </summary>
public interface IAuditRepository
{
    /// <summary>
    /// Retrieves all audit log records.
    /// </summary>
    /// <returns>A list of audit logs.</returns>
    List<AuditLog> GetAuditLogs();

    /// <summary>
    /// Inserts an audit log entry within a database transaction.
    /// </summary>
    /// <param name="action">Description of the performed action.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void InsertAuditLog(string action, SqliteTransaction tx);

    /// <summary>
    /// Creates and stores a new audit log entry.
    /// </summary>
    /// <param name="userId">ID of the user who performed the action.</param>
    /// <param name="action">Description of the action.</param>
    void Log(int userId, string action);
}
