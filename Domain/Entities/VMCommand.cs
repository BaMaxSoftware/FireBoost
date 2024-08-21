using System;
using System.Windows.Input;

namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class VMCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary></summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary></summary>
        public VMCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary></summary>
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        /// <summary></summary>
        public void Execute(object parameter) => _execute(parameter);
    }
}
