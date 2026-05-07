using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IExpenseRepository"/>.
/// Responsible for managing expense records in the database.
/// </summary>
public sealed class SqliteExpenseRepository : IExpenseRepository
{
    private readonly SQLiteHelper _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteExpenseRepository"/> class.
    /// </summary>
    /// <param name="db">Database helper used for SQLite operations.</param>
    public SqliteExpenseRepository(SQLiteHelper db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves all expenses for a specific budget cycle.
    /// </summary>
    /// <param name="cycleId">Budget cycle identifier.</param>
    public List<Expense> GetExpenses(int cycleId)
        => _db.GetExpenses(cycleId);

    /// <summary>
    /// Inserts a new expense using a database transaction.
    /// </summary>
    /// <param name="expense">Expense to insert.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void InsertExpense(Expense expense, SqliteTransaction tx)
        => _db.InsertExpense(expense, tx);

    /// <summary>
    /// Updates an existing expense using a database transaction.
    /// </summary>
    /// <param name="expense">Expense containing updated data.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void UpdateExpense(Expense expense, SqliteTransaction tx)
        => _db.UpdateExpense(expense, tx);

    /// <summary>
    /// Deletes an expense using a database transaction.
    /// </summary>
    /// <param name="expenseId">Expense identifier.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void DeleteExpense(int expenseId, SqliteTransaction tx)
        => _db.DeleteExpense(expenseId, tx);

    /// <summary>
    /// Updates an expense using an internal transaction.
    /// </summary>
    /// <param name="expense">Expense to update.</param>
    public void Update(Expense expense)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        UpdateExpense(expense, tx);

        tx.Commit();
    }

    /// <summary>
    /// Deletes an expense using an internal transaction.
    /// </summary>
    /// <param name="expenseId">Expense identifier.</param>
    public void Delete(int expenseId)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        DeleteExpense(expenseId, tx);

        tx.Commit();
    }

    /// <summary>
    /// Deletes all expenses for a specific budget cycle.
    /// </summary>
    /// <param name="cycleId">Budget cycle identifier.</param>
    public void DeleteAllForCycle(int cycleId)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        _db.DeleteAllExpensesForCycle(cycleId, tx);

        tx.Commit();
    }
}
