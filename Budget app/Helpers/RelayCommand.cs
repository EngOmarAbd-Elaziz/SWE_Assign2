using System;
using System.Windows.Input;

namespace BudgetApp.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;

        public RelayCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public void Execute(object parameter) => _execute(parameter);

        public bool CanExecute(object parameter) => true;

        public event EventHandler CanExecuteChanged;
    }
}