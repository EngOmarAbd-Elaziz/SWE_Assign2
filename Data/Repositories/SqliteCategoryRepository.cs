using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="ICategoryRepository"/>.
/// Responsible for managing expense categories in the database.
/// </summary>
public sealed class SqliteCategoryRepository : ICategoryRepository
{
    private readonly SQLiteHelper _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteCategoryRepository"/> class.
    /// </summary>
    /// <param name="db">Database helper used for SQLite operations.</param>
    public SqliteCategoryRepository(SQLiteHelper db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves all available categories.
    /// </summary>
    public List<string> GetCategories()
        => _db.GetCategories();

    /// <summary>
    /// Adds a new category using a database transaction.
    /// </summary>
    /// <param name="category">Category name.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void AddCategory(string category, SqliteTransaction tx)
        => _db.AddCategory(category, tx);

    /// <summary>
    /// Deletes a category using a database transaction.
    /// </summary>
    /// <param name="categoryName">Name of the category to delete.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    public void DeleteCategory(string categoryName, SqliteTransaction tx)
        => _db.DeleteCategory(categoryName, tx);
}
