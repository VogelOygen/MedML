using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public partial class DashboardView : UserControl
    {
        private const string CSV_PATH = "Data/heart.csv";
        private List<HeartData> _data;

        public DashboardView()
        {
            InitializeComponent();
            LoadData();
            InitializeCharts();
        }

        private void LoadData()
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CSV_PATH);
                var lines = File.ReadAllLines(fullPath).Skip(1);
                _data = new List<HeartData>();

                foreach (var line in lines)
                {
                    var values = line.Split(',');
                    if (values.Length != 12) continue;

                    _data.Add(new HeartData
                    {
                        Age = values[0],
                        Sex = values[1],
                        ChestPainType = values[2],
                        RestingBP = values[3],
                        Cholesterol = values[4],
                        FastingBS = values[5],
                        RestingECG = values[6],
                        MaxHR = values[7],
                        ExerciseAngina = values[8],
                        Oldpeak = values[9],
                        ST_Slope = values[10],
                        HeartDisease = values[11]
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void InitializeCharts()
        {
            if (_data == null || !_data.Any()) return;

            InitializeAgeChart();
            InitializeSexChart();
            InitializeChestPainChart();
            InitializeCholesterolChart();
            InitializeProbabilityChart();
        }

        private void InitializeAgeChart()
        {
            var ageGroups = _data
                .GroupBy(d => (int.Parse(d.Age) / 10) * 10)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    AgeGroup = g.Key,
                    Healthy = g.Count(p => p.HeartDisease == "0"),
                    Diseased = g.Count(p => p.HeartDisease == "1")
                })
                .ToList();

            var healthySeries = new ColumnSeries<int>
            {
                Name = "Здоровые",
                Values = ageGroups.Select(g => g.Healthy).ToArray(),
                Fill = new SolidColorPaint(SKColors.Green.WithAlpha(200))
            };

            var diseasedSeries = new ColumnSeries<int>
            {
                Name = "Больные",
                Values = ageGroups.Select(g => g.Diseased).ToArray(),
                Fill = new SolidColorPaint(SKColors.Red.WithAlpha(200))
            };

            AgeChart.Series = new ISeries[] { healthySeries, diseasedSeries };
            AgeChart.XAxes = new[] { new Axis
            {
                Labels = ageGroups.Select(g => $"{g.AgeGroup}-{g.AgeGroup + 9}").ToArray(),
                LabelsRotation = 0
            }};
            AgeChart.YAxes = new[] { new Axis
            {
                Name = "Количество пациентов"
            }};
        }

        private void InitializeSexChart()
        {
            var sexData = _data
                .GroupBy(d => d.Sex)
                .Select(g => new
                {
                    Gender = g.Key == "M" ? "Мужчины" : "Женщины",
                    Diseased = g.Count(p => p.HeartDisease == "1"),
                    Total = g.Count()
                })
                .ToList();

            var series = sexData.Select(d => new PieSeries<ObservableValue>
            {
                Name = $"{d.Gender} ({d.Diseased} из {d.Total})",
                Values = new[] { new ObservableValue((double)d.Diseased / d.Total * 100) },
                Fill = new SolidColorPaint(d.Gender == "Мужчины" ? SKColors.Blue.WithAlpha(200) : SKColors.Pink.WithAlpha(200))
            });

            SexChart.Series = series.Cast<ISeries>().ToArray();
        }

        private void InitializeChestPainChart()
        {
            var painData = _data
                .GroupBy(d => d.ChestPainType)
                .Select(g => new
                {
                    Type = g.Key,
                    Healthy = g.Count(p => p.HeartDisease == "0"),
                    Diseased = g.Count(p => p.HeartDisease == "1")
                })
                .ToList();

            var healthySeries = new StackedColumnSeries<int>
            {
                Name = "Здоровые",
                Values = painData.Select(d => d.Healthy).ToArray(),
                Fill = new SolidColorPaint(SKColors.Green.WithAlpha(200))
            };

            var diseasedSeries = new StackedColumnSeries<int>
            {
                Name = "Больные",
                Values = painData.Select(d => d.Diseased).ToArray(),
                Fill = new SolidColorPaint(SKColors.Red.WithAlpha(200))
            };

            ChestPainChart.Series = new ISeries[] { healthySeries, diseasedSeries };
            ChestPainChart.XAxes = new[] { new Axis
            {
                Labels = painData.Select(d => d.Type).ToArray(),
                LabelsRotation = 0
            }};
            ChestPainChart.YAxes = new[] { new Axis
            {
                Name = "Количество пациентов"
            }};
        }

        private void InitializeCholesterolChart()
        {
            // Группируем данные по диапазонам холестерина
            var ranges = new[]
            {
                (Min: 0, Max: 200, Name: "Нормальный", Color: SKColors.Green),
                (Min: 200, Max: 240, Name: "Пограничный", Color: SKColors.Orange),
                (Min: 240, Max: int.MaxValue, Name: "Высокий", Color: SKColors.Red)
            };

            var groupedData = _data
                .Where(d => int.TryParse(d.Cholesterol, out _))
                .GroupBy(d =>
                {
                    var chol = int.Parse(d.Cholesterol);
                    return ranges.First(r => chol >= r.Min && chol < r.Max);
                })
                .Select(g => new
                {
                    Range = g.Key,
                    Healthy = g.Count(d => d.HeartDisease == "0"),
                    Diseased = g.Count(d => d.HeartDisease == "1"),
                    Total = g.Count()
                })
                .OrderBy(x => x.Range.Min)
                .ToList();

            var healthySeries = new StackedColumnSeries<int>
            {
                Name = "Здоровые",
                Values = groupedData.Select(g => g.Healthy).ToArray(),
                Fill = new SolidColorPaint(SKColors.Green.WithAlpha(150))
            };

            var diseasedSeries = new StackedColumnSeries<int>
            {
                Name = "Больные",
                Values = groupedData.Select(g => g.Diseased).ToArray(),
                Fill = new SolidColorPaint(SKColors.Red.WithAlpha(150))
            };

            var labels = groupedData.Select(g => 
                $"{g.Range.Name}\n{g.Range.Min}-{(g.Range.Max == int.MaxValue ? "+" : g.Range.Max.ToString())}\n{g.Total} чел.").ToArray();

            CholesterolChart.Series = new ISeries[] { healthySeries, diseasedSeries };
            CholesterolChart.XAxes = new[] { new Axis
            {
                Labels = labels,
                LabelsRotation = 0
            }};
            CholesterolChart.YAxes = new[] { new Axis
            {
                Name = "Количество пациентов"
            }};
        }

        private void InitializeProbabilityChart()
        {
            // Рассчитываем общую статистику
            var totalPatients = _data.Count;
            var diseasedCount = _data.Count(d => d.HeartDisease == "1");
            var probability = (double)diseasedCount / totalPatients;

            // Создаем градиентную заливку
            var gradientPaint = new LinearGradientPaint(new[]
            {
                new SKColor(46, 204, 113), // зеленый
                new SKColor(241, 196, 15), // желтый
                new SKColor(231, 76, 60)   // красный
            });

            var series = new PieSeries<ObservableValue>
            {
                Values = new[] { new ObservableValue(probability * 100) },
                Name = "Вероятность",
                Fill = gradientPaint,
                MaxRadialColumnWidth = 50,
                DataLabelsSize = 20,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:N1}%"
            };

            var stats = new LabelVisual
            {
                Text = $"Статистика заболеваемости:\n" +
                      $"Всего пациентов: {totalPatients}\n" +
                      $"С заболеванием: {diseasedCount}\n" +
                      $"Процент: {probability:P1}",
                TextSize = 16,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.Black)
            };

            ProbabilityChart.Series = new ISeries[] { series };
            ProbabilityChart.VisualElements = new[] { stats };
        }
    }
} 