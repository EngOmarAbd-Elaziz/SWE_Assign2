using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public sealed class SqliteExpenseRepository : IExpenseRepository
{
    private readonly SQLiteHelper _db;

    public SqliteExpenseRepository(SQLiteHelper db)
    {
        _db = db;
    }

    public List<Expense> GetExpenses(int cycleId) => _db.GetExpenses(cycleId);

    public void InsertExpense(Expense expense, SqliteTransaction tx) => _db.InsertExpense(expense, tx);

    public void UpdateExpense(Expense expense, SqliteTransaction tx) => _db.UpdateExpense(expense, tx);

    public void DeleteExpense(int expenseId, SqliteTransaction tx) => _db.DeleteExpense(expenseId, tx);

    public void Update(Expense expense)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        UpdateExpense(expense, tx);
        tx.Commit();
    }

    public void Delete(int expenseId)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        DeleteExpense(expenseId, tx);
        tx.Commit();
    }

    public void DeleteAllForCycle(int cycleId)
    {
        using var connection = _db.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _db.DeleteAllExpensesForCycle(cycleId, tx);
        tx.Commit();
    }
}
