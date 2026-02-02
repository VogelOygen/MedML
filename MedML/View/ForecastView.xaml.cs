using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using MedML.Models;

namespace MedML.View
{
    public partial class ForecastView : UserControl
    {
        private readonly Random _random = new Random();
        private readonly Dictionary<string, (double Min, double Max)> _numericRanges;
        private readonly Dictionary<string, string[]> _categoricalValues;

        public ForecastView()
        {
            InitializeComponent();

            // Инициализация словарей в конструкторе
            _numericRanges = new Dictionary<string, (double Min, double Max)>
            {
                { "Age", (29, 77) },
                { "RestingBP", (94, 200) },
                { "Cholesterol", (126, 564) },
                { "MaxHR", (71, 202) },
                { "Oldpeak", (0, 6.2) }
            };

            _categoricalValues = new Dictionary<string, string[]>
            {
                { "Sex", new[] { "M", "F" } },
                { "ChestPainType", new[] { "ATA", "NAP", "ASY", "TA" } },
                { "FastingBS", new[] { "0", "1" } },
                { "RestingECG", new[] { "Normal", "ST", "LVH" } },
                { "ExerciseAngina", new[] { "N", "Y" } },
                { "ST_Slope", new[] { "Up", "Flat", "Down" } }
            };

            SetupProbabilityChart();
        }

        private void SetupProbabilityChart()
        {
            // Настраиваем пустой график вероятности
            UpdateProbabilityChart(0);
        }

        private void GenerateTestDataButton_Click(object sender, RoutedEventArgs e)
        {
            // Генерируем числовые значения
            AgeTextBox.Text = GenerateNumericValue("Age").ToString("F0");
            RestingBPTextBox.Text = GenerateNumericValue("RestingBP").ToString("F0");
            CholesterolTextBox.Text = GenerateNumericValue("Cholesterol").ToString("F0");
            MaxHRTextBox.Text = GenerateNumericValue("MaxHR").ToString("F0");
            OldpeakTextBox.Text = GenerateNumericValue("Oldpeak").ToString("F1");

            // Генерируем категориальные значения
            SexComboBox.SelectedIndex = _random.Next(2);
            ChestPainComboBox.SelectedIndex = _random.Next(4);
            FastingBSComboBox.SelectedIndex = _random.Next(2);
            RestingECGComboBox.SelectedIndex = _random.Next(3);
            ExerciseAnginaComboBox.SelectedIndex = _random.Next(2);
            STSlopeComboBox.SelectedIndex = _random.Next(3);
        }

        private double GenerateNumericValue(string feature)
        {
            var range = _numericRanges[feature];
            return _random.NextDouble() * (range.Max - range.Min) + range.Min;
        }

        private void CalculateProbabilityButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, что все поля заполнены
                if (!ValidateInputs()) return;

