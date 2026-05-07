using Masroofy.App.Models;

namespace Masroofy.App.Strategies;

/// <summary>
/// واجهة خاصة باستراتيجيات حساب الحد الآمن للمصروف اليومي.
/// 
/// تستخدم الـ Strategy Pattern للسماح بتغيير طريقة الحساب
/// بدون تعديل الكود الأساسي داخل BudgetService.
/// </summary>
public interface ICalculationStrategy
{
    /// <summary>
    /// حساب الحد الآمن اليومي للإنفاق.
    /// </summary>
    /// <param name="cycle">
    /// دورة الميزانية الحالية التي تحتوي على الرصيد المتبقي.
    /// </param>
    /// 
    /// <param name="remainingDays">
    /// عدد الأيام المتبقية في دورة الميزانية.
    /// </param>
    /// 
    /// <returns>
    /// قيمة الحد الآمن اليومي الذي يمكن للمستخدم صرفه.
    /// </returns>
    decimal CalculateSafeLimit(
        BudgetCycle cycle,
        int remainingDays);
}
