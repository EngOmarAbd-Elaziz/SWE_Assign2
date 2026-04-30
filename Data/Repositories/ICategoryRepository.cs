using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public interface ICategoryRepository
{
    List<string> GetCategories();
    void AddCategory(string category, SqliteTransaction tx);
    void DeleteCategory(string categoryName, SqliteTransaction tx);
}