                // Собираем данные из формы
                var data = new HeartData
                {
                    Age = AgeTextBox.Text,
                    Sex = (SexComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Мужской" ? "M" : "F",
                    ChestPainType = GetChestPainTypeCode((ChestPainComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? ""),
                    RestingBP = RestingBPTextBox.Text,
                    Cholesterol = CholesterolTextBox.Text,
                    FastingBS = (FastingBSComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.StartsWith(">") == true ? "1" : "0",
                    RestingECG = GetRestingECGCode((RestingECGComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? ""),
                    MaxHR = MaxHRTextBox.Text,
                    ExerciseAngina = (ExerciseAnginaComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Да" ? "Y" : "N",
                    Oldpeak = OldpeakTextBox.Text,
                    ST_Slope = GetSTSlopeCode((STSlopeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "")
                };

                // TODO: Здесь будет вызов ML модели
                // Пока используем случайное значение для демонстрации
                double probability = CalculateProbability(data);

                // Обновляем визуализацию
                UpdateProbabilityChart(probability);
                UpdateAdditionalInfo(data, probability);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете вероятности: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            // Проверяем числовые поля
            if (!ValidateNumericField(AgeTextBox, "Age", "Возраст") ||
                !ValidateNumericField(RestingBPTextBox, "RestingBP", "Давление в покое") ||
                !ValidateNumericField(CholesterolTextBox, "Cholesterol", "Холестерин") ||
                !ValidateNumericField(MaxHRTextBox, "MaxHR", "Максимальный пульс") ||
                !ValidateNumericField(OldpeakTextBox, "Oldpeak", "Депрессия ST"))
                return false;

            // Проверяем выпадающие списки
            if (!ValidateComboBox(SexComboBox, "Пол") ||
                !ValidateComboBox(ChestPainComboBox, "Тип боли в груди") ||
                !ValidateComboBox(FastingBSComboBox, "Уровень сахара") ||
                !ValidateComboBox(RestingECGComboBox, "ЭКГ в покое") ||
                !ValidateComboBox(ExerciseAnginaComboBox, "Стенокардия") ||
                !ValidateComboBox(STSlopeComboBox, "Наклон ST"))
                return false;

            return true;
        }

        private bool ValidateNumericField(TextBox textBox, string feature, string displayName)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show($"Пожалуйста, заполните поле '{displayName}'", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Focus();
                return false;
            }

            if (!double.TryParse(textBox.Text, out double value))
            {
                MessageBox.Show($"Поле '{displayName}' должно содержать число", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Focus();
                return false;
            }

            var range = _numericRanges[feature];
            if (value < range.Min || value > range.Max)
            {
                MessageBox.Show($"Значение в поле '{displayName}' должно быть между {range.Min} и {range.Max}", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                textBox.Focus();
                return false;
            }

            return true;
        }

        private bool ValidateComboBox(ComboBox comboBox, string displayName)
        {
            if (comboBox.SelectedItem == null)
            {
                MessageBox.Show($"Пожалуйста, выберите значение для поля '{displayName}'", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                comboBox.Focus();
                return false;
            }
            return true;
        }

        private void UpdateProbabilityChart(double probability)
        {
            var value = new ObservableValue(probability * 100);
            var series = new PieSeries<ObservableValue>
            {
                Values = new[] { value },
                Name = "Вероятность",
                Fill = new LinearGradientPaint(new[]
                {
                    new SKColor(46, 204, 113),  // зеленый
                    new SKColor(241, 196, 15),  // желтый
                    new SKColor(231, 76, 60)    // красный
                }),
                MaxRadialColumnWidth = 50,
                DataLabelsSize = 20,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:N1}%"
            };

            ProbabilityChart.Series = new ISeries[] { series };
        }

        private void UpdateAdditionalInfo(HeartData data, double probability)
        {
            string riskLevel = probability switch
            {
                < 0.3 => "Низкий риск",
                < 0.6 => "Средний риск",
                _ => "Высокий риск"
            };

            string recommendations = probability switch
            {
                < 0.3 => "• Продолжайте вести здоровый образ жизни\n• Регулярные профилактические осмотры",
                < 0.6 => "• Рекомендуется консультация кардиолога\n• Контроль факторов риска\n• Регулярный мониторинг состояния",
                _ => "• Срочная консультация кардиолога\n• Дополнительное обследование\n• Строгий контроль факторов риска"
            };

            AdditionalInfoTextBlock.Text = $"Уровень риска: {riskLevel}\n\n" +
                                         $"Рекомендации:\n{recommendations}\n\n" +
                                         $"Основные факторы риска:\n" +
                                         $"• Возраст: {data.Age} лет\n" +
                                         $"• Холестерин: {data.Cholesterol} мг/дл\n" +
                                         $"• Макс. пульс: {data.MaxHR}";
        }

        private string GetChestPainTypeCode(string displayName) => displayName switch
        {
            "Типичная стенокардия" => "ATA",
            "Нетипичная стенокардия" => "NAP",
            "Асимптоматическая" => "ASY",
            "Нестенокардическая боль" => "TA",
            _ => throw new ArgumentException($"Неизвестный тип боли: {displayName}")
        };

        private string GetRestingECGCode(string displayName) => displayName switch
        {
            "Нормальная" => "Normal",
            "Отклонение ST-T" => "ST",
            "Гипертрофия ЛЖ" => "LVH",
            _ => throw new ArgumentException($"Неизвестный тип ЭКГ: {displayName}")
        };

        private string GetSTSlopeCode(string displayName) => displayName switch
        {
            "Восходящий" => "Up",
            "Плоский" => "Flat",
            "Нисходящий" => "Down",
            _ => throw new ArgumentException($"Неизвестный наклон ST: {displayName}")
        };

        // Временная функция для демонстрации, будет заменена на реальную ML модель
        private double CalculateProbability(HeartData data)
        {
            // TODO: Заменить на реальную ML модель
            // Пока возвращаем случайное значение с небольшим влиянием входных данных
            double baseProbability = _random.NextDouble() * 0.5; // Базовая случайность
            
            // Добавляем влияние возраста
            double ageEffect = (double.Parse(data.Age) - 29) / (77 - 29) * 0.2;
            
            // Добавляем влияние типа боли
            double painEffect = data.ChestPainType == "ASY" ? 0.2 : 0.1;
            
            // Добавляем влияние стенокардии
            double anginaEffect = data.ExerciseAngina == "Y" ? 0.1 : 0;

            return Math.Min(Math.Max(baseProbability + ageEffect + painEffect + anginaEffect, 0), 1);
        }
    }
} 