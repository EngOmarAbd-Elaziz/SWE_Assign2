using Masroofy.App.Models;

namespace Masroofy.App.Strategies;

/// <summary>
/// Defines a strategy for calculating a safe daily spending limit.
/// 
/// This interface is used with the Strategy Pattern to allow
/// different budgeting behaviors without modifying the core service logic.
/// </summary>
public interface ICalculationStrategy
{
    /// <summary>
    /// Calculates the safe daily spending limit based on the current budget cycle.
    /// </summary>
    /// <param name="cycle">
    /// The current budget cycle containing remaining balance and limits.
    /// </param>
    /// <param name="remainingDays">
    /// Number of days left in the budget cycle.
    /// </param>
    /// <returns>
    /// The calculated safe daily spending amount.
    /// </returns>
    decimal CalculateSafeLimit(
        BudgetCycle cycle,
        int remainingDays);
}
