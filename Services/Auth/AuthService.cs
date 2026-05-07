using Masroofy.App.Core;
using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;

namespace Masroofy.App.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly SecurityService _security;

    public AuthService(IUserRepository users, SecurityService security)
    {
        _users = users;
        _security = security;
    }

    public bool IsFirstRun() => _users.GetUserCount() == 0;

    public User? Authenticate(string username, string pin)
    {
        var user = _users.GetUserByName(username);
        if (user == null)
        {
            return null;
        }

        return _security.VerifyPin(pin, user.PinHash) ? user : null;
    }

    public bool Register(string username, string pin, string role, string? masterKey, out string message)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(pin))
        {
            message = "Username and PIN are required.";
            return false;
        }

        if (role == "Admin" && !string.Equals(masterKey, AppConstants.AdminSecretKey, StringComparison.Ordinal))
        {
            message = "Invalid secret master key for admin registration.";
            return false;
        }

        if (_users.GetUserByName(username) != null)
        {
            message = "User already exists.";
            return false;
        }

        _users.CreateUser(username.Trim(), _security.HashPinSha256(pin), role);
        message = "User registered successfully.";
        return true;
    }
}
