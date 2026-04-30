namespace Masroofy.App.Models;

public sealed class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string PinHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}
