using Masroofy.App.Models;

namespace Masroofy.App.Services;

public interface IAuthService
{
    bool IsFirstRun();
    User? Authenticate(string username, string pin);
    bool Register(string username, string pin, string role, string? masterKey, out string message);
}
