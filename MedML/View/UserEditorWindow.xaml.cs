using System.Windows;
using System.Windows.Controls;

namespace MedML.View
{
    public partial class UserEditorWindow : Window
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Role { get; private set; }
        public bool IsActive { get; private set; }

        public UserEditorWindow(string username = "", string role = "Client", bool isActive = true, bool allowPasswordEmpty = true)
        {
            InitializeComponent();
            UsernameTextBox.Text = username;
            IsActiveCheckBox.IsChecked = isActive;
            foreach (ComboBoxItem item in RoleComboBox.Items)
            {
                if ((string)item.Content == role)
                {
                    RoleComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Username = UsernameTextBox.Text?.Trim();
            Password = PasswordBox.Password;
            Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Client";
            IsActive = IsActiveCheckBox.IsChecked == true;
            if (string.IsNullOrWhiteSpace(Username))
            {
                MessageBox.Show("Введите логин", "Пользователи", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
