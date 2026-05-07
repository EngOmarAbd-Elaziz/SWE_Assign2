using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// Defines operations for managing budget cycles in the database.
/// </summary>
public interface IBudgetCycleRepository
{
    /// <summary>
    /// Retrieves the currently active budget cycle for a specific user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>
    /// The active <see cref="BudgetCycle"/> if found; otherwise null.
    /// </returns>
    BudgetCycle? GetActiveCycle(int userId);

    /// <summary>
    /// Inserts a new budget cycle into the database using a transaction.
    /// </summary>
    /// <param name="cycle">Budget cycle to insert.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    /// <returns>The ID of the newly created cycle.</returns>
    int InsertCycle(BudgetCycle cycle, SqliteTransaction tx);

    /// <summary>
    /// Updates an existing budget cycle using a database transaction.
    /// </summary>
    /// <param name="cycle">Budget cycle containing updated data.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void UpdateCycle(BudgetCycle cycle, SqliteTransaction tx);

    /// <summary>
    /// Updates an existing budget cycle without requiring a transaction.
    /// </summary>
    /// <param name="cycle">Budget cycle containing updated data.</param>
    void Update(BudgetCycle cycle);
}
