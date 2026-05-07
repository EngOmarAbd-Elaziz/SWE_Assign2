using Masroofy.App.Models;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IUserRepository"/>.
/// Responsible for managing user-related database operations such as
/// creation, retrieval, and authentication data updates.
/// </summary>
public sealed class SqliteUserRepository : IUserRepository
{
    private readonly SQLiteHelper _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteUserRepository"/> class.
    /// </summary>
    /// <param name="db">Database helper used for SQLite operations.</param>
    public SqliteUserRepository(SQLiteHelper db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets the total number of users in the system.
    /// </summary>
    public int GetUserCount() => _db.GetUserCount();

    /// <summary>
    /// Retrieves all users from the database.
    /// </summary>
    public List<User> GetUsers() => _db.GetUsers();

    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <param name="name">Username to search for.</param>
    public User? GetUserByName(string name) => _db.GetUserByName(name);

    /// <summary>
    /// Retrieves the default system user.
    /// </summary>
    public User? GetDefaultUser() => _db.GetDefaultUser();

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="name">User name.</param>
    /// <param name="pinHash">Hashed PIN for authentication.</param>
    /// <param name="role">User role (e.g., Admin, User).</param>
    public void CreateUser(string name, string pinHash, string role) => _db.CreateUser(name, pinHash, role);

    /// <summary>
    /// Updates the PIN hash for a specific user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="pinHash">New hashed PIN value.</param>
    public void UpdateUserPinHash(int userId, string pinHash) => _db.UpdateUserPinHash(userId, pinHash);

    /// <summary>
    /// Updates the administrator password hash.
    /// </summary>
    /// <param name="adminPasswordHash">New admin password hash.</param>
    public void UpdateAdminPassword(string adminPasswordHash) => _db.UpdateAdminPassword(adminPasswordHash);
}
