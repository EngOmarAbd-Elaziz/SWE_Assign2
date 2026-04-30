using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public sealed class SqliteBudgetCycleRepository : IBudgetCycleRepository
{
    private readonly SQLiteHelper _db;

    public SqliteBudgetCycleRepository(SQLiteHelper db)
    {
        _db = db;
    }

    public BudgetCycle? GetActiveCycle(int userId) => _db.GetActiveCycle(userId);

    public int InsertCycle(BudgetCycle cycle, SqliteTransaction tx) => _db.InsertCycle(cycle, tx);

    public void UpdateCycle(BudgetCycle cycle, SqliteTransaction tx) => _db.UpdateCycle(cycle, tx);

    public void Update(BudgetCycle cycle)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        UpdateCycle(cycle, tx);
        tx.Commit();
    }
}
