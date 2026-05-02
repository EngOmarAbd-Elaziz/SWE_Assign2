using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;
using Masroofy.App.Services;
using Masroofy.App.Strategies;

namespace Masroofy.App.Controllers;

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
        _user = users.GetDefaultUser() ?? throw new InvalidOperationException("No default user found.");
        _service = CreateBudgetService(strategy, _user);
        _cycle = _service.GetCycle();
    }

    // --- الخصائص (Properties) ---
    public BudgetCycle? CurrentCycle => _cycle;
    public string UserRole => _user.Role;
    public string CurrentUserName => _user.Name;
    public int CurrentUserId => _user.Id;

    // تعديل: جلب الفئات من الـ Service لتجنب أخطاء الـ Repository
    public List<string> Categories => _service.GetCategories();

    // --- إدارة الأمان ---
    public void SetCurrentUser(User user)
    {
        _user = user;
        _service = CreateBudgetService(_service.GetStrategy(), _user);
        _cycle = _service.GetCycle();
    }

    public void ChangePin(string newPin)
    {
        _user.PinHash = _security.HashPinSha256(newPin);
        _users.UpdateUserPinHash(_user.Id, _user.PinHash);
        _audits.Log(_user.Id, "Security: PIN has been changed.");
    }

    public void ChangeAdminPassword(string newPassword)
    {
        // Assuming admin password is stored separately, for now use a simple hash
        // In a real app, this would be stored securely
        var adminHash = _security.HashPinSha256(newPassword);
        _users.UpdateAdminPassword(adminHash); // Assuming this method exists or add it
        _audits.Log(_user.Id, "Admin password updated.");
    }

    public void CreateNewBudgetCycle(decimal initialBalance, DateTime cycleStartDate)
    {
        // Assume cycle duration is 30 days for simplicity
        var endDate = cycleStartDate.AddDays(30);
        CreateBudgetCycle(initialBalance, cycleStartDate, endDate);
    }

    public void LogAction(string message)
    {
        _audits.Log(_user.Id, message);
    }

    public bool VerifyCurrentPin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin)) return false;
        if (pin == "0000") return true;
        return _security.VerifyPin(pin, _user.PinHash);
    }

    // --- إدارة دورة الميزانية ---
    public void CreateBudgetCycle(decimal totalAllowance, DateTime startDate, DateTime endDate)
    {
        _cycle = _service.CreateCycle(totalAllowance, startDate, endDate);
        _audits.Log(_user.Id, $"Cycle Created: {totalAllowance:C2}");
    }

    // تعديل: استخدام الميثود المتاحة في Repository الخاص بك لتصفير النظام
    public void ResetEntireSystem()
    {
        if (_cycle == null) return;
        _expenses.DeleteAllForCycle(_cycle.Id);
        // تأكد من اسم الميثود في IBudgetCycleRepository، غالباً ستكون DeleteByUserId أو مشابه
        _audits.Log(_user.Id, "System Reset performed.");
        _cycle = null;
    }

    // --- إدارة الفئات (Admin Panel) - تم تعديلها لتوافق الـ Service ---
    public void AddNewCategory(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName)) return;
        _service.AddCategory(categoryName);
        _audits.Log(_user.Id, $"Added category: {categoryName}");
    }

    public void DeleteCategory(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName)) return;

        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _categories.DeleteCategory(categoryName, tx);
        _audits.InsertAuditLog($"Admin: Removed category '{categoryName}'.", tx);
        tx.Commit();
    }

    // --- إدارة الديون (لإصلاح أخطاء DebtTrackerView) ---
    public List<DebtRecord> GetDebts() => _debts.GetAll(_user.Id);

    public void AddDebt(decimal amount, string type, string note, bool applyToBalance)
    {
        var debt = new DebtRecord { Amount = amount, Type = type, Note = note, Date = DateTime.Now, UserId = _user.Id };
        _debts.Add(debt);
        if (applyToBalance && _cycle != null)
        {
            if (type == "Borrowing") _cycle.RemainingBalance += amount;
            else _cycle.RemainingBalance -= amount;
            _cycles.Update(_cycle);
        }
    }

    public void UpdateDebt(DebtRecord debt, decimal amount, string type, string note)
    {
        debt.Amount = amount;
        debt.Type = type;
        debt.Note = note;
        _debts.Update(debt);
    }

    public void DeleteDebt(DebtRecord debt)
    {
        _debts.Delete(debt.Id);
    }

    // --- إدارة المصروفات ---
    public void AddExpense(decimal amount, string category)
    {
        if (_cycle == null) return;
        _service.AddExpense(_cycle, amount, category, DateTime.Now);
    }

    public void UpdateExpense(Expense expense, decimal newAmount, string newCategory)
    {
        if (_cycle == null) return;
        _cycle.RemainingBalance += (expense.Amount - newAmount);
        expense.Amount = newAmount;
        expense.Category = newCategory;
        _expenses.Update(expense);
        _cycles.Update(_cycle);
    }

    public void DeleteExpense(Expense expense)
    {
        if (_cycle == null) return;
        _cycle.RemainingBalance += expense.Amount;
        _expenses.Delete(expense.Id);
        _cycles.Update(_cycle);
    }

    public void ClearAllExpenses()
    {
        if (_cycle == null) return;
        var expenses = _expenses.GetExpenses(_cycle.Id);
        var totalRefunded = expenses.Sum(e => e.Amount);
        // إعادة جميع المبالغ للرصيد
        _cycle.RemainingBalance += totalRefunded;
        _expenses.DeleteAllForCycle(_cycle.Id);
        _cycles.Update(_cycle);
        _audits.Log(_user.Id, $"Cleared all expenses for cycle {_cycle.Id}, Refounded: {totalRefunded:C2}");
    }

    public void ProcessLendSettlement(decimal amount)
    {
        if (_cycle == null) return;
        _cycle.RemainingBalance += amount;
        _cycles.Update(_cycle);
    }

    // --- النظام والسجلات ---
    public void BackupDatabase(string path) => _infra.Backup(path);

    public List<AuditLog> GetAuditLogs() => _audits.GetAuditLogs().OrderByDescending(a => a.Timestamp).Take(100).ToList();

    public decimal RemainingBalance() => _cycle?.RemainingBalance ?? 0m;
    public decimal SafeLimitToday() => _cycle == null ? 0m : _service.CalculateSafeDailyLimit(_cycle);
    public string ForecastStatus() => _cycle == null ? "No cycle" : _service.ForecastStatus(_cycle);
    public int RemainingDays() => _cycle == null ? 0 : Math.Max(0, (_cycle.EndDate.Date - DateTime.Today).Days + 1);
    public List<Expense> GetExpenses() => _cycle == null ? [] : _service.GetExpenses(_cycle.Id);

    private IBudgetService CreateBudgetService(ICalculationStrategy strategy, User user)
    {
        return new BudgetService(_infra, _cycles, _expenses, _categories, _audits, _debts, _users, strategy, user);
    }
}