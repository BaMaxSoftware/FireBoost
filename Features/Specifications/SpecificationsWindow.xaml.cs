using Autodesk.Revit.DB;
using System.Windows;
using System.Windows.Controls;

namespace FireBoost.Features.Specifications
{
    /// <summary>
    /// Логика взаимодействия для SpecificationWindow.xaml
    /// </summary>
    public partial class SpecificationsWindow : Window
    {
        private readonly SpecificationsVM _vm;

        /// <summary></summary>
        public SpecificationsWindow(SpecificationsVM vm)
        {
            _vm = vm;
            DataContext = _vm;
            InitializeComponent();
        }

        private void ViewSelector_Checked(object sender, RoutedEventArgs e) => _vm.AddSchedule((sender as CheckBox)?.CommandParameter as Element);

        private void ViewSelector_Unchecked(object sender, RoutedEventArgs e) => _vm.RemoveSchedule((sender as CheckBox)?.CommandParameter as Element);

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e) => _vm.SettingsShowDialog();
    }
}
