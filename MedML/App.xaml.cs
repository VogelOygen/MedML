using System.Configuration;
using System.Data;
using System.Windows;
using MedML.Data;

namespace MedML
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var context = new AppDbContext())
            {
                DbInitializer.Initialize(context);
            }
            this.DispatcherUnhandledException += (s, exArgs) =>
            {
                MessageBox.Show($"Необработанная ошибка: {exArgs.Exception.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                exArgs.Handled = true;
            };
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var login = new View.LoginWindow();
            var result = login.ShowDialog();
            if (result != true || Session.CurrentUser == null)
            {
                Shutdown();
                return;
            }
            var main = new MainWindow();
            Current.MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
        }
    }

}
