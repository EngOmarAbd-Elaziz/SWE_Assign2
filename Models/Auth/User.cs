namespace Masroofy.App.Models;

/// <summary>
/// Represents a system user with authentication and role information.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username of the system user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Plain PIN value (used temporarily or for input validation).
    /// Note: should not be stored permanently in production systems.
    /// </summary>
    public string Pin { get; set; } = string.Empty;

    /// <summary>
    /// Hashed PIN used for secure authentication.
    /// </summary>
    public string PinHash { get; set; } = string.Empty;

    /// <summary>
    /// Role assigned to the user (e.g., Admin, User).
    /// </summary>
    public string Role { get; set; } = "User";
}
