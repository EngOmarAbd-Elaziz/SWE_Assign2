using Masroofy.App.Models;

namespace Masroofy.App.Data.Repositories;

/// <summary>
/// Defines operations for managing application users,
/// including authentication data and role management.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets the total number of users in the system.
    /// </summary>
    /// <returns>Total user count.</returns>
    int GetUserCount();

    /// <summary>
    /// Retrieves all users from the system.
    /// </summary>
    /// <returns>A list of users.</returns>
    List<User> GetUsers();

    /// <summary>
    /// Retrieves a user by their name.
    /// </summary>
    /// <param name="name">Username to search for.</param>
    /// <returns>The user if found; otherwise null.</returns>
    User? GetUserByName(string name);

    /// <summary>
    /// Retrieves the default system user.
    /// </summary>
    /// <returns>The default user if exists; otherwise null.</returns>
    User? GetDefaultUser();

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="name">User name.</param>
    /// <param name="pinHash">Hashed PIN for authentication.</param>
    /// <param name="role">User role (e.g., Admin, User).</param>
    void CreateUser(string name, string pinHash, string role);

    /// <summary>
    /// Updates the PIN hash for a specific user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="pinHash">New hashed PIN value.</param>
    void UpdateUserPinHash(int userId, string pinHash);

    /// <summary>
    /// Updates the administrator password hash.
    /// </summary>
    /// <param name="adminPasswordHash">New admin password hash.</param>
    void UpdateAdminPassword(string adminPasswordHash);
}
