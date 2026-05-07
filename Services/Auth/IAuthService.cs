using Masroofy.App.Models;

namespace Masroofy.App.Services;

/// <summary>
/// Defines authentication and user registration operations for the application.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Checks whether the application is running for the first time (no users exist).
    /// </summary>
    /// <returns>True if no users are registered; otherwise false.</returns>
    bool IsFirstRun();

    /// <summary>
    /// Authenticates a user using username and PIN.
    /// </summary>
    /// <param name="username">The username of the account.</param>
    /// <param name="pin">The plain PIN entered by the user.</param>
    /// <returns>The authenticated <see cref="User"/> if credentials are valid; otherwise null.</returns>
    User? Authenticate(string username, string pin);

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="username">Desired username.</param>
    /// <param name="pin">User PIN.</param>
    /// <param name="role">Role of the user (e.g., Admin or User).</param>
    /// <param name="masterKey">Secret key required for admin registration.</param>
    /// <param name="message">Output message indicating success or failure reason.</param>
    /// <returns>True if registration succeeds; otherwise false.</returns>
    bool Register(string username, string pin, string role, string? masterKey, out string message);
}
