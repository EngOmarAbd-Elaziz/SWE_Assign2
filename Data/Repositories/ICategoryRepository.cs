using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// Defines operations for managing expense categories in the system.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Retrieves all available expense categories.
    /// </summary>
    /// <returns>A list of category names.</returns>
    List<string> GetCategories();

    /// <summary>
    /// Adds a new expense category to the database.
    /// </summary>
    /// <param name="category">Name of the category to add.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void AddCategory(string category, SqliteTransaction tx);

    /// <summary>
    /// Deletes an existing expense category from the database.
    /// </summary>
    /// <param name="categoryName">Name of the category to delete.</param>
    /// <param name="tx">Active SQLite transaction.</param>
    void DeleteCategory(string categoryName, SqliteTransaction tx);
}
