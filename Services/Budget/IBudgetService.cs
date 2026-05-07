using Masroofy.App.Models;
using Masroofy.App.Strategies;

namespace Masroofy.App.Services;

/// <summary>
/// Defines the contract for budget management operations, covering cycle lifecycle,
/// expense tracking, debt logging, category administration, daily limit calculations,
/// forecasting, and user PIN changes.
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Returns the currently active calculation strategy used for safe daily limit computation.
    /// </summary>
    /// <returns>The current <see cref="ICalculationStrategy"/> instance.</returns>
    ICalculationStrategy GetStrategy();

    /// <summary>
    /// Replaces the active calculation strategy, allowing runtime switching between
    /// different limit calculation algorithms.
    /// </summary>
    /// <param name="strategy">The new strategy to apply.</param>
    void SetStrategy(ICalculationStrategy strategy);

    /// <summary>
    /// Retrieves the most recently created budget cycle for the current user,
    /// treated as the active cycle.
    /// </summary>
    /// <returns>The active <see cref="BudgetCycle"/>, or null if none exists.</returns>
    BudgetCycle? GetCycle();

    /// <summary>
    /// Creates and persists a new budget cycle for the current user with the given
    /// allowance and date range.
    /// </summary>
    /// <param name="totalAllowance">The total monetary allowance for the cycle.</param>
    /// <param name="startDate">The start date of the cycle.</param>
    /// <param name="endDate">The end date of the cycle.</param>
    /// <returns>The newly created <see cref="BudgetCycle"/> with its assigned Id.</returns>
    BudgetCycle CreateCycle(decimal totalAllowance, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Retrieves all expenses associated with the specified budget cycle.
    /// </summary>
    /// <param name="cycleId">The primary key of the budget cycle to query.</param>
    /// <returns>A list of <see cref="Expense"/> records for the cycle.</returns>
    List<Expense> GetExpenses(int cycleId);

    /// <summary>
    /// Records a new expense against the given cycle, deducting the amount from
    /// the remaining balance.
    /// </summary>
    /// <param name="cycle">The active budget cycle to charge the expense against.</param>
    /// <param name="amount">The monetary amount of the expense.</param>
    /// <param name="category">The category name to assign to the expense.</param>
    /// <param name="when">The date and time the expense occurred.</param>
    void AddExpense(BudgetCycle cycle, decimal amount, string category, DateTime when);

    /// <summary>
    /// Updates an existing expense with a new amount and category, adjusting the
    /// cycle's remaining balance by the difference.
    /// </summary>
    /// <param name="cycle">The active budget cycle the expense belongs to.</param>
    /// <param name="original">The original expense record to update.</param>
    /// <param name="newAmount">The replacement monetary amount.</param>
    /// <param name="category">The replacement category name.</param>
    void UpdateExpense(BudgetCycle cycle, Expense original, decimal newAmount, string category);

    /// <summary>
    /// Deletes an expense from the given cycle and restores its amount to the
    /// remaining balance.
    /// </summary>
    /// <param name="cycle">The active budget cycle the expense belongs to.</param>
    /// <param name="expense">The expense record to delete.</param>
    void DeleteExpense(BudgetCycle cycle, Expense expense);

    /// <summary>
    /// Applies a daily rollover to the cycle if today is a new day within the active
    /// cycle range that has not yet been rolled over. Has no effect if today falls
    /// outside the cycle range or a rollover was already applied today.
    /// </summary>
    /// <param name="cycle">The active budget cycle to evaluate and update.</param>
    void ApplyRolloverIfNeeded(BudgetCycle cycle);

    /// <summary>
    /// Calculates the safe daily spending limit for a given date by dividing the
    /// adjusted remaining balance across the days left in the cycle, factoring in
    /// any active debt impact.
    /// </summary>
    /// <param name="cycle">The active budget cycle used for date range and allowance data.</param>
    /// <param name="forDate">The date to calculate the limit for; defaults to today if null.</param>
    /// <returns>The calculated safe daily spending limit.</returns>
    decimal CalculateSafeDailyLimit(BudgetCycle cycle, DateTime? forDate = null);

    /// <summary>
    /// Checks whether yesterday's total spending exceeded the calculated safe daily
    /// limit for that day. Returns false if yesterday falls outside the active cycle range.
    /// </summary>
    /// <param name="cycle">The active budget cycle to evaluate.</param>
    /// <returns>True if yesterday's spending exceeded the safe daily limit; otherwise false.</returns>
    bool WasYesterdayOverspent(BudgetCycle cycle);

    /// <summary>
    /// Determines whether the current user has spent 80% or more of their total
    /// cycle allowance.
    /// </summary>
    /// <param name="cycle">The active budget cycle to evaluate.</param>
    /// <returns>True if total spending has reached or exceeded 80% of the allowance; otherwise false.</returns>
    bool IsAtEightyPercent(BudgetCycle cycle);

    /// <summary>
    /// Retrieves all available expense category names.
    /// </summary>
    /// <returns>A list of category name strings.</returns>
    List<string> GetCategories();

    /// <summary>
    /// Retrieves the most recent audit log entries across all operations.
    /// </summary>
    /// <returns>A list of <see cref="AuditLog"/> records.</returns>
    List<AuditLog> GetAuditLogs();

    /// <summary>
    /// Retrieves all debt records belonging to the current user.
    /// </summary>
    /// <returns>A list of <see cref="DebtRecord"/> objects.</returns>
    List<DebtRecord> GetDebts();

    /// <summary>
    /// Adds a new expense category.
    /// </summary>
    /// <param name="name">The name of the category to create.</param>
    void AddCategory(string name);

    /// <summary>
    /// Exports all expenses for the given cycle to a CSV file at the specified path,
    /// ordered by date ascending. Overwrites any existing file at that path.
    /// </summary>
    /// <param name="cycle">The budget cycle whose expenses will be exported.</param>
    /// <param name="filePath">The full file path where the CSV will be written.</param>
    void ExportExpensesToCsv(BudgetCycle cycle, string filePath);

    /// <summary>
    /// Logs a new debt record for the current user. When the type is Borrowing and
    /// <paramref name="applyToBalance"/> is true, the amount is added to the active
    /// cycle's remaining balance.
    /// </summary>
    /// <param name="amount">The monetary amount of the debt.</param>
    /// <param name="type">The debt type, either Borrowing or Lending.</param>
    /// <param name="note">A descriptive note about the debt.</param>
    /// <param name="applyToBalance">When true and type is Borrowing, the amount is credited to the active cycle balance.</param>
    void AddDebt(decimal amount, string type, string note, bool applyToBalance = false);

    /// <summary>
    /// Updates an existing debt record with new values. The debt's timestamp is
    /// refreshed to the current time.
    /// </summary>
    /// <param name="debt">The debt record to update; its Id identifies the row.</param>
    /// <param name="amount">The new monetary amount.</param>
    /// <param name="type">The new debt type, either Borrowing or Lending.</param>
    /// <param name="note">The new descriptive note.</param>
    void UpdateDebt(DebtRecord debt, decimal amount, string type, string note);

    /// <summary>
    /// Deletes a debt record by its Id.
    /// </summary>
    /// <param name="debt">The debt record to delete.</param>
    void DeleteDebt(DebtRecord debt);

    /// <summary>
    /// Produces a plain-text forecast indicating whether the current spending velocity
    /// is on track to stay within the cycle's remaining balance, or at risk of exceeding it.
    /// </summary>
    /// <param name="cycle">The active budget cycle to evaluate.</param>
    /// <returns>A single-sentence forecast string describing the projected budget status.</returns>
    string ForecastStatus(BudgetCycle cycle);

    /// <summary>
    /// Updates the current user's PIN by hashing the new value and persisting it.
    /// </summary>
    /// <param name="newPin">The plain-text PIN to hash and store.</param>
    void ChangePin(string newPin);
}
