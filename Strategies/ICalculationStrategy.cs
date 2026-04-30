using Masroofy.App.Models;

namespace Masroofy.App.Strategies;

public interface ICalculationStrategy
{
    decimal CalculateSafeLimit(BudgetCycle cycle, int remainingDays);
}
