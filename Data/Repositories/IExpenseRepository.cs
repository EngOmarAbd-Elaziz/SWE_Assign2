using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// Defines operations for managing expenses in the system.
/// </summary>
public interface IExpenseRepository
{
    /// <summary>
    /// Retrieves all expenses associated with a specific budget cycle.
    /// </summary>
    /// <param name="cycleId">Budget cycle identifier.</param>
    /// <returns>A list of expenses.</returns>
    List<Expense> GetExpenses(int cycleId);

    /// <summary>
    /// Inserts a new expense using a database transaction.
    /// </summary>
    /// <param name="expense">Expense object to insert.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void InsertExpense(Expense expense, SqliteTransaction tx);

    /// <summary>
    /// Updates an existing expense using a database transaction.
    /// </summary>
    /// <param name="expense">Expense object containing updated data.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void UpdateExpense(Expense expense, SqliteTransaction tx);

    /// <summary>
    /// Deletes an expense using a database transaction.
    /// </summary>
    /// <param name="expenseId">Identifier of the expense to delete.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void DeleteExpense(int expenseId, SqliteTransaction tx);

    /// <summary>
    /// Updates an existing expense.
    /// </summary>
    /// <param name="expense">Expense object containing updated data.</param>
    void Update(Expense expense);

    /// <summary>
    /// Deletes an expense by its identifier.
    /// </summary>
    /// <param name="expenseId">Identifier of the expense to delete.</param>
    void Delete(int expenseId);

    /// <summary>
    /// Deletes all expenses associated with a specific budget cycle.
    /// </summary>
    /// <param name="cycleId">Budget cycle identifier.</param>
    void DeleteAllForCycle(int cycleId);
}
