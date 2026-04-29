using BudgetApp.Interfaces;
using BudgetApp.Models;

namespace BudgetApp.Services
{
    public class BudgetService
    {
        private readonly IDatabase _db;
        private readonly ICalculationStrategy _strategy;
        private BudgetCycle _cycle;

        public BudgetService(IDatabase db, ICalculationStrategy strategy)
        {
            _db = db;
            _strategy = strategy;
            _cycle = _db.Load<BudgetCycle>() ?? new BudgetCycle();
        }

        public double GetDailyLimit()
        {
            return _strategy.Calculate(_cycle.RemainingBalance, 10);
        }

        public void UpdateBalance(double amount)
        {
            _cycle.RemainingBalance -= amount;
            _db.Save(_cycle);
        }
    }
}