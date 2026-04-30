using Microsoft.Data.Sqlite;

namespace Masroofy.App.Data.Repositories;

public sealed class SqliteCategoryRepository : ICategoryRepository
{
    private readonly SQLiteHelper _db;

    public SqliteCategoryRepository(SQLiteHelper db)
    {
        _db = db;
    }

    public List<string> GetCategories() => _db.GetCategories();

    public void AddCategory(string category, SqliteTransaction tx) => _db.AddCategory(category, tx);

    public void DeleteCategory(string categoryName, SqliteTransaction tx) => _db.DeleteCategory(categoryName, tx);
}
