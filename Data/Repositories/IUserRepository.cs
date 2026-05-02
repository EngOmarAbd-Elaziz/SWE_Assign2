using Masroofy.App.Models;

namespace Masroofy.App.Data.Repositories;

public interface IUserRepository
{
    int GetUserCount();
    List<User> GetUsers();
    User? GetUserByName(string name);
    User? GetDefaultUser();
    void CreateUser(string name, string pinHash, string role);
    void UpdateUserPinHash(int userId, string pinHash);
    void UpdateAdminPassword(string adminPasswordHash);
}
