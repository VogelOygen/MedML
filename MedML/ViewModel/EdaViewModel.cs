using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using MedML.Models;

namespace MedML.ViewModel
{
    public class EdaViewModel : INotifyPropertyChanged
    {
        private const string CSV_PATH = "Data/heart.csv";
        private List<HeartData> _rawData = new List<HeartData>();

        // Filters
        public ObservableCollection<string> SexOptions { get; } = new ObservableCollection<string> { "Все", "M", "F" };
        public ObservableCollection<string> DiseaseOptions { get; } = new ObservableCollection<string> { "Все", "Здоровые", "Больные" };

        private string _selectedSex = "Все";
        public string SelectedSex
        {
            get => _selectedSex;
            set
            {
                if (_selectedSex != value)
                {
                    _selectedSex = value;
                    OnPropertyChanged();
                    UpdateCharts();
                }
            }
        }

        private string _selectedDiseaseStatus = "Все";
        public string SelectedDiseaseStatus
        {
            get => _selectedDiseaseStatus;
            set
            {
                if (_selectedDiseaseStatus != value)
                {
                    _selectedDiseaseStatus = value;
                    OnPropertyChanged();
                    UpdateCharts();
                }
            }
        }

        private int _minAge = 0;
        public int MinAge
        {
            get => _minAge;
            set
            {
                if (_minAge != value)
                {
                    _minAge = value;
                    OnPropertyChanged();
                    UpdateCharts();
                }
            }
        }

        private int _maxAge = 100;
        public int MaxAge
        {
            get => _maxAge;
            set
            {
                if (_maxAge != value)
                {
                    _maxAge = value;
                    OnPropertyChanged();
                    UpdateCharts();
                }
            }
        }

        // Chart 1: Scatter Plot (Age vs MaxHR)
        private ISeries[] _scatterSeries = Array.Empty<ISeries>();
        public ISeries[] ScatterSeries 
        { 
            get => _scatterSeries; 
            set { _scatterSeries = value; OnPropertyChanged(); } 
        }
        public Axis[] ScatterXAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] ScatterYAxes { get; set; } = Array.Empty<Axis>();

        // Chart 2: Cholesterol Distribution (Histogram-like)
        private ISeries[] _cholesterolSeries = Array.Empty<ISeries>();
        public ISeries[] CholesterolSeries 
        { 
            get => _cholesterolSeries; 
            set { _cholesterolSeries = value; OnPropertyChanged(); } 
        }
        public Axis[] CholesterolXAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] CholesterolYAxes { get; set; } = Array.Empty<Axis>();

        // Chart 3: RestingBP by ChestPainType (Column Chart)
        private ISeries[] _bpByPainSeries = Array.Empty<ISeries>();
        public ISeries[] BpByPainSeries 
        { 
            get => _bpByPainSeries; 
            set { _bpByPainSeries = value; OnPropertyChanged(); } 
        }
        public Axis[] BpByPainXAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] BpByPainYAxes { get; set; } = Array.Empty<Axis>();

        public EdaViewModel()
        {
            LoadData();
            
            // Initialize Axes once
            ScatterXAxes = new[] { new Axis { Name = "Возраст" } };
            ScatterYAxes = new[] { new Axis { Name = "Макс. пульс (MaxHR)" } };
            
            CholesterolYAxes = new[] { new Axis { Name = "Количество" } };
            
            BpByPainYAxes = new[] { new Axis { Name = "Среднее артериальное давление (RestingBP)" } };

            UpdateCharts();
        }

        private void LoadData()
        {
            _rawData = new List<HeartData>();
            if (!File.Exists(CSV_PATH)) return;

            var lines = File.ReadAllLines(CSV_PATH);
            foreach (var line in lines.Skip(1))
            {
                var values = line.Split(',');
                if (values.Length < 12) continue;

                _rawData.Add(new HeartData
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

            // Set default age range based on data
            if (_rawData.Any())
            {
                var ages = _rawData.Select(x => int.TryParse(x.Age, out int a) ? a : 0).Where(a => a > 0).ToList();
                if (ages.Any())
                {
                    MinAge = ages.Min();
                    MaxAge = ages.Max();
                }
            }
        }

        private IEnumerable<HeartData> GetFilteredData()
        {
            if (_rawData == null) return Enumerable.Empty<HeartData>();

            var query = _rawData.AsEnumerable();

            if (SelectedSex != "Все")
            {
                query = query.Where(x => x.Sex == SelectedSex);
            }

            if (SelectedDiseaseStatus != "Все")
            {
                string target = SelectedDiseaseStatus == "Здоровые" ? "0" : "1";
                query = query.Where(x => x.HeartDisease == target);
            }

            query = query.Where(x => 
                int.TryParse(x.Age, out int age) && 
                age >= MinAge && 
                age <= MaxAge);

            return query;
        }

        private void UpdateCharts()
        {
            var filteredData = GetFilteredData().ToList();
            if (!filteredData.Any())
            {
                // Clear charts if no data matches
                ScatterSeries = Array.Empty<ISeries>();
                CholesterolSeries = Array.Empty<ISeries>();
                BpByPainSeries = Array.Empty<ISeries>();
                return;
            }

            UpdateScatterPlot(filteredData);
            UpdateCholesterolDist(filteredData);
            UpdateBpByPain(filteredData);
        }

        private void UpdateScatterPlot(List<HeartData> data)
        {
            var healthyPoints = new List<ObservablePoint>();
            var diseasedPoints = new List<ObservablePoint>();

            foreach (var item in data)
            {
                if (double.TryParse(item.Age, out double age) && 
                    double.TryParse(item.MaxHR, out double maxHr))
                {
                    var point = new ObservablePoint(age, maxHr);
                    if (item.HeartDisease == "0")
                        healthyPoints.Add(point);
                    else
                        diseasedPoints.Add(point);
                }
            }

            var seriesList = new List<ISeries>();

            if (healthyPoints.Any())
            {
                seriesList.Add(new ScatterSeries<ObservablePoint>
                {
                    Name = "Здоровые",
                    Values = healthyPoints,
                    Fill = new SolidColorPaint(SKColors.Green.WithAlpha(150)),
                    GeometrySize = 10
                });
            }

            if (diseasedPoints.Any())
            {
                seriesList.Add(new ScatterSeries<ObservablePoint>
                {
                    Name = "Больные",
                    Values = diseasedPoints,
                    Fill = new SolidColorPaint(SKColors.Red.WithAlpha(150)),
                    GeometrySize = 10
                });
            }

            ScatterSeries = seriesList.ToArray();
        }

        private void UpdateCholesterolDist(List<HeartData> data)
        {
            var values = data
                .Select(x => double.TryParse(x.Cholesterol, out double v) ? v : 0)
                .Where(v => v > 0)
                .ToList();

            if (!values.Any())
            {
                CholesterolSeries = Array.Empty<ISeries>();
                CholesterolXAxes = new[] { new Axis { Name = "Уровень холестерина", Labels = Array.Empty<string>() } };
                return;
            }

            int binSize = 20;
            var histogram = values
                .GroupBy(v => Math.Floor(v / binSize) * binSize)
                .OrderBy(g => g.Key)
                .Select(g => new { Bin = g.Key, Count = g.Count() })
                .ToList();

            CholesterolSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Холестерин",
                    Values = histogram.Select(h => h.Count).ToArray(),
                    YToolTipLabelFormatter = (point) => $"{histogram[point.Index].Bin}-{histogram[point.Index].Bin + binSize}: {point.Coordinate.PrimaryValue} чел."
                }
            };

            CholesterolXAxes = new[] { new Axis 
            { 
                Name = "Уровень холестерина",
                Labels = histogram.Select(h => h.Bin.ToString()).ToArray()
            }};
        }

        private void UpdateBpByPain(List<HeartData> data)
        {
            var groupedData = data
                .GroupBy(x => x.ChestPainType)
                .Select(g => new
                {
                    Type = g.Key,
                    AvgBP = g.Average(x => double.TryParse(x.RestingBP, out double v) ? v : 0)
                })
                .OrderByDescending(x => x.AvgBP)
                .ToList();

            if (!groupedData.Any())
            {
                BpByPainSeries = Array.Empty<ISeries>();
                BpByPainXAxes = new[] { new Axis { Name = "Тип боли в груди", Labels = Array.Empty<string>() } };
                return;
            }

            BpByPainSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Среднее давление",
                    Values = groupedData.Select(x => x.AvgBP).ToArray(),
                    Fill = new SolidColorPaint(SKColors.BlueViolet)
                }
            };

            BpByPainXAxes = new[] { new Axis 
            { 
                Name = "Тип боли в груди",
                Labels = groupedData.Select(x => x.Type).ToArray()
            }};
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
