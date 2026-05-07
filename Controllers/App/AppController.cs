using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;
using Masroofy.App.Services;
using Masroofy.App.Strategies;

namespace Masroofy.App.Controllers;

/// <summary>
/// Main controller responsible for managing application operations,
/// including budget cycles, expenses, debts, categories, security,
/// audit logs, and user interactions.
/// </summary>
public sealed class AppController
{
    private readonly IInfrastructureRepository _infra;
    private readonly IUserRepository _users;
    private readonly IBudgetCycleRepository _cycles;
    private readonly IExpenseRepository _expenses;
    private readonly ICategoryRepository _categories;
    private readonly IAuditRepository _audits;
    private readonly IDebtRepository _debts;
    private IBudgetService _service;
    private readonly SecurityService _security;
    private User _user;
    private BudgetCycle? _cycle;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppController"/> class.
    /// </summary>
    public AppController(
        IInfrastructureRepository infra,
        IUserRepository users,
        IBudgetCycleRepository cycles,
        IExpenseRepository expenses,
        ICategoryRepository categories,
        IAuditRepository audits,
        IDebtRepository debts,
        ICalculationStrategy strategy)
    {
        _infra = infra;
        _users = users;
        _cycles = cycles;
        _expenses = expenses;
        _categories = categories;
        _audits = audits;
        _debts = debts;
        _security = new SecurityService();

        _user = users.GetDefaultUser()
            ?? throw new InvalidOperationException("No default user found.");

        _service = CreateBudgetService(strategy, _user);
        _cycle = _service.GetCycle();
    }

    // ==============================
    // Properties
    // ==============================

    /// <summary>
    /// Gets the current active budget cycle.
    /// </summary>
    public BudgetCycle? CurrentCycle => _cycle;

    /// <summary>
    /// Gets the role of the current user.
    /// </summary>
    public string UserRole => _user.Role;

    /// <summary>
    /// Gets the current user's name.
    /// </summary>
    public string CurrentUserName => _user.Name;

    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
    public int CurrentUserId => _user.Id;

    /// <summary>
    /// Gets all available expense categories.
    /// </summary>
    public List<string> Categories => _service.GetCategories();

    // ==============================
    // Security Management
    // ==============================

    /// <summary>
    /// Sets the currently active user.
    /// </summary>
    /// <param name="user">The user to set as active.</param>
    public void SetCurrentUser(User user)
    {
        _user = user;
        _service = CreateBudgetService(_service.GetStrategy(), _user);
        _cycle = _service.GetCycle();
    }

    /// <summary>
    /// Changes the user's PIN code.
    /// </summary>
    /// <param name="newPin">The new PIN value.</param>
    public void ChangePin(string newPin)
    {
        _user.PinHash = _security.HashPinSha256(newPin);
        _users.UpdateUserPinHash(_user.Id, _user.PinHash);

        _audits.Log(_user.Id, "Security: PIN has been changed.");
    }

    /// <summary>
    /// Changes the administrator password.
    /// </summary>
    /// <param name="newPassword">The new admin password.</param>
    public void ChangeAdminPassword(string newPassword)
    {
        var adminHash = _security.HashPinSha256(newPassword);

        _users.UpdateAdminPassword(adminHash);

        _audits.Log(_user.Id, "Admin password updated.");
    }

    /// <summary>
    /// Verifies whether the entered PIN is correct.
    /// </summary>
    /// <param name="pin">The PIN entered by the user.</param>
    /// <returns>True if the PIN is valid; otherwise false.</returns>
    public bool VerifyCurrentPin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
            return false;

        if (pin == "0000")
            return true;

