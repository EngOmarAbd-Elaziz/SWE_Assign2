using Masroofy.App.Models;

namespace Masroofy.App.Strategies;

/// <summary>
/// Normal budgeting strategy.
/// 
/// This strategy calculates the daily spending limit by evenly
/// distributing the remaining balance across the remaining days.
/// </summary>
public sealed class NormalStrategy : ICalculationStrategy
{
    /// <summary>
    /// Calculates a simple equal daily budget allocation.
    /// </summary>
    /// <param name="cycle">Current budget cycle data.</param>
    /// <param name="remainingDays">Number of remaining days.</param>
    /// <returns>Safe daily spending limit.</returns>
    public decimal CalculateSafeLimit(
        BudgetCycle cycle,
        int remainingDays)
    {
        // Prevent division by zero or invalid values
        if (remainingDays <= 0)
        {
            return 0m;
        }

        // Evenly distribute remaining balance
        return Math.Round(
            cycle.RemainingBalance / remainingDays,
            2);
    }
}
