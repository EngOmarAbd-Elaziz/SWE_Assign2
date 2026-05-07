using Masroofy.App.Models;
using Masroofy.App.Strategies;

namespace Masroofy.App.Services;

/// <summary>
/// Defines all business operations related to
/// budget management, expenses, debts,
/// forecasting, and categories.
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Gets the current calculation strategy.
    /// </summary>
    /// <returns>Calculation strategy instance.</returns>
    ICalculationStrategy GetStrategy();

    /// <summary>
    /// Changes the active calculation strategy.
    /// </summary>
    /// <param name="strategy">New strategy object.</param>
    void SetStrategy(ICalculationStrategy strategy);

    /// <summary>
    /// Retrieves the active budget cycle.
    /// </summary>
    /// <returns>Current budget cycle or null.</returns>
    BudgetCycle? GetCycle();

    /// <summary>
    /// Creates a new budget cycle.
    /// </summary>
    /// <param name="totalAllowance">Total budget amount.</param>
    /// <param name="startDate">Cycle start date.</param>
    /// <param name="endDate">Cycle end date.</param>
    /// <returns>Created budget cycle.</returns>
    BudgetCycle CreateCycle(
        decimal totalAllowance,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Retrieves all expenses for a cycle.
    /// </summary>
    /// <param name="cycleId">Budget cycle ID.</param>
    /// <returns>List of expenses.</returns>
    List<Expense> GetExpenses(int cycleId);

    /// <summary>
    /// Adds a new expense to the cycle.
    /// </summary>
    /// <param name="cycle">Current budget cycle.</param>
    /// <param name="amount">Expense amount.</param>
    /// <param name="category">Expense category.</param>
    /// <param name="when">Expense date.</param>
    void AddExpense(
        BudgetCycle cycle,
        decimal amount,
        string category,
        DateTime when);

    /// <summary>
    /// Updates an existing expense.
    /// </summary>
    /// <param name="cycle">Current budget cycle.</param>
    /// <param name="original">Original expense.</param>
    /// <param name="newAmount">Updated amount.</param>
    /// <param name="category">Updated category.</param>
    void UpdateExpense(
        BudgetCycle cycle,
        Expense original,
        decimal newAmount,
        string category);

    /// <summary>
    /// Deletes an expense from the cycle.
    /// </summary>
    /// <param name="cycle">Current budget cycle.</param>
    /// <param name="expense">Expense to delete.</param>
    void DeleteExpense(BudgetCycle cycle, Expense expense);

    /// <summary>
    /// Applies daily rollover updates if needed.
    /// </summary>
    /// <param name="cycle">Current budget cycle.</param>
    void ApplyRolloverIfNeeded(BudgetCycle cycle);

    /// <summary>
    /// Calculates the safe daily spending limit.
    /// </summary>
    /// <param name="cycle">Current budget cycle.</param>
    /// <param name="forDate">Optional target date.</param>
    /// <returns>Safe daily limit amount.</returns>
    decimal CalculateSafeDailyLimit(
        BudgetCycle cycle,
        DateTime? forDate = null);

    /// <summary>
    /// Checks if yesterday's spending exceeded the limit.
    /// </summary>
    /// <param name="cycle">Current budget cycle.</param>
    /// <returns>True if overspent; otherwise false.</returns>
    bool WasYesterdayOverspent(BudgetCycle cycle);

    /// <summary>
    /// Determines whether 80% of the budget was spent.
    /// </summary>
    /// <param name="cycle">Current budget cycle.</param>
    /// <returns>True if spending reached 80%.</returns>
    bool IsAtEightyPercent(BudgetCycle cycle);

    /// <summary>
    /// Retrieves all available categories.
    /// </summary>
    /// <returns>List of category names.</returns>
    List<string> GetCategories();

    /// <summary>
    /// Retrieves recent audit logs.
    /// </summary>
    /// <returns>List of audit logs.</returns>
    List<AuditLog> GetAuditLogs();

    /// <summary>
    /// Retrieves all debt records for the user.
    /// </summary>
    /// <returns>List of debts.</returns>
    List<DebtRecord> GetDebts();

    /// <summary>
    /// Adds a new expense category.
    /// </summary>
    /// <param name="name">Category name.</param>
    void AddCategory(string name);

    /// <summary>
    /// Exports expenses to a CSV file.
    /// </summary>
    /// <param name="cycle">Budget cycle.</param>
    /// <param name="filePath">Destination file path.</param>
    void ExportExpensesToCsv(
        BudgetCycle cycle,
        string filePath);

    /// <summary>
    /// Adds a new debt record.
    /// </summary>
    /// <param name="amount">Debt amount.</param>
    /// <param name="type">Debt type.</param>
    /// <param name="note">Debt note.</param>
    /// <param name="applyToBalance">
    /// Indicates whether balance should be updated.
    /// </param>
    void AddDebt(
        decimal amount,
        string type,
        string note,
        bool applyToBalance = false);

    /// <summary>
    /// Updates an existing debt record.
    /// </summary>
    /// <param name="debt">Debt object.</param>
    /// <param name="amount">Updated amount.</param>
    /// <param name="type">Updated type.</param>
    /// <param name="note">Updated note.</param>
    void UpdateDebt(
        DebtRecord debt,
        decimal amount,
        string type,
        string note);

    /// <summary>
    /// Deletes a debt record.
    /// </summary>
    /// <param name="debt">Debt to delete.</param>
    void DeleteDebt(DebtRecord debt);

    /// <summary>
    /// Predicts future budget status.
    /// </summary>
    /// <param name="cycle">Current budget cycle.</param>
    /// <returns>Forecast status message.</returns>
    string ForecastStatus(BudgetCycle cycle);

    /// <summary>
    /// Changes the current user's PIN.
    /// </summary>
    /// <param name="newPin">New PIN value.</param>
    void ChangePin(string newPin);
}
