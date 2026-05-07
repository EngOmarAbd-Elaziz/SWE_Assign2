using Masroofy.App.Models;
using Masroofy.App.Strategies;

namespace Masroofy.App.Services;

public interface IBudgetService
{
    ICalculationStrategy GetStrategy();
    void SetStrategy(ICalculationStrategy strategy);
    BudgetCycle? GetCycle();
    BudgetCycle CreateCycle(decimal totalAllowance, DateTime startDate, DateTime endDate);
    List<Expense> GetExpenses(int cycleId);
    void AddExpense(BudgetCycle cycle, decimal amount, string category, DateTime when);
    void UpdateExpense(BudgetCycle cycle, Expense original, decimal newAmount, string category);
    void DeleteExpense(BudgetCycle cycle, Expense expense);
    void ApplyRolloverIfNeeded(BudgetCycle cycle);
    decimal CalculateSafeDailyLimit(BudgetCycle cycle, DateTime? forDate = null);
    bool WasYesterdayOverspent(BudgetCycle cycle);
    bool IsAtEightyPercent(BudgetCycle cycle);
    List<string> GetCategories();
    List<AuditLog> GetAuditLogs();
    List<DebtRecord> GetDebts();
    void AddCategory(string name);
    void ExportExpensesToCsv(BudgetCycle cycle, string filePath);
    void AddDebt(decimal amount, string type, string note, bool applyToBalance = false);
    void UpdateDebt(DebtRecord debt, decimal amount, string type, string note);
    void DeleteDebt(DebtRecord debt);
    string ForecastStatus(BudgetCycle cycle);
    void ChangePin(string newPin);
}
