using System;
using System.Windows;
using System.Windows.Controls;
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
                if (!int.TryParse(AgeTextBox.Text, out int age)) throw new Exception("Некорректный возраст");
                if (!int.TryParse(RestingBPTextBox.Text, out int restingBP)) throw new Exception("Некорректное давление");
                if (!int.TryParse(CholesterolTextBox.Text, out int cholesterol)) throw new Exception("Некорректный холестерин");
                if (!int.TryParse(MaxHRTextBox.Text, out int maxHR)) throw new Exception("Некорректный пульс");
                if (!float.TryParse(OldpeakTextBox.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float oldpeak)) throw new Exception("Некорректный Oldpeak");

                Record.Age = age;
                Record.Sex = (SexComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                Record.ChestPainType = (ChestPainTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                Record.RestingBP = restingBP;
                Record.Cholesterol = cholesterol;
                Record.FastingBS = FastingBSComboBox.SelectedIndex;
                Record.RestingECG = (RestingECGComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                Record.MaxHR = maxHR;
                Record.ExerciseAngina = (ExerciseAnginaComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                Record.Oldpeak = oldpeak;
                Record.ST_Slope = (STSlopeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
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
