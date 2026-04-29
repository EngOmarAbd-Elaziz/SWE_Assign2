using BudgetApp.Interfaces;

namespace BudgetApp.Strategies.Interfaces
{
    public class SavingStrategy : ICalculationStrategy
    {
        public string StragyName => "Saving";

        public double Calculate(double Balance, int daysLeft)
        {
            return (Balance / daysLeft) * 0.20;
        }
    }
}
