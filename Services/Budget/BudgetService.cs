using System.Text;
using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;
using Masroofy.App.Services;
using Masroofy.App.Strategies;

namespace Masroofy.App.Services;

/// <summary>
/// Main service responsible for managing all budget-related operations for a specific user,
/// including cycle management, expense tracking, debt logging, category administration,
/// daily limit calculations, rollover logic, forecasting, and PIN changes.
/// </summary>
public sealed class BudgetService : IBudgetService
{
    private readonly IInfrastructureRepository _infra;
    private readonly IBudgetCycleRepository _cycles;
    private readonly IExpenseRepository _expenses;
    private readonly ICategoryRepository _categories;
    private readonly IAuditRepository _audits;
    private readonly IDebtRepository _debts;
    private readonly IUserRepository _users;
    private ICalculationStrategy _strategy;
    private readonly User _user;
    private readonly SecurityService _security = new();

    /// <summary>
    /// Initializes a new instance of <see cref="BudgetService"/> with all required
    /// repositories, a calculation strategy, and the currently authenticated user.
    /// </summary>
    /// <param name="infra">Provides raw database connection creation for transactional operations.</param>
    /// <param name="cycles">Repository for reading and persisting budget cycle records.</param>
    /// <param name="expenses">Repository for reading and persisting expense records.</param>
    /// <param name="categories">Repository for reading and managing expense categories.</param>
    /// <param name="audits">Repository for writing audit log entries.</param>
    /// <param name="debts">Repository for reading and persisting debt records.</param>
    /// <param name="users">Repository for reading and updating user records.</param>
    /// <param name="strategy">The calculation strategy used to determine the safe daily spending limit.</param>
    /// <param name="user">The authenticated user this service instance operates on behalf of.</param>
    public BudgetService(
        IInfrastructureRepository infra,
        IBudgetCycleRepository cycles,
        IExpenseRepository expenses,
        ICategoryRepository categories,
        IAuditRepository audits,
        IDebtRepository debts,
        IUserRepository users,
        ICalculationStrategy strategy,
        User user)
    {
        _infra = infra;
        _cycles = cycles;
        _expenses = expenses;
        _categories = categories;
        _audits = audits;
        _debts = debts;
        _users = users;
        _strategy = strategy;
        _user = user;
    }

    /// <summary>
    /// Returns the currently active calculation strategy used for safe daily limit computation.
    /// </summary>
    /// <returns>The current <see cref="ICalculationStrategy"/> instance.</returns>
    public ICalculationStrategy GetStrategy() => _strategy;

    /// <summary>
    /// Replaces the active calculation strategy with the provided one, allowing runtime
    /// switching between different limit calculation algorithms.
    /// </summary>
    /// <param name="strategy">The new strategy to use for daily limit calculations.</param>
    public void SetStrategy(ICalculationStrategy strategy) => _strategy = strategy;

    /// <summary>
    /// Retrieves the most recently created budget cycle for the current user,
    /// treated as the active cycle.
    /// </summary>
    /// <returns>The active <see cref="BudgetCycle"/>, or null if none exists.</returns>
    public BudgetCycle? GetCycle() => _cycles.GetActiveCycle(_user.Id);

    /// <summary>
    /// Creates a new budget cycle for the current user with the specified allowance and date range,
    /// persisting it along with an audit log entry in a single atomic transaction.
    /// </summary>
    /// <param name="totalAllowance">The total monetary allowance for the cycle.</param>
    /// <param name="startDate">The start date of the cycle; only the date component is used.</param>
    /// <param name="endDate">The end date of the cycle; only the date component is used.</param>
    /// <returns>The newly created and persisted <see cref="BudgetCycle"/> with its assigned Id.</returns>
    public BudgetCycle CreateCycle(decimal totalAllowance, DateTime startDate, DateTime endDate)
    {
        var cycle = new BudgetCycle
        {
            UserId = _user.Id,
            TotalAllowance = totalAllowance,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            RemainingBalance = totalAllowance,
            LastRolloverDate = startDate.Date
        };

        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        cycle.Id = _cycles.InsertCycle(cycle, tx);
        _audits.InsertAuditLog("Budget cycle created", tx);
        tx.Commit();
        return cycle;
    }

    /// <summary>
    /// Retrieves all expenses associated with the specified budget cycle.
    /// </summary>
    /// <param name="cycleId">The primary key of the budget cycle to query.</param>
    /// <returns>A list of <see cref="Expense"/> records for the cycle.</returns>
    public List<Expense> GetExpenses(int cycleId) => _expenses.GetExpenses(cycleId);

