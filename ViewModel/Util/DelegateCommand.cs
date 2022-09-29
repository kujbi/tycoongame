using System;
using System.Windows.Input;
using Model.Util;

namespace ViewModel.Util
{
    /// <summary>
    /// Delegált parancs osztály
    /// </summary>
    public class DelegateCommand : ICommand
    {

        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;
        
        /// <summary>
        /// Inicializál egy új parancsot execute akcióval
        /// </summary>
        /// <param name="execute">az akció</param>
        public DelegateCommand(Action<object?> execute) : this((Func<object?, bool>?) null, execute) { }

        /// <summary>
        /// Inicializál egy új parancsot az execute akcióval, ami akkor futhat le ha a canExecute State-ben true érték van.
        /// </summary>
        /// <param name="canExecute">a State</param>
        /// <param name="execute">az akció</param>
        public DelegateCommand(State<bool> canExecute, Action<object?> execute)
        {
            _canExecute = _ => canExecute.Value;
            canExecute.ValueChanged += (_, _) => RaiseCanExecuteChanged();
            _execute = execute;
        }
        
        /// <summary>
        /// Inicializál egy új parancsot az execute akcióval, ami akkor futhat le ha a canExecute true-t ad vissza
        /// </summary>
        /// <param name="canExecute">egy függvény, akkor futhat le a parancs ha ez true-t ad vissza</param>
        /// <param name="execute">a parancs akciója</param>
        public DelegateCommand(Func<object?, bool>? canExecute, Action<object?> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Le kell futtatni, ha a CanExecute megváltozik
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Megmondja, hogy az Execute lefuthat-e
        /// </summary>
        /// <param name="parameter">egy paraméter</param>
        /// <returns>igazat, ha lefuthat az Execute</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;  //_canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Ez az akció aminek le kell futni a parancs hívásakor
        /// </summary>
        /// <param name="parameter">egy paraméter</param>
        public void Execute(object? parameter)
        {
            if(CanExecute(parameter)) _execute(parameter);
        }

        /// <summary>
        /// Meghívja a CanExecuteChanged eseményt
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
