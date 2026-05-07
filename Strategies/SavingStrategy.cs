using Masroofy.App.Models;

namespace Masroofy.App.Strategies;

/// <summary>
/// Saving strategy that preserves a portion of the balance as a buffer.
/// 
/// This strategy reduces the available budget by a configurable percentage
/// before distributing it across the remaining days.
/// </summary>
public sealed class SavingStrategy : ICalculationStrategy
{
    /// <summary>
    /// Percentage of the balance reserved as savings (buffer).
    /// Example: 0.1 = 10% reserved and not included in daily spending.
    /// </summary>
    private readonly decimal _bufferRate;

    /// <summary>
    /// Initializes a new instance of the SavingStrategy class.
    /// </summary>
    /// <param name="bufferRate">
    /// The reserved percentage of the balance (default is 10%).
    /// </param>
    public SavingStrategy(decimal bufferRate = 0.1m)
    {
        _bufferRate = bufferRate;
    }

    /// <summary>
    /// Calculates a conservative daily spending limit by reserving part of the balance.
    /// </summary>
    /// <param name="cycle">Current budget cycle data.</param>
    /// <param name="remainingDays">Number of remaining days.</param>
    /// <returns>Safe daily spending limit with savings buffer applied.</returns>
    public decimal CalculateSafeLimit(
        BudgetCycle cycle,
        int remainingDays)
    {
        // Prevent invalid calculations
        if (remainingDays <= 0)
        {
            return 0m;
        }

        // Reserve part of the balance as savings buffer
        var protectedBalance =
            cycle.RemainingBalance * (1m - _bufferRate);

        // Distribute remaining safe balance evenly
        return Math.Round(
            protectedBalance / remainingDays,
            2);
    }
}