    /// <summary>
    /// Records a new expense against the given cycle, deducting the amount from the
    /// remaining balance and writing an audit entry, all within a single atomic transaction.
    /// </summary>
    /// <param name="cycle">The active budget cycle to charge the expense against.</param>
    /// <param name="amount">The monetary amount of the expense.</param>
    /// <param name="category">The category name to assign to the expense.</param>
    /// <param name="when">The date and time the expense occurred.</param>
    public void AddExpense(BudgetCycle cycle, decimal amount, string category, DateTime when)
    {
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();

        _expenses.InsertExpense(new Expense
        {
            CycleId = cycle.Id,
            UserId = _user.Id,
            Amount = amount,
            Category = category,
            Date = when
        }, tx);

        cycle.RemainingBalance -= amount;
        _cycles.UpdateCycle(cycle, tx);
        _audits.InsertAuditLog($"Expense added: {amount:0.00} ({category})", tx);
        tx.Commit();
    }

    /// <summary>
    /// Updates an existing expense with a new amount and category, adjusting the cycle's
    /// remaining balance by the difference and recording an audit entry, all within a single
    /// atomic transaction.
    /// </summary>
    /// <param name="cycle">The active budget cycle the expense belongs to.</param>
    /// <param name="original">The original expense record to update.</param>
    /// <param name="newAmount">The replacement monetary amount.</param>
    /// <param name="category">The replacement category name.</param>
    public void UpdateExpense(BudgetCycle cycle, Expense original, decimal newAmount, string category)
    {
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();

        _expenses.UpdateExpense(new Expense
        {
            Id = original.Id,
            CycleId = original.CycleId,
            UserId = original.UserId,
            Amount = newAmount,
            Category = category,
            Date = original.Date
        }, tx);

        cycle.RemainingBalance += original.Amount - newAmount;
        _cycles.UpdateCycle(cycle, tx);
        _audits.InsertAuditLog($"Expense updated: #{original.Id}", tx);
        tx.Commit();
    }

    /// <summary>
    /// Deletes an expense from the given cycle, restoring its amount to the remaining
    /// balance and writing an audit entry, all within a single atomic transaction.
    /// </summary>
    /// <param name="cycle">The active budget cycle the expense belongs to.</param>
    /// <param name="expense">The expense record to delete.</param>
    public void DeleteExpense(BudgetCycle cycle, Expense expense)
    {
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _expenses.DeleteExpense(expense.Id, tx);
        cycle.RemainingBalance += expense.Amount;
        _cycles.UpdateCycle(cycle, tx);
        _audits.InsertAuditLog($"Expense deleted: #{expense.Id}", tx);
        tx.Commit();
    }

    /// <summary>
    /// Applies a daily rollover to the cycle if today is a new day within the active cycle range
    /// that has not yet been rolled over. Resets the remaining balance to the total allowance
    /// minus all spending to date, then persists the update with an audit entry.
    /// Has no effect if today falls outside the cycle range or a rollover was already applied today.
    /// </summary>
    /// <param name="cycle">The active budget cycle to evaluate and update.</param>
    public void ApplyRolloverIfNeeded(BudgetCycle cycle)
    {
        var today = DateTime.Today;
        if (today <= cycle.LastRolloverDate.Date || today < cycle.StartDate.Date || today > cycle.EndDate.Date)
        {
            return;
        }

        cycle.LastRolloverDate = today;
        cycle.RemainingBalance = cycle.TotalAllowance - GetTotalSpent(cycle.Id);
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _cycles.UpdateCycle(cycle, tx);
        _audits.InsertAuditLog("Daily rollover applied", tx);
        tx.Commit();
    }

    /// <summary>
    /// Calculates the safe daily spending limit for a given date by dividing the adjusted
    /// remaining balance across the number of days left in the cycle. Debt impact is factored
    /// in by adding borrowing amounts and subtracting lending amounts from the balance.
    /// </summary>
    /// <param name="cycle">The active budget cycle used for date range and allowance data.</param>
    /// <param name="forDate">The date to calculate the limit for; defaults to today if null.</param>
    /// <returns>The calculated safe daily spending limit as a decimal amount.</returns>
    public decimal CalculateSafeDailyLimit(BudgetCycle cycle, DateTime? forDate = null)
    {
        var date = (forDate ?? DateTime.Today).Date;
        var remainingDays = Math.Max(1, (cycle.EndDate.Date - date).Days + 1);
        var remainingBeforeDate = cycle.TotalAllowance - GetTotalSpent(cycle.Id, date.AddDays(-1));
        var debtImpact = _debts.GetDebts(_user.Id).Sum(d => d.Type == "Borrowing" ? d.Amount : -d.Amount);
        var tempCycle = new BudgetCycle { RemainingBalance = remainingBeforeDate + debtImpact };
        return _strategy.CalculateSafeLimit(tempCycle, remainingDays);
    }

    /// <summary>
    /// Checks whether yesterday's total spending exceeded the calculated safe daily limit for that day.
    /// Returns false if yesterday falls outside the active cycle's date range.
    /// </summary>
    /// <param name="cycle">The active budget cycle to evaluate.</param>
    /// <returns>True if yesterday's spending exceeded the safe daily limit; otherwise false.</returns>
    public bool WasYesterdayOverspent(BudgetCycle cycle)
    {
        var yesterday = DateTime.Today.AddDays(-1).Date;
        if (yesterday < cycle.StartDate.Date || yesterday > cycle.EndDate.Date)
        {
            return false;
        }

        var yesterdayLimit = CalculateSafeDailyLimit(cycle, yesterday);
        var spentYesterday = GetTotalSpentOnDate(cycle.Id, yesterday);
        return spentYesterday > yesterdayLimit;
    }

    /// <summary>
    /// Determines whether the current user has spent 80% or more of their total cycle allowance.
    /// </summary>
    /// <param name="cycle">The active budget cycle to evaluate.</param>
    /// <returns>True if total spending has reached or exceeded 80% of the allowance; otherwise false.</returns>
    public bool IsAtEightyPercent(BudgetCycle cycle)
    {
        var spent = cycle.TotalAllowance - cycle.RemainingBalance;
        return spent >= cycle.TotalAllowance * 0.8m;
    }

    /// <summary>
    /// Retrieves all available expense category names.
    /// </summary>
    /// <returns>A list of category name strings.</returns>
    public List<string> GetCategories() => _categories.GetCategories();

    /// <summary>
    /// Retrieves the most recent audit log entries across all operations.
    /// </summary>
    /// <returns>A list of <see cref="AuditLog"/> records.</returns>
    public List<AuditLog> GetAuditLogs() => _audits.GetAuditLogs();

    /// <summary>
    /// Retrieves all debt records belonging to the current user.
    /// </summary>
    /// <returns>A list of <see cref="DebtRecord"/> objects.</returns>
    public List<DebtRecord> GetDebts() => _debts.GetDebts(_user.Id);

    /// <summary>
    /// Adds a new expense category and records the action in the audit log,
    /// both within a single atomic transaction.
    /// </summary>
    /// <param name="name">The name of the category to create.</param>
    public void AddCategory(string name)
    {
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _categories.AddCategory(name, tx);
        _audits.InsertAuditLog($"Category created: {name}", tx);
        tx.Commit();
    }

    /// <summary>
    /// Exports all expenses for the given cycle to a CSV file at the specified path,
    /// ordered by date ascending. The file includes Id, CycleId, Amount, Category, and Date columns.
    /// </summary>
    /// <param name="cycle">The budget cycle whose expenses will be exported.</param>
    /// <param name="filePath">The full file path where the CSV will be written. Overwrites existing files.</param>
    public void ExportExpensesToCsv(BudgetCycle cycle, string filePath)
    {
        var expenses = GetExpenses(cycle.Id).OrderBy(e => e.Date).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("Id,CycleId,Amount,Category,Date");
        foreach (var expense in expenses)
        {
            sb.AppendLine($"{expense.Id},{expense.CycleId},{expense.Amount:0.00},\"{expense.Category}\",{expense.Date:O}");
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    /// Logs a new debt record for the current user and optionally adds the borrowed amount
    /// to the active cycle's remaining balance if the debt type is Borrowing. All changes
    /// are persisted within a single atomic transaction.
    /// </summary>
    /// <param name="amount">The monetary amount of the debt.</param>
    /// <param name="type">The debt type, either Borrowing or Lending.</param>
    /// <param name="note">A descriptive note about the debt.</param>
    /// <param name="applyToBalance">
    /// When true and the type is Borrowing, the amount is added to the active cycle's remaining balance.
    /// </param>
    public void AddDebt(decimal amount, string type, string note, bool applyToBalance = false)
    {
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        var debt = new DebtRecord
        {
            UserId = _user.Id,
            Amount = amount,
            Type = type,
            Note = note,
            Date = DateTime.Now
        };
        _debts.InsertDebt(debt, tx);

        if (applyToBalance && type == "Borrowing")
        {
            var cycle = GetCycle();
            if (cycle != null)
            {
                cycle.RemainingBalance += amount;
                _cycles.UpdateCycle(cycle, tx);
            }
        }
        _audits.InsertAuditLog($"Debt logged: {type} {amount:0.00}", tx);
        tx.Commit();
    }

    /// <summary>
    /// Updates an existing debt record with new values and records the change in the audit log,
    /// both within a single atomic transaction. The debt's timestamp is refreshed to the current time.
    /// </summary>
    /// <param name="debt">The debt record to update; its Id is used to identify the row.</param>
    /// <param name="amount">The new monetary amount.</param>
    /// <param name="type">The new debt type, either Borrowing or Lending.</param>
    /// <param name="note">The new descriptive note.</param>
    public void UpdateDebt(DebtRecord debt, decimal amount, string type, string note)
    {
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        debt.Amount = amount;
        debt.Type = type;
        debt.Note = note;
        debt.Date = DateTime.Now;
        _debts.UpdateDebt(debt, tx);
        _audits.InsertAuditLog($"Debt updated: #{debt.Id}", tx);
        tx.Commit();
    }

    /// <summary>
    /// Deletes a debt record by its Id and records the deletion in the audit log,
    /// both within a single atomic transaction.
    /// </summary>
    /// <param name="debt">The debt record to delete.</param>
    public void DeleteDebt(DebtRecord debt)
    {
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _debts.DeleteDebt(debt.Id, tx);
        _audits.InsertAuditLog($"Debt deleted: #{debt.Id}", tx);
        tx.Commit();
    }

    /// <summary>
    /// Produces a plain-text forecast indicating whether the current spending velocity
    /// is on track to stay within the cycle's remaining balance, or at risk of exceeding it.
    /// Velocity is calculated as total spent divided by days elapsed since the cycle started.
    /// </summary>
    /// <param name="cycle">The active budget cycle to evaluate.</param>
    /// <returns>A single-sentence forecast string describing the projected budget status.</returns>
    public string ForecastStatus(BudgetCycle cycle)
    {
        var daysPassed = Math.Max(1, (DateTime.Today - cycle.StartDate.Date).Days + 1);
        var spent = cycle.TotalAllowance - cycle.RemainingBalance;
        var spendVelocity = spent / daysPassed;
        var remainingDays = Math.Max(1, (cycle.EndDate.Date - DateTime.Today).Days + 1);
        var projectedSpend = spendVelocity * remainingDays;
        return projectedSpend <= cycle.RemainingBalance
            ? "On track: budget likely to last."
            : "Risk: spending velocity may exceed cycle balance.";
    }

    /// <summary>
    /// Updates the current user's PIN by hashing the new value and persisting it via the user repository.
    /// </summary>
    /// <param name="newPin">The plain-text PIN to hash and store.</param>
    public void ChangePin(string newPin)
    {
        _users.UpdateUserPinHash(_user.Id, _security.HashPinSha256(newPin));
    }

    /// <summary>
    /// Sums all expenses for a given cycle, optionally capped to a specific date.
    /// </summary>
    /// <param name="cycleId">The primary key of the budget cycle to sum.</param>
    /// <param name="upToDate">When provided, only expenses on or before this date are included.</param>
    /// <returns>The total amount spent within the specified scope.</returns>
    private decimal GetTotalSpent(int cycleId, DateTime? upToDate = null)
    {
        var expenses = _expenses.GetExpenses(cycleId);
        return upToDate == null
            ? expenses.Sum(e => e.Amount)
            : expenses.Where(e => e.Date.Date <= upToDate.Value.Date).Sum(e => e.Amount);
    }

    /// <summary>
    /// Sums all expenses for a given cycle that occurred on a specific calendar date.
    /// </summary>
    /// <param name="cycleId">The primary key of the budget cycle to query.</param>
    /// <param name="date">The specific date to filter expenses by.</param>
    /// <returns>The total amount spent on the given date.</returns>
    private decimal GetTotalSpentOnDate(int cycleId, DateTime date)
    {
        return _expenses.GetExpenses(cycleId)
            .Where(e => e.Date.Date == date.Date)
            .Sum(e => e.Amount);
    }
}
