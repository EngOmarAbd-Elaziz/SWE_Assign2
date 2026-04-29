using BudgetApp.Models;
using BudgetApp.Services;

namespace BudgetApp.Controllers
{
    public class ExpenseController
    {
        private readonly ExpenseService _service;

        public ExpenseController(ExpenseService service)
        {
            _service = service;
        }

        public void AddExpense(double amount, string categoryName)
        {
            var expense = new Expense
            {
                Amount = amount,
                Category = new Category { Name = categoryName }
            };

            _service.SaveExpense(expense);
        }
    }
}