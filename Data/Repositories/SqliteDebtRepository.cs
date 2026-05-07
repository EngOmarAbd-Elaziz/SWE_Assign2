using Masroofy.App.Models;
using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IDebtRepository"/>.
/// Handles CRUD operations for debt records in the database.
/// </summary>
public sealed class SqliteDebtRepository : IDebtRepository
{
    private readonly SQLiteHelper _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDebtRepository"/> class.
    /// </summary>
    /// <param name="db">Database helper used for SQLite operations.</param>
    public SqliteDebtRepository(SQLiteHelper db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves all debt records for a specific user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    public List<DebtRecord> GetDebts(int userId)
        => _db.GetDebts(userId);

    /// <summary>
    /// Inserts a new debt record using a database transaction.
    /// </summary>
    /// <param name="debt">Debt record to insert.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void InsertDebt(DebtRecord debt, SqliteTransaction tx)
        => _db.InsertDebt(debt, tx);

    /// <summary>
    /// Updates an existing debt record using a database transaction.
    /// </summary>
    /// <param name="debt">Debt record containing updated data.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void UpdateDebt(DebtRecord debt, SqliteTransaction tx)
        => _db.UpdateDebt(debt, tx);

    /// <summary>
    /// Deletes a debt record using a database transaction.
    /// </summary>
    /// <param name="debtId">Debt record identifier.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void DeleteDebt(int debtId, SqliteTransaction tx)
        => _db.DeleteDebt(debtId, tx);

    /// <summary>
    /// Retrieves all debt records for a user (alias for GetDebts).
    /// </summary>
    /// <param name="userId">User identifier.</param>
    public List<DebtRecord> GetAll(int userId)
        => GetDebts(userId);

    /// <summary>
    /// Adds a new debt record using an internal transaction.
    /// </summary>
    /// <param name="debt">Debt record to add.</param>
    public void Add(DebtRecord debt)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        InsertDebt(debt, tx);

        tx.Commit();
    }

    /// <summary>
    /// Updates a debt record using an internal transaction.
    /// </summary>
    /// <param name="debt">Debt record to update.</param>
    public void Update(DebtRecord debt)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        UpdateDebt(debt, tx);

        tx.Commit();
    }

    /// <summary>
    /// Deletes a debt record using an internal transaction.
    /// </summary>
    /// <param name="debtId">Debt record identifier.</param>
    public void Delete(int debtId)
    {
        using var connection = _db.CreateConnection();
        connection.Open();

        using var tx = connection.BeginTransaction();

        DeleteDebt(debtId, tx);

        tx.Commit();
    }
}
