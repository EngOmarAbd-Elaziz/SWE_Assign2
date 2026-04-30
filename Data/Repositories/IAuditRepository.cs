using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public interface IAuditRepository
{
    List<AuditLog> GetAuditLogs();
    void InsertAuditLog(string action, SqliteTransaction tx);
    void Log(int userId, string action);
}
