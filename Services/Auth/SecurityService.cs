using System.Security.Cryptography;
using System.Text;

namespace Masroofy.App.Services;

public sealed class SecurityService
{
    public string HashPinSha256(string pin)
    {
        var bytes = Encoding.UTF8.GetBytes(pin);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public bool VerifyPin(string pin, string expectedHash)
    {
        return string.Equals(HashPinSha256(pin), expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}
