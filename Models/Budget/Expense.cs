namespace Masroofy.App.Models;

public sealed class Expense
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CycleId { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
