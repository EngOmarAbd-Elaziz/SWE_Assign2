using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public sealed class SqliteAuditRepository : IAuditRepository
{
    private readonly SQLiteHelper _db;

    public SqliteAuditRepository(SQLiteHelper db)
    {
        _db = db;
    }

    public List<AuditLog> GetAuditLogs() => _db.GetAuditLogs();

    public void InsertAuditLog(string action, SqliteTransaction tx) => _db.InsertAuditLog(action, tx);

    public void Log(int userId, string action)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _db.InsertAuditLog(action, tx);
        tx.Commit();
    }
}
