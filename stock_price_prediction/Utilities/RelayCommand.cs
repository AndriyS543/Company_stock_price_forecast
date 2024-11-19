using System.Windows.Input;

namespace stock_price_prediction.Utilities
{
    // RelayCommand is a custom implementation of ICommand.
    // It is used to delegate command logic to methods passed as parameters and to manage command enabling.
    class RelayCommand : ICommand
    {
        // The execute action to be performed.
        private readonly Action<object> _execute;

        // The function to determine whether the command can execute.
        private readonly Func<object, bool> _canExecute;

        // Event that is triggered when the execution state changes.
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Constructor accepting the execute action and an optional canExecute function.
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        // Method to determine if the command can execute.
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        // Method to execute the command.
        public void Execute(object parameter) => _execute(parameter);
    }
}
