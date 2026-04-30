using Masroofy.App.Models;

namespace Masroofy.App.Strategies;

public sealed class SavingStrategy : ICalculationStrategy
{
    private readonly decimal _bufferRate;

    public SavingStrategy(decimal bufferRate = 0.1m)
    {
        _bufferRate = bufferRate;
    }

    public decimal CalculateSafeLimit(BudgetCycle cycle, int remainingDays)
    {
        if (remainingDays <= 0)
        {
            return 0m;
        }

        var protectedBalance = cycle.RemainingBalance * (1m - _bufferRate);
        return Math.Round(protectedBalance / remainingDays, 2);
    }
}
