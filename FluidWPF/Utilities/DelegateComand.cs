using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FluidWPF.Utilities
{
    public class DelegateCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;

        }
        public bool CanExecute(object parameter)
        {
            return canExecute?.Invoke(parameter)??true;
            return this.canExecute == null || this.canExecute(parameter ?? throw new Exception());
        }
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

    }
}
