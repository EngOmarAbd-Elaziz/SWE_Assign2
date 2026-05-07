namespace Masroofy.App.Models;

/// <summary>
/// Represents a budgeting cycle for a user, defining a fixed time period
/// with an allocated allowance and tracking remaining balance.
/// </summary>
public sealed class BudgetCycle
{
    /// <summary>
    /// Unique identifier for the budget cycle.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the user who owns this budget cycle.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Total amount allocated for the cycle.
    /// </summary>
    public decimal TotalAllowance { get; set; }

    /// <summary>
    /// Start date of the budget cycle.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the budget cycle.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Remaining balance available in the cycle.
    /// </summary>
    public decimal RemainingBalance { get; set; }

    /// <summary>
    /// The last date when the budget was rolled over or updated.
    /// </summary>
    public DateTime LastRolloverDate { get; set; }
}
