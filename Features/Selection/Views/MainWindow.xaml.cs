using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using FireBoost.Features.Selection.ViewModels;

namespace FireBoost.Features.Selection.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SelectionVM _vm;

        /// <summary></summary>
        public MainWindow(SelectionVM vm)
        { 
            _vm = vm;
            DataContext = _vm;
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e) => _vm.Start();

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void ButtonSettings_Click(object sender, RoutedEventArgs e) => _vm.SettingsShowDialog();

        private void Window_Loaded(object sender, RoutedEventArgs e) => _vm.GetActiveUIDocument();
    }
}
