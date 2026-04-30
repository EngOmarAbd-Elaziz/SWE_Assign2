using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public sealed class SqliteDebtRepository : IDebtRepository
{
    private readonly SQLiteHelper _db;

    public SqliteDebtRepository(SQLiteHelper db)
    {
        _db = db;
    }

    public List<DebtRecord> GetDebts(int userId) => _db.GetDebts(userId);

    public void InsertDebt(DebtRecord debt, SqliteTransaction tx) => _db.InsertDebt(debt, tx);

    public void UpdateDebt(DebtRecord debt, SqliteTransaction tx) => _db.UpdateDebt(debt, tx);

    public void DeleteDebt(int debtId, SqliteTransaction tx) => _db.DeleteDebt(debtId, tx);

    public List<DebtRecord> GetAll(int userId) => GetDebts(userId);

    public void Add(DebtRecord debt)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        InsertDebt(debt, tx);
        tx.Commit();
    }

    public void Update(DebtRecord debt)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        UpdateDebt(debt, tx);
        tx.Commit();
    }

    public void Delete(int debtId)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        DeleteDebt(debtId, tx);
        tx.Commit();
    }
}
