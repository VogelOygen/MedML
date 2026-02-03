using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MedML.Data;
using MedML.Models;
using MedML.Security;

namespace MedML.View
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (var ctx = new AppDbContext())
            {
                UsersGrid.ItemsSource = ctx.Users.OrderBy(u => u.Id).ToList();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserEditorWindow();
            if (dialog.ShowDialog() == true)
            {
                using (var ctx = new AppDbContext())
                {
                    var user = new User
                    {
                        Username = dialog.Username,
                        PasswordHash = PasswordHelper.Hash(dialog.Password),
                        Role = dialog.Role,
                        IsActive = dialog.IsActive
                    };
                    ctx.Users.Add(user);
                    ctx.SaveChanges();
                }
                LoadUsers();
            }
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is User selected)
            {
                var dialog = new UserEditorWindow(selected.Username, selected.Role, selected.IsActive, false);
                if (dialog.ShowDialog() == true)
                {
                    using (var ctx = new AppDbContext())
                    {
                        var user = ctx.Users.Find(selected.Id);
                        if (user != null)
                        {
                            user.Username = dialog.Username;
                            user.Role = dialog.Role;
                            user.IsActive = dialog.IsActive;
                            if (!string.IsNullOrWhiteSpace(dialog.Password))
                            {
                                user.PasswordHash = PasswordHelper.Hash(dialog.Password);
                            }
                            ctx.SaveChanges();
                        }
                    }
                    LoadUsers();
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя", "Пользователи", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem is User selected)
            {
                if (MessageBox.Show("Удалить выбранного пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (var ctx = new AppDbContext())
                    {
                        var user = ctx.Users.Find(selected.Id);
                        if (user != null)
                        {
                            ctx.Users.Remove(user);
                            ctx.SaveChanges();
                        }
                    }
                    LoadUsers();
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя", "Пользователи", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
