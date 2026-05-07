using Masroofy.App.Core;
using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;

namespace Masroofy.App.Services;

/// <summary>
/// Handles user authentication and registration, including first-run detection,
/// PIN verification, role-based access control, and master key validation for admin accounts.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly SecurityService _security;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthService"/> with the required user repository
    /// and security service dependencies.
    /// </summary>
    /// <param name="users">The repository used to query and persist user records.</param>
    /// <param name="security">The service used for PIN hashing and verification.</param>
    public AuthService(IUserRepository users, SecurityService security)
    {
        _users = users;
        _security = security;
    }

    /// <summary>
    /// Determines whether the application has no registered users yet,
    /// indicating this is the first run and initial setup is required.
    /// </summary>
    /// <returns>True if no users exist in the repository; otherwise false.</returns>
    public bool IsFirstRun() => _users.GetUserCount() == 0;

    /// <summary>
    /// Attempts to authenticate a user by verifying their username and PIN against stored credentials.
    /// </summary>
    /// <param name="username">The username to look up.</param>
    /// <param name="pin">The plain-text PIN to verify against the stored hash.</param>
    /// <returns>The matching <see cref="User"/> if authentication succeeds; otherwise null.</returns>
    public User? Authenticate(string username, string pin)
    {
        var user = _users.GetUserByName(username);
        if (user == null)
        {
            return null;
        }
        return _security.VerifyPin(pin, user.PinHash) ? user : null;
    }

    /// <summary>
    /// Registers a new user after validating the provided credentials, role, and master key.
    /// Admin registration requires a valid master key matching <see cref="AppConstants.AdminSecretKey"/>.
    /// Returns false with a descriptive message if any validation step fails.
    /// </summary>
    /// <param name="username">The desired username; must not be null or whitespace.</param>
    /// <param name="pin">The desired PIN; must not be null or whitespace.</param>
    /// <param name="role">The role to assign to the new user, e.g. Admin or User.</param>
    /// <param name="masterKey">The secret master key required only when registering an Admin account.</param>
    /// <param name="message">
    /// When the method returns, contains a success confirmation or a description of why registration failed.
    /// </param>
    /// <returns>True if the user was registered successfully; otherwise false.</returns>
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
