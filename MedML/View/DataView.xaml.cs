using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using Microsoft.Win32;
using MedML.Models;
using MedML.Data;
using System.Text;

namespace MedML.View
{
    public partial class DataView : UserControl
    {
        private const string CSV_PATH = "Data/heart.csv";
        private List<HeartData> _data = new List<HeartData>();
        private List<HeartDiseaseRecord> _rawRecords = new List<HeartDiseaseRecord>();

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
            LoadData();
        }

        private void LoadData()
        {
            if (DbSourceCheckBox != null && DbSourceCheckBox.IsChecked == true)
            {
                if (ExportButton != null) ExportButton.Visibility = Visibility.Visible;
                if (CrudButtons != null) CrudButtons.Visibility = Visibility.Visible;
                LoadDbData();
            }
            else
            {
                if (ExportButton != null) ExportButton.Visibility = Visibility.Collapsed;
                if (CrudButtons != null) CrudButtons.Visibility = Visibility.Collapsed;
                LoadCsvData();
            }
        }

        private void LoadDbData()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    _rawRecords = context.HeartDiseaseRecords.OrderBy(r => r.Id).ToList();
                    UpdateViewFromRawRecords();
                    Debug.WriteLine($"Загружено записей из БД: {_data.Count}");
                }
            }
            catch (Exception ex)
            {
                string message = $"Ошибка при загрузке из БД: {ex.Message}\nStackTrace: {ex.StackTrace}";
                Debug.WriteLine(message);
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCsvData()
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CSV_PATH);
                Debug.WriteLine($"Пытаемся загрузить файл: {fullPath}");

                if (!File.Exists(fullPath))
                {
                    // Try to find in project structure if running in debug
                     fullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\heart.csv"));
                }

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

                _rawRecords = new List<HeartDiseaseRecord>();

                // Пропускаем заголовок
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    if (values.Length != 12)
                    {
                        Debug.WriteLine($"Неверное количество столбцов в строке {i}: {values.Length}");
                        continue;
                    }

                    try 
                    {
                        var record = new HeartDiseaseRecord
                        {
                            // Id is not set for CSV records, will be default (0)
                            Age = int.Parse(values[0]),
                            Sex = values[1],
                            ChestPainType = values[2],
                            RestingBP = int.Parse(values[3]),
                            Cholesterol = int.Parse(values[4]),
                            FastingBS = int.Parse(values[5]),
                            RestingECG = values[6],
                            MaxHR = int.Parse(values[7]),
                            ExerciseAngina = values[8],
                            Oldpeak = float.Parse(values[9], System.Globalization.CultureInfo.InvariantCulture),
                            ST_Slope = values[10],
                            HeartDisease = int.Parse(values[11])
                        };
                        _rawRecords.Add(record);
                    }
                    catch (Exception ex)
                    {
                         Debug.WriteLine($"Ошибка парсинга строки {i}: {ex.Message}");
                    }
                }

                UpdateViewFromRawRecords();
                Debug.WriteLine($"Загружено записей: {_data.Count}");
            }
            catch (Exception ex)
            {
                string message = $"Ошибка при загрузке CSV файла: {ex.Message}\nStackTrace: {ex.StackTrace}";
                Debug.WriteLine(message);
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateViewFromRawRecords()
        {
            _data = _rawRecords.Select(r => new HeartData
            {
                Id = r.Id,
                Age = r.Age.ToString(),
                Sex = r.Sex == "M" ? "Мужской" : "Женский",
                ChestPainType = _chestPainTypes.ContainsKey(r.ChestPainType) ? _chestPainTypes[r.ChestPainType] : r.ChestPainType,
                RestingBP = r.RestingBP.ToString(),
                Cholesterol = r.Cholesterol.ToString(),
                FastingBS = r.FastingBS == 1 ? "> 120 мг/дл" : "≤ 120 мг/дл",
                RestingECG = _restingECG.ContainsKey(r.RestingECG) ? _restingECG[r.RestingECG] : r.RestingECG,
                MaxHR = r.MaxHR.ToString(),
                ExerciseAngina = r.ExerciseAngina == "Y" ? "Да" : "Нет",
                Oldpeak = r.Oldpeak.ToString(),
                ST_Slope = _slope.ContainsKey(r.ST_Slope) ? _slope[r.ST_Slope] : r.ST_Slope,
                HeartDisease = r.HeartDisease == 1 ? "Есть" : "Нет"
            }).ToList();

            DataGrid.ItemsSource = null;
            DataGrid.ItemsSource = _data;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void OnDataSourceChanged(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_rawRecords == null || !_rawRecords.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = "heart_export.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    // Заголовок
                    sb.AppendLine("Age,Sex,ChestPainType,RestingBP,Cholesterol,FastingBS,RestingECG,MaxHR,ExerciseAngina,Oldpeak,ST_Slope,HeartDisease");

                    foreach (var record in _rawRecords)
                    {
                        sb.AppendLine($"{record.Age},{record.Sex},{record.ChestPainType},{record.RestingBP},{record.Cholesterol},{record.FastingBS},{record.RestingECG},{record.MaxHR},{record.ExerciseAngina},{record.Oldpeak.ToString(System.Globalization.CultureInfo.InvariantCulture)},{record.ST_Slope},{record.HeartDisease}");
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                    MessageBox.Show("Данные успешно экспортированы", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editor = new RecordEditorWindow();
            if (editor.ShowDialog() == true)
            {
                try
                {
                    using (var context = new AppDbContext())
                    {
                        context.HeartDiseaseRecords.Add(editor.Record);
                        context.SaveChanges();
                    }
                    LoadDbData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataGrid.SelectedItem is HeartData selectedData)
            {
                // Find original record in _rawRecords by ID. If ID is 0 (CSV), this logic might be tricky but we only enable buttons for DB.
                // Actually for DB, ID should be > 0.
                
                var recordToEdit = _rawRecords.FirstOrDefault(r => r.Id == selectedData.Id);
                
                if (recordToEdit == null) 
                {
                     MessageBox.Show("Не удалось найти запись", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                     return;
                }

                var editor = new RecordEditorWindow(recordToEdit);
                if (editor.ShowDialog() == true)
                {
                    try
                    {
                        using (var context = new AppDbContext())
                        {
                            var dbRecord = context.HeartDiseaseRecords.Find(recordToEdit.Id);
                            if (dbRecord != null)
                            {
                                // Copy properties
                                dbRecord.Age = editor.Record.Age;
                                dbRecord.Sex = editor.Record.Sex;
                                dbRecord.ChestPainType = editor.Record.ChestPainType;
                                dbRecord.RestingBP = editor.Record.RestingBP;
                                dbRecord.Cholesterol = editor.Record.Cholesterol;
                                dbRecord.FastingBS = editor.Record.FastingBS;
                                dbRecord.RestingECG = editor.Record.RestingECG;
                                dbRecord.MaxHR = editor.Record.MaxHR;
                                dbRecord.ExerciseAngina = editor.Record.ExerciseAngina;
                                dbRecord.Oldpeak = editor.Record.Oldpeak;
                                dbRecord.ST_Slope = editor.Record.ST_Slope;
                                dbRecord.HeartDisease = editor.Record.HeartDisease;
                                
                                context.SaveChanges();
                            }
                        }
                        LoadDbData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                 MessageBox.Show("Выберите запись для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
             if (DataGrid.SelectedItem is HeartData selectedData)
             {
                 if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                 {
                     try
                     {
                         using (var context = new AppDbContext())
                         {
                             var dbRecord = context.HeartDiseaseRecords.Find(selectedData.Id);
                             if (dbRecord != null)
                             {
                                 context.HeartDiseaseRecords.Remove(dbRecord);
                                 context.SaveChanges();
                             }
                         }
                         LoadDbData();
                     }
                     catch (Exception ex)
                     {
                         MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                     }
                 }
             }
             else
             {
                  MessageBox.Show("Выберите запись для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
             }
        }
    }
}
