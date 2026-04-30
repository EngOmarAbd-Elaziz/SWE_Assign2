using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public interface IBudgetCycleRepository
{
    BudgetCycle? GetActiveCycle(int userId);
    int InsertCycle(BudgetCycle cycle, SqliteTransaction tx);
    void UpdateCycle(BudgetCycle cycle, SqliteTransaction tx);
    void Update(BudgetCycle cycle);
}
