namespace Masroofy.App.Models;

public sealed class BudgetCycle
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAllowance { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime LastRolloverDate { get; set; }
}
