using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.ML;
using System.Windows;

namespace MedML.ViewModel
{
    /// <summary>
    /// ViewModel для окна обучения модели
    /// </summary>
    public class LearnModelViewModel : INotifyPropertyChanged
    {
        #region Свойства

        private string _statusMessage = "Готов к обучению";
        /// <summary>
        /// Сообщение о статусе обучения
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        private double _progress;
        /// <summary>
        /// Прогресс обучения (0-100)
        /// </summary>
        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        private bool _isLearning;
        /// <summary>
        /// Флаг, указывающий идет ли процесс обучения
        /// </summary>
        public bool IsLearning
        {
            get => _isLearning;
            set
            {
                if (_isLearning != value)
                {
                    _isLearning = value;
                    OnPropertyChanged(nameof(IsLearning));
                    // Обновляем доступность команды
                    (LearnModelCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Команды

        /// <summary>
        /// Команда для запуска обучения модели
        /// </summary>
        public ICommand LearnModelCommand { get; private set; }

        #endregion

        public LearnModelViewModel()
        {
            // Инициализация команд
            LearnModelCommand = new RelayCommand(ExecuteLearnModel, CanExecuteLearnModel);
        }

        #region Методы команд

        /// <summary>
        /// Проверка возможности выполнения команды обучения
        /// </summary>
        private bool CanExecuteLearnModel(object parameter)
        {
            // Можно обучать только если не идет процесс обучения
            return !IsLearning;
        }

        /// <summary>
        /// Выполнение команды обучения модели
        /// </summary>
        private async void ExecuteLearnModel(object parameter)
        {
            try
            {
                IsLearning = true;
                StatusMessage = "Начало обучения модели...";
                Progress = 0;

                // Запускаем обучение асинхронно
                await Task.Run(() => LearnModel());

                StatusMessage = "Обучение завершено успешно!";
                Progress = 100;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при обучении: {ex.Message}";
                MessageBox.Show($"Произошла ошибка при обучении модели: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLearning = false;
            }
        }

        #endregion

        #region Методы обучения

        /// <summary>
        /// Метод обучения модели с использованием ML.NET API
        /// </summary>
        private void LearnModel()
        {
            // Путь для сохранения обученной модели
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModelMed.mlnet");
            
            // Обновляем статус
            StatusMessage = "Загрузка данных...";
            Progress = 10;
            
            // Используем API ML.NET для обучения модели
            // Вызываем метод Train из класса MLModelMed с параметром по умолчанию для пути к данным
            // Путь к данным будет автоматически определен внутри метода Train
            MLModelMed.Train(modelPath);
            
            // Обновляем прогресс
            Progress = 90;
            StatusMessage = "Сохранение модели...";
            
            // Модель сохраняется автоматически в методе Train
            Progress = 100;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}