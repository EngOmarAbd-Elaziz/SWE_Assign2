using Budget_app.Data;
using BudgetApp.Controllers;
using BudgetApp.Data;
using BudgetApp.Services;
using BudgetApp.Strategies;
using BudgetApp.ViewModels;
using System.Windows.Controls;

namespace BudgetApp.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            var db = new JsonDatabase();
            var strategy = new NormalStrategy();

            var budgetService = new BudgetService(db, strategy);
            var expenseService = new ExpenseService(db, budgetService);

            var vm = new DashboardViewModel(
                new ExpenseController(expenseService),
                new BudgetController(budgetService)
            );

            DataContext = vm;
        }
    }
}