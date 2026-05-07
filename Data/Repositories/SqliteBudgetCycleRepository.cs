using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IBudgetCycleRepository"/>.
/// Handles all database operations related to budget cycles.
/// </summary>
public sealed class SqliteBudgetCycleRepository : IBudgetCycleRepository
{
    private readonly SQLiteHelper _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteBudgetCycleRepository"/> class.
    /// </summary>
    /// <param name="db">Database helper used for SQLite operations.</param>
    public SqliteBudgetCycleRepository(SQLiteHelper db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves the active budget cycle for a specific user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>The active budget cycle if found; otherwise null.</returns>
    public BudgetCycle? GetActiveCycle(int userId)
        => _db.GetActiveCycle(userId);

    /// <summary>
    /// Inserts a new budget cycle into the database using a transaction.
    /// </summary>
    /// <param name="cycle">Budget cycle to insert.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    /// <returns>The ID of the inserted cycle.</returns>
    public int InsertCycle(BudgetCycle cycle, SqliteTransaction tx)
        => _db.InsertCycle(cycle, tx);

    /// <summary>
    /// Updates an existing budget cycle using a transaction.
    /// </summary>
    /// <param name="cycle">Budget cycle containing updated data.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void UpdateCycle(BudgetCycle cycle, SqliteTransaction tx)
        => _db.UpdateCycle(cycle, tx);

    /// <summary>
    /// Updates a budget cycle using an internal transaction.
    /// </summary>
    /// <param name="cycle">Budget cycle to update.</param>
    public void Update(BudgetCycle cycle)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        UpdateCycle(cycle, tx);

        tx.Commit();
    }
}
