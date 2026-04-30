using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public interface IDebtRepository
{
    List<DebtRecord> GetDebts(int userId);
    void InsertDebt(DebtRecord debt, SqliteTransaction tx);
    void UpdateDebt(DebtRecord debt, SqliteTransaction tx);
    void DeleteDebt(int debtId, SqliteTransaction tx);
    List<DebtRecord> GetAll(int userId);
    void Add(DebtRecord debt);
    void Update(DebtRecord debt);
    void Delete(int debtId);
}
