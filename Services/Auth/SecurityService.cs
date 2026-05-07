using System.Security.Cryptography;
using System.Text;

namespace Masroofy.App.Services;

/// <summary>
/// Provides security-related utilities such as hashing and verifying PIN codes.
/// </summary>
public sealed class SecurityService
{
    /// <summary>
    /// Generates a SHA256 hash for the given PIN value.
    /// </summary>
    /// <param name="pin">Plain PIN input.</param>
    /// <returns>Hexadecimal representation of the hashed PIN.</returns>
    public string HashPinSha256(string pin)
    {
        var bytes = Encoding.UTF8.GetBytes(pin);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Verifies whether a provided PIN matches the stored hashed value.
    /// </summary>
    /// <param name="pin">Plain PIN entered by the user.</param>
    /// <param name="expectedHash">Stored hashed PIN value.</param>
    /// <returns>True if the PIN matches the hash; otherwise false.</returns>
    public bool VerifyPin(string pin, string expectedHash)
    {
        return string.Equals(
            HashPinSha256(pin),
            expectedHash,
            StringComparison.OrdinalIgnoreCase);
    }
}
