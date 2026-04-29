using Budget_app.Controllers;
using Budget_app.Data;
using Budget_app.Services;
using Budget_app.Strategies;
using BudgetApp.Controllers;
using BudgetApp.Data;
using BudgetApp.Services;
using BudgetApp.Strategies;
using System.Windows;

namespace BudgetApp
{
    public partial class MainWindow : Window
    {
        private ExpenseController _expenseController;
        private BudgetController _budgetController;

        public MainWindow()
        {
            InitializeComponent();

            var db = new JsonDatabase();
            var strategy = new NormalStrategy();

            var budgetService = new BudgetService(db, strategy);
            var expenseService = new ExpenseService(db, budgetService);

            _expenseController = new ExpenseController(expenseService);
            _budgetController = new BudgetController(budgetService);
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            double amount;
            if (!double.TryParse(AmountBox.Text, out amount))
            {
                MessageBox.Show("Invalid amount");
                return;
            }

            _expenseController.AddExpense(amount, CategoryBox.Text);
        }

        private void ShowLimit_Click(object sender, RoutedEventArgs e)
        {
            var limit = _budgetController.GetDailyLimit();
            MessageBox.Show($"Daily Limit: {limit}");
        }
    }
}