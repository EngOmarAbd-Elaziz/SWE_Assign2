using Masroofy.App.Models;

namespace Masroofy.App.Strategies;

public sealed class NormalStrategy : ICalculationStrategy
{
    public decimal CalculateSafeLimit(BudgetCycle cycle, int remainingDays)
    {
        if (remainingDays <= 0)
        {
            return 0m;
        }

        return Math.Round(cycle.RemainingBalance / remainingDays, 2);
    }
}
