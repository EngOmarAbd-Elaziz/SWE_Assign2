namespace Masroofy.App.Models;

/// <summary>
/// Represents a financial expense recorded by a user within a budget cycle.
/// </summary>
public sealed class Expense
{
    /// <summary>
    /// Unique identifier for the expense record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the user who created the expense.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Identifier of the budget cycle associated with this expense.
    /// </summary>
    public int CycleId { get; set; }

    /// <summary>
    /// Amount of money spent.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Category of the expense (e.g., Food, Transport).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Date when the expense was recorded.
    /// </summary>
    public DateTime Date { get; set; }
}
