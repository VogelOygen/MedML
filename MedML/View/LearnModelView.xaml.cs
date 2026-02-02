using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MedML.ViewModel;

namespace MedML.View
{
    /// <summary>
    /// Логика взаимодействия для LearnModelView.xaml
    /// </summary>
    public partial class LearnModelView : UserControl
    {
        /// <summary>
        /// ViewModel для этого контрола
        /// </summary>
        private LearnModelViewModel _viewModel;

        public LearnModelView()
        {
            InitializeComponent();
            
            // Создаем экземпляр ViewModel и устанавливаем его как DataContext
            _viewModel = new LearnModelViewModel();
            this.DataContext = _viewModel;
        }
    }
}