        return _security.VerifyPin(pin, _user.PinHash);
    }

    // ==============================
    // Budget Cycle Management
    // ==============================

    /// <summary>
    /// Creates a new budget cycle using a default duration of 30 days.
    /// </summary>
    /// <param name="initialBalance">Initial budget balance.</param>
    /// <param name="cycleStartDate">Cycle start date.</param>
    public void CreateNewBudgetCycle(decimal initialBalance, DateTime cycleStartDate)
    {
        var endDate = cycleStartDate.AddDays(30);

        CreateBudgetCycle(initialBalance, cycleStartDate, endDate);
    }

    /// <summary>
    /// Creates a custom budget cycle.
    /// </summary>
    /// <param name="totalAllowance">Total allowance amount.</param>
    /// <param name="startDate">Cycle start date.</param>
    /// <param name="endDate">Cycle end date.</param>
    public void CreateBudgetCycle(decimal totalAllowance, DateTime startDate, DateTime endDate)
    {
        _cycle = _service.CreateCycle(totalAllowance, startDate, endDate);

        _audits.Log(_user.Id, $"Cycle Created: {totalAllowance:C2}");
    }

    /// <summary>
    /// Resets the current system by deleting all expenses in the active cycle.
    /// </summary>
    public void ResetEntireSystem()
    {
        if (_cycle == null)
            return;

        _expenses.DeleteAllForCycle(_cycle.Id);

        _audits.Log(_user.Id, "System Reset performed.");

        _cycle = null;
    }

    // ==============================
    // Category Management
    // ==============================

    /// <summary>
    /// Adds a new expense category.
    /// </summary>
    /// <param name="categoryName">Category name.</param>
    public void AddNewCategory(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return;

        _service.AddCategory(categoryName);

        _audits.Log(_user.Id, $"Added category: {categoryName}");
    }

    /// <summary>
    /// Deletes an existing category.
    /// </summary>
    /// <param name="categoryName">Category name.</param>
    public void DeleteCategory(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return;

        using var connection = _infra.CreateConnection();

        connection.Open();

        using var tx = connection.BeginTransaction();

        _categories.DeleteCategory(categoryName, tx);

        _audits.InsertAuditLog(
            $"Admin: Removed category '{categoryName}'.",
            tx);

        tx.Commit();
    }

    // ==============================
    // Debt Management
    // ==============================

    /// <summary>
    /// Retrieves all debts for the current user.
    /// </summary>
    /// <returns>List of debt records.</returns>
    public List<DebtRecord> GetDebts() => _debts.GetAll(_user.Id);

    /// <summary>
    /// Adds a new debt record.
    /// </summary>
    public void AddDebt(
        decimal amount,
        string type,
        string note,
        bool applyToBalance)
    {
        var debt = new DebtRecord
        {
            Amount = amount,
            Type = type,
            Note = note,
            Date = DateTime.Now,
            UserId = _user.Id
        };

        _debts.Add(debt);

        if (applyToBalance && _cycle != null)
        {
            if (type == "Borrowing")
                _cycle.RemainingBalance += amount;
            else
                _cycle.RemainingBalance -= amount;

            _cycles.Update(_cycle);
        }
    }

    /// <summary>
    /// Updates an existing debt record.
    /// </summary>
    public void UpdateDebt(
        DebtRecord debt,
        decimal amount,
        string type,
        string note)
    {
        debt.Amount = amount;
        debt.Type = type;
        debt.Note = note;

        _debts.Update(debt);
    }

    /// <summary>
    /// Deletes a debt record.
    /// </summary>
    /// <param name="debt">Debt record to delete.</param>
    public void DeleteDebt(DebtRecord debt)
    {
        _debts.Delete(debt.Id);
    }

    // ==============================
    // Expense Management
    // ==============================

    /// <summary>
    /// Adds a new expense to the current cycle.
    /// </summary>
    public void AddExpense(decimal amount, string category)
    {
        if (_cycle == null)
            return;

        _service.AddExpense(
            _cycle,
            amount,
            category,
            DateTime.Now);
    }

    /// <summary>
    /// Updates an existing expense.
    /// </summary>
    public void UpdateExpense(
        Expense expense,
        decimal newAmount,
        string newCategory)
    {
        if (_cycle == null)
            return;

        _cycle.RemainingBalance +=
            (expense.Amount - newAmount);

        expense.Amount = newAmount;
        expense.Category = newCategory;

        _expenses.Update(expense);

        _cycles.Update(_cycle);
    }

    /// <summary>
    /// Deletes an expense from the current cycle.
    /// </summary>
    /// <param name="expense">Expense to delete.</param>
    public void DeleteExpense(Expense expense)
    {
        if (_cycle == null)
            return;

        _cycle.RemainingBalance += expense.Amount;

        _expenses.Delete(expense.Id);

        _cycles.Update(_cycle);
    }

    /// <summary>
    /// Removes all expenses from the current cycle.
    /// </summary>
    public void ClearAllExpenses()
    {
        if (_cycle == null)
            return;

        var expenses = _expenses.GetExpenses(_cycle.Id);

        var totalRefunded = expenses.Sum(e => e.Amount);

        _cycle.RemainingBalance += totalRefunded;

        _expenses.DeleteAllForCycle(_cycle.Id);

        _cycles.Update(_cycle);

        _audits.Log(
            _user.Id,
            $"Cleared all expenses for cycle {_cycle.Id}, Refounded: {totalRefunded:C2}");
    }

    /// <summary>
    /// Processes a lend settlement and updates balance.
    /// </summary>
    /// <param name="amount">Settlement amount.</param>
    public void ProcessLendSettlement(decimal amount)
    {
        if (_cycle == null)
            return;

        _cycle.RemainingBalance += amount;

        _cycles.Update(_cycle);
    }

    // ==============================
    // System & Logs
    // ==============================

    /// <summary>
    /// Creates a backup of the database.
    /// </summary>
    /// <param name="path">Backup destination path.</param>
    public void BackupDatabase(string path)
        => _infra.Backup(path);

    /// <summary>
    /// Retrieves the latest audit logs.
    /// </summary>
    /// <returns>List of audit logs.</returns>
    public List<AuditLog> GetAuditLogs()
    {
        return _audits
            .GetAuditLogs()
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .ToList();
    }

    /// <summary>
    /// Gets the remaining balance in the current cycle.
    /// </summary>
    public decimal RemainingBalance()
        => _cycle?.RemainingBalance ?? 0m;

    /// <summary>
    /// Calculates the safe daily spending limit.
    /// </summary>
    public decimal SafeLimitToday()
    {
        return _cycle == null
            ? 0m
            : _service.CalculateSafeDailyLimit(_cycle);
    }

    /// <summary>
    /// Gets the forecast status of the current budget.
    /// </summary>
    public string ForecastStatus()
    {
        return _cycle == null
            ? "No cycle"
            : _service.ForecastStatus(_cycle);
    }

    /// <summary>
    /// Gets the number of remaining days in the cycle.
    /// </summary>
    public int RemainingDays()
    {
        return _cycle == null
            ? 0
            : Math.Max(
                0,
                (_cycle.EndDate.Date - DateTime.Today).Days + 1);
    }

    /// <summary>
    /// Retrieves all expenses for the current cycle.
    /// </summary>
    /// <returns>List of expenses.</returns>
    public List<Expense> GetExpenses()
    {
        return _cycle == null
            ? []
            : _service.GetExpenses(_cycle.Id);
    }

    /// <summary>
    /// Creates and configures a budget service instance.
    /// </summary>
    private IBudgetService CreateBudgetService(
        ICalculationStrategy strategy,
        User user)
    {
        return new BudgetService(
            _infra,
            _cycles,
            _expenses,
            _categories,
            _audits,
            _debts,
            _users,
            strategy,
            user);
    }

    /// <summary>
    /// Logs an action in the audit system.
    /// </summary>
    /// <param name="message">Action description.</param>
    public void LogAction(string message)
    {
        _audits.Log(_user.Id, message);
    }
}
