using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public interface IExpenseRepository
{
    List<Expense> GetExpenses(int cycleId);
    void InsertExpense(Expense expense, SqliteTransaction tx);
    void UpdateExpense(Expense expense, SqliteTransaction tx);
    void DeleteExpense(int expenseId, SqliteTransaction tx);
    void Update(Expense expense);
    void Delete(int expenseId);
    void DeleteAllForCycle(int cycleId);
}
