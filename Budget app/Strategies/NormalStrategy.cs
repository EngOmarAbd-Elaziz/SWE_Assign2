using System;
using BudgetApp.Interfaces;

namespace BudgetApp.Strategies.Interfaces

{

    public class NormalStrategy : ICalculationStrategy
    {
        public string StragyName => "Normal";

        public double Calculate(double Balance, int daysLeft)
        {
            return Balance / daysLeft;
        }
    }
}
