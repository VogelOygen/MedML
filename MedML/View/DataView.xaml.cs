using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using MedML.Models;

namespace MedML.View
{
    public partial class DataView : UserControl
    {
        private const string CSV_PATH = "Data/heart.csv";
        private List<HeartData> _data = new List<HeartData>();

        private readonly Dictionary<string, string> _chestPainTypes = new Dictionary<string, string>
        {
            {"ATA", "Типичная стенокардия"},
            {"NAP", "Нетипичная стенокардия"},
            {"ASY", "Асимптоматическая"},
            {"TA", "Нестенокардическая боль"}
        };

        private readonly Dictionary<string, string> _restingECG = new Dictionary<string, string>
        {
            {"Normal", "Нормальная"},
            {"ST", "Отклонение ST-T"},
            {"LVH", "Гипертрофия ЛЖ"}
        };

        private readonly Dictionary<string, string> _slope = new Dictionary<string, string>
        {
            {"Up", "Восходящий"},
            {"Flat", "Плоский"},
            {"Down", "Нисходящий"}
        };

        public DataView()
        {
            InitializeComponent();
            LoadCsvData();
        }

        private void LoadCsvData()
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CSV_PATH);
                Debug.WriteLine($"Пытаемся загрузить файл: {fullPath}");

                if (!File.Exists(fullPath))
                {
                    string message = $"Файл CSV не найден по пути: {fullPath}";
                    Debug.WriteLine(message);
                    MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var lines = File.ReadAllLines(fullPath);
                Debug.WriteLine($"Прочитано строк: {lines.Length}");

                if (lines.Length == 0)
                {
                    Debug.WriteLine("Файл пуст");
                    MessageBox.Show("CSV файл пуст", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _data = new List<HeartData>();

                // Пропускаем заголовок
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    if (values.Length != 12)
                    {
                        Debug.WriteLine($"Неверное количество столбцов в строке {i}: {values.Length}");
                        continue;
                    }

                    var heartData = new HeartData
                    {
                        Age = values[0],
                        Sex = values[1] == "M" ? "Мужской" : "Женский",
                        ChestPainType = _chestPainTypes.ContainsKey(values[2]) ? _chestPainTypes[values[2]] : values[2],
                        RestingBP = values[3],
                        Cholesterol = values[4],
                        FastingBS = values[5] == "1" ? "> 120 мг/дл" : "≤ 120 мг/дл",
                        RestingECG = _restingECG.ContainsKey(values[6]) ? _restingECG[values[6]] : values[6],
                        MaxHR = values[7],
                        ExerciseAngina = values[8] == "Y" ? "Да" : "Нет",
                        Oldpeak = values[9],
                        ST_Slope = _slope.ContainsKey(values[10]) ? _slope[values[10]] : values[10],
                        HeartDisease = values[11] == "1" ? "Есть" : "Нет"
                    };

                    _data.Add(heartData);
                }

                Debug.WriteLine($"Загружено записей: {_data.Count}");
                DataGrid.ItemsSource = null; // Сбрасываем источник данных
                DataGrid.ItemsSource = _data; // Устанавливаем новый источник данных
            }
            catch (Exception ex)
            {
                string message = $"Ошибка при загрузке CSV файла: {ex.Message}\nStackTrace: {ex.StackTrace}";
                Debug.WriteLine(message);
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCsvData();
        }
    }
} 