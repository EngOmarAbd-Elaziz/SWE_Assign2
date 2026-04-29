namespace BudgetApp.Interfaces

{
    public interface ICalculationStrategy
    {
        string StragyName { get; }
        double Calculate(double balance, int days);
    }
}
﻿
