namespace Masroofy.App.Models;

public sealed class DebtRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = "Borrowing";
    public string Note { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
