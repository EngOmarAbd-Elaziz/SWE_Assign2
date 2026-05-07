using Masroofy.App.Models;

namespace Masroofy.App.Strategies;

/// <summary>
/// استراتيجية الحساب العادي للحد اليومي للمصروف.
/// 
/// تقوم بتقسيم الرصيد المتبقي على عدد الأيام المتبقية بشكل مباشر.
/// </summary>
public sealed class NormalStrategy : ICalculationStrategy
{
    /// <summary>
    /// حساب الحد اليومي بطريقة بسيطة (قسمة متساوية).
    /// </summary>
    /// <param name="cycle">دورة الميزانية الحالية.</param>
    /// <param name="remainingDays">عدد الأيام المتبقية.</param>
    /// <returns>قيمة الحد اليومي الآمن.</returns>
    public decimal CalculateSafeLimit(
        BudgetCycle cycle,
        int remainingDays)
    {
        // حماية من القسمة على صفر أو أيام سالبة
        if (remainingDays <= 0)
        {
            return 0m;
        }

        // تقسيم الرصيد المتبقي على الأيام المتبقية
        return Math.Round(
            cycle.RemainingBalance / remainingDays,
            2);
    }
}
