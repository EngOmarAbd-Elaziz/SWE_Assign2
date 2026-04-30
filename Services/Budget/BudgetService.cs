using System.Text;
using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;
using Masroofy.App.Services;
using Masroofy.App.Strategies;

namespace Masroofy.App.Services;

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

    public ICalculationStrategy GetStrategy() => _strategy;

    public void SetStrategy(ICalculationStrategy strategy) => _strategy = strategy;

    public BudgetCycle? GetCycle() => _cycles.GetActiveCycle(_user.Id);

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

    public List<Expense> GetExpenses(int cycleId) => _expenses.GetExpenses(cycleId);

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

    public decimal CalculateSafeDailyLimit(BudgetCycle cycle, DateTime? forDate = null)
    {
        var date = (forDate ?? DateTime.Today).Date;
        var remainingDays = Math.Max(1, (cycle.EndDate.Date - date).Days + 1);
        var remainingBeforeDate = cycle.TotalAllowance - GetTotalSpent(cycle.Id, date.AddDays(-1));
        var debtImpact = _debts.GetDebts(_user.Id).Sum(d => d.Type == "Borrowing" ? d.Amount : -d.Amount);
        var tempCycle = new BudgetCycle { RemainingBalance = remainingBeforeDate + debtImpact };
        return _strategy.CalculateSafeLimit(tempCycle, remainingDays);
    }

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

    public bool IsAtEightyPercent(BudgetCycle cycle)
    {
        var spent = cycle.TotalAllowance - cycle.RemainingBalance;
        return spent >= cycle.TotalAllowance * 0.8m;
    }

    public List<string> GetCategories() => _categories.GetCategories();
    public List<AuditLog> GetAuditLogs() => _audits.GetAuditLogs();
    public List<DebtRecord> GetDebts() => _debts.GetDebts(_user.Id);

    public void AddCategory(string name)
    {
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _categories.AddCategory(name, tx);
        _audits.InsertAuditLog($"Category created: {name}", tx);
        tx.Commit();
    }

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

    public void DeleteDebt(DebtRecord debt)
    {
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _debts.DeleteDebt(debt.Id, tx);
        _audits.InsertAuditLog($"Debt deleted: #{debt.Id}", tx);
        tx.Commit();
    }

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

    public void ChangePin(string newPin)
    {
        _users.UpdateUserPinHash(_user.Id, _security.HashPinSha256(newPin));
    }

    private decimal GetTotalSpent(int cycleId, DateTime? upToDate = null)
    {
        var expenses = _expenses.GetExpenses(cycleId);
        return upToDate == null
            ? expenses.Sum(e => e.Amount)
            : expenses.Where(e => e.Date.Date <= upToDate.Value.Date).Sum(e => e.Amount);
    }

    private decimal GetTotalSpentOnDate(int cycleId, DateTime date)
    {
        return _expenses.GetExpenses(cycleId)
            .Where(e => e.Date.Date == date.Date)
            .Sum(e => e.Amount);
    }
}
