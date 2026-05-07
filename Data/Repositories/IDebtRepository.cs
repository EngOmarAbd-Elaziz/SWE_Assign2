using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// Defines operations for managing debt records in the system.
/// </summary>
public interface IDebtRepository
{
    /// <summary>
    /// Retrieves all debt records for a specific user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>A list of debt records.</returns>
    List<DebtRecord> GetDebts(int userId);

    /// <summary>
    /// Inserts a new debt record using a database transaction.
    /// </summary>
    /// <param name="debt">Debt record to insert.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void InsertDebt(DebtRecord debt, SqliteTransaction tx);

    /// <summary>
    /// Updates an existing debt record using a database transaction.
    /// </summary>
    /// <param name="debt">Debt record containing updated information.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void UpdateDebt(DebtRecord debt, SqliteTransaction tx);

    /// <summary>
    /// Deletes a debt record using a database transaction.
    /// </summary>
    /// <param name="debtId">Identifier of the debt record.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void DeleteDebt(int debtId, SqliteTransaction tx);

    /// <summary>
    /// Retrieves all debts for a specific user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>A list of all debt records.</returns>
    List<DebtRecord> GetAll(int userId);

    /// <summary>
    /// Adds a new debt record.
    /// </summary>
    /// <param name="debt">Debt record to add.</param>
    void Add(DebtRecord debt);

    /// <summary>
    /// Updates an existing debt record.
    /// </summary>
    /// <param name="debt">Debt record containing updated data.</param>
    void Update(DebtRecord debt);

    /// <summary>
    /// Deletes a debt record.
    /// </summary>
    /// <param name="debtId">Identifier of the debt record to delete.</param>
    void Delete(int debtId);
}
