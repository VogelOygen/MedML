using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MedML
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplyRoleVisibility();
        }

        private void ApplyRoleVisibility()
        {
            var role = Session.CurrentUser?.Role ?? "Client";
            if (role == "Admin")
            {
                TabData.Visibility = Visibility.Visible;
                TabDashboard.Visibility = Visibility.Visible;
                TabEda.Visibility = Visibility.Visible;
                TabForecast.Visibility = Visibility.Visible;
                TabLearn.Visibility = Visibility.Visible;
                TabUsers.Visibility = Visibility.Visible;
                TabUsers.Content = new View.UsersView();
            }
            else
            {
                TabData.Visibility = Visibility.Collapsed;
                TabDashboard.Visibility = Visibility.Collapsed;
                TabEda.Visibility = Visibility.Collapsed;
                TabForecast.Visibility = Visibility.Visible;
                TabLearn.Visibility = Visibility.Collapsed;
                TabUsers.Visibility = Visibility.Collapsed;
                TabUsers.Content = null;
                MainTabControl.SelectedItem = TabForecast;
            }
        }
    }
}
