using System.Linq;
using System.Windows;
using MedML.Data;
using MedML.Security;

namespace MedML.View
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text?.Trim();
            var password = PasswordBox.Password ?? "";
            var hash = PasswordHelper.Hash(password);
            using (var ctx = new AppDbContext())
            {
                var user = ctx.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash && u.IsActive);
                if (user == null)
                {
                    MessageBox.Show("Неверные имя пользователя или пароль", "Вход", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Session.CurrentUser = user;
            }
            DialogResult = true;
            Close();
        }
    }
}
