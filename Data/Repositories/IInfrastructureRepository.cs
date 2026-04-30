using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public interface IInfrastructureRepository
{
    string DatabasePath { get; }
    void InitializeDatabase(string defaultPinHash);
    SqliteConnection CreateConnection();
    void Backup(string backupPath);
}
