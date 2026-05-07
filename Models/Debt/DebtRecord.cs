namespace Masroofy.App.Models;

/// <summary>
/// Represents a debt record for a user, including borrowing or repayment information.
/// </summary>
public sealed class DebtRecord
{
    /// <summary>
    /// Unique identifier for the debt record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the user associated with this debt.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Amount of the debt (borrowed or repaid).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Type of the debt (e.g., Borrowing, Repayment).
    /// </summary>
    public string Type { get; set; } = "Borrowing";

    /// <summary>
    /// Additional notes or description for the debt record.
    /// </summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Date when the debt record was created.
    /// </summary>
    public DateTime Date { get; set; }
}
