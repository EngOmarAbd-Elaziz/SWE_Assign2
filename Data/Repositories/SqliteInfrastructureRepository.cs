using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public sealed class SqliteInfrastructureRepository : IInfrastructureRepository
{
    private readonly SQLiteHelper _db;

    public SqliteInfrastructureRepository(SQLiteHelper db)
    {
        _db = db;
    }

    public string DatabasePath => _db.DatabasePath;

    public void InitializeDatabase(string defaultPinHash) => _db.InitializeDatabase(defaultPinHash);

    public SqliteConnection CreateConnection() => _db.CreateConnection();

    public void Backup(string backupPath) => _db.Backup(backupPath);
}
