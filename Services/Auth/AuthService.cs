using Masroofy.App.Core;
using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;

namespace Masroofy.App.Services;

/// <summary>
/// Provides authentication and registration services for application users,
/// including login validation and secure user creation.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly SecurityService _security;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="users">User repository for database access.</param>
    /// <param name="security">Security service for hashing and PIN verification.</param>
    public AuthService(IUserRepository users, SecurityService security)
    {
        _users = users;
        _security = security;
    }

    /// <summary>
    /// Checks whether the system is being run for the first time (no users exist).
    /// </summary>
    /// <returns>True if no users exist; otherwise false.</returns>
    public bool IsFirstRun()
        => _users.GetUserCount() == 0;

    /// <summary>
    /// Authenticates a user using username and PIN.
    /// </summary>
    /// <param name="username">Username of the user.</param>
    /// <param name="pin">Plain PIN entered by the user.</param>
    /// <returns>The authenticated user if credentials are valid; otherwise null.</returns>
    public User? Authenticate(string username, string pin)
    {
        var user = _users.GetUserByName(username);

        if (user == null)
            return null;

        return _security.VerifyPin(pin, user.PinHash)
            ? user
            : null;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="username">Desired username.</param>
    /// <param name="pin">User PIN.</param>
    /// <param name="role">User role (Admin or User).</param>
    /// <param name="masterKey">Secret key required for admin registration.</param>
    /// <param name="message">Output message describing result.</param>
    /// <returns>True if registration succeeded; otherwise false.</returns>
    public bool Register(
        string username,
        string pin,
        string role,
        string? masterKey,
        out string message)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(pin))
        {
            message = "Username and PIN are required.";
            return false;
        }

        if (role == "Admin" &&
            !string.Equals(masterKey, AppConstants.AdminSecretKey, StringComparison.Ordinal))
        {
            message = "Invalid secret master key for admin registration.";
            return false;
        }

        if (_users.GetUserByName(username) != null)
        {
            message = "User already exists.";
            return false;
        }

        _users.CreateUser(
            username.Trim(),
            _security.HashPinSha256(pin),
            role);

        message = "User registered successfully.";
        return true;
    }
}
