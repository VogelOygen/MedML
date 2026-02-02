using System;
using System.Windows.Input;

namespace MedML.ViewModel
{
    /// <summary>
    /// Реализация команды для шаблона MVVM с поддержкой CommandManager.RequerySuggested
    /// </summary>
    public class RelayCommand : ICommand, IDisposable
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Создает новый экземпляр команды RelayCommand
        /// </summary>
        /// <param name="execute">Действие, выполняемое при вызове команды</param>
        /// <param name="canExecute">Функция, определяющая, может ли команда выполняться</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            
            // Подписываемся на событие RequerySuggested для автоматического обновления состояния команды
            CommandManager.RequerySuggested += CommandManager_RequerySuggested;
        }

        /// <summary>
        /// Обработчик события RequerySuggested для обновления состояния команды
        /// </summary>
        private void CommandManager_RequerySuggested(object sender, EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Определяет, может ли команда выполняться в текущем состоянии
        /// </summary>
        /// <param name="parameter">Параметр команды</param>
        /// <returns>True, если команда может быть выполнена; иначе false</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Выполняет команду
        /// </summary>
        /// <param name="parameter">Параметр команды</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Событие, возникающее при изменении возможности выполнения команды
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Вызывает событие CanExecuteChanged для обновления состояния команды
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Освобождает ресурсы, используемые командой
        /// </summary>
        public void Dispose()
        {
            // Отписываемся от события RequerySuggested при уничтожении объекта
            CommandManager.RequerySuggested -= CommandManager_RequerySuggested;
        }
    }
}