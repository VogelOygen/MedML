using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using MedML.Models;

namespace MedML.View
{
    public partial class RecordEditorWindow : Window
    {
        public HeartDiseaseRecord Record { get; private set; }

        public RecordEditorWindow(HeartDiseaseRecord record = null)
        {
            InitializeComponent();

            if (record != null)
            {
                Record = new HeartDiseaseRecord
                {
                    Id = record.Id,
                    Age = record.Age,
                    Sex = record.Sex,
                    ChestPainType = record.ChestPainType,
                    RestingBP = record.RestingBP,
                    Cholesterol = record.Cholesterol,
                    FastingBS = record.FastingBS,
                    RestingECG = record.RestingECG,
                    MaxHR = record.MaxHR,
                    ExerciseAngina = record.ExerciseAngina,
                    Oldpeak = record.Oldpeak,
                    ST_Slope = record.ST_Slope,
                    HeartDisease = record.HeartDisease
                };
                PopulateFields();
                Title = "Редактирование записи";
            }
            else
            {
                Record = new HeartDiseaseRecord();
                Title = "Новая запись";
            }
        }

        private void IntegerTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void FloatTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var tb = sender as TextBox;
            string incoming = e.Text;
            if (!Regex.IsMatch(incoming, "^[0-9.]+$"))
            {
                e.Handled = true;
                return;
            }
            if (incoming.Contains(".") && tb != null && tb.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void PopulateFields()
        {
            AgeTextBox.Text = Record.Age.ToString();
            SetComboBox(SexComboBox, Record.Sex);
            SetComboBox(ChestPainTypeComboBox, Record.ChestPainType);
            RestingBPTextBox.Text = Record.RestingBP.ToString();
            CholesterolTextBox.Text = Record.Cholesterol.ToString();
            FastingBSComboBox.SelectedIndex = Record.FastingBS; // 0 or 1 maps to index
            SetComboBox(RestingECGComboBox, Record.RestingECG);
            MaxHRTextBox.Text = Record.MaxHR.ToString();
            SetComboBox(ExerciseAnginaComboBox, Record.ExerciseAngina);
            OldpeakTextBox.Text = Record.Oldpeak.ToString(System.Globalization.CultureInfo.InvariantCulture);
            SetComboBox(STSlopeComboBox, Record.ST_Slope);
            HeartDiseaseComboBox.SelectedIndex = Record.HeartDisease; // 0 or 1 maps to index
        }

        private void SetComboBox(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString().StartsWith(value))
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var errors = new System.Collections.Generic.List<string>();
                if (!int.TryParse(AgeTextBox.Text, out int age)) errors.Add("Возраст: введите целое число");
                if (!int.TryParse(RestingBPTextBox.Text, out int restingBP)) errors.Add("RestingBP: введите целое число");
                if (!int.TryParse(CholesterolTextBox.Text, out int cholesterol)) errors.Add("Cholesterol: введите целое число");
                if (!int.TryParse(MaxHRTextBox.Text, out int maxHR)) errors.Add("MaxHR: введите целое число");
                if (!float.TryParse(OldpeakTextBox.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float oldpeak)) errors.Add("Oldpeak: используйте точку как разделитель");

                if (errors.Count == 0)
                {
                    if (age < 18 || age > 100) errors.Add("Возраст должен быть в диапазоне 18–100");
                    if (restingBP < 60 || restingBP > 200) errors.Add("RestingBP должен быть в диапазоне 60–200");
                    if (cholesterol < 100 || cholesterol > 600) errors.Add("Cholesterol должен быть в диапазоне 100–600");
                    if (maxHR < 60 || maxHR > 220) errors.Add("MaxHR должен быть в диапазоне 60–220");
                    if (oldpeak < 0.0f || oldpeak > 10.0f) errors.Add("Oldpeak должен быть в диапазоне 0.0–10.0");
                }

                var sex = (SexComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                var cpt = (ChestPainTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                var ecg = (RestingECGComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                var ang = (ExerciseAnginaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                var slope = (STSlopeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (string.IsNullOrEmpty(sex)) errors.Add("Пол обязателен");
                if (string.IsNullOrEmpty(cpt)) errors.Add("Тип боли в груди обязателен");
                if (string.IsNullOrEmpty(ecg)) errors.Add("RestingECG обязателен");
                if (string.IsNullOrEmpty(ang)) errors.Add("ExerciseAngina обязателен");
                if (string.IsNullOrEmpty(slope)) errors.Add("ST_Slope обязателен");

                if (errors.Count > 0)
                {
                    MessageBox.Show(string.Join("\n", errors), "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Record.Age = age;
                Record.Sex = sex;
                Record.ChestPainType = cpt;
                Record.RestingBP = restingBP;
                Record.Cholesterol = cholesterol;
                Record.FastingBS = FastingBSComboBox.SelectedIndex;
                Record.RestingECG = ecg;
                Record.MaxHR = maxHR;
                Record.ExerciseAngina = ang;
                Record.Oldpeak = oldpeak;
                Record.ST_Slope = slope;
                Record.HeartDisease = HeartDiseaseComboBox.SelectedIndex;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
