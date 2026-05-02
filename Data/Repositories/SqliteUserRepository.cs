using Masroofy.App.Models;

namespace Masroofy.App.Data.Repositories;

public sealed class SqliteUserRepository : IUserRepository
{
    private readonly SQLiteHelper _db;

    public SqliteUserRepository(SQLiteHelper db)
    {
        _db = db;
    }

    public int GetUserCount() => _db.GetUserCount();

    public List<User> GetUsers() => _db.GetUsers();

    public User? GetUserByName(string name) => _db.GetUserByName(name);

    public User? GetDefaultUser() => _db.GetDefaultUser();

    public void CreateUser(string name, string pinHash, string role) => _db.CreateUser(name, pinHash, role);

    public void UpdateUserPinHash(int userId, string pinHash) => _db.UpdateUserPinHash(userId, pinHash);

    public void UpdateAdminPassword(string adminPasswordHash) => _db.UpdateAdminPassword(adminPasswordHash);
}
