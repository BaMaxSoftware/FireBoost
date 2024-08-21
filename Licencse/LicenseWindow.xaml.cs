using System.Windows;
using System.Windows.Media;
using LP = License.Properties;

namespace License
{
    /// <summary>
    /// Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class LicenseWindow : Window
    {
        private readonly SolidColorBrush brushOk = new SolidColorBrush(Color.FromRgb(35, 220, 60));
        private readonly SolidColorBrush brushWhite = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private readonly SolidColorBrush brushFalse = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        private readonly LicenseSetting _licenceSetting;

        public LicenseWindow()
        {
            _licenceSetting = Core.JsonSetting<LicenseSetting>.Open();
            InitializeComponent();
            CompanyNameTb.Text = _licenceSetting.CompanyName;
            PasswordBox.Password = _licenceSetting.Password; 
            ValidatePassword();
        }

        private bool ValidatePassword()
        {
            bool isEnteredPasswordValid = Licenser.ValidatePassword(false);
            PasswordBox.Background = isEnteredPasswordValid ? brushOk : brushFalse;
            CompanyNameTb.Background = isEnteredPasswordValid ? brushOk : brushFalse;
            return isEnteredPasswordValid;
        }

        private void OkButtom_Click(object sender, RoutedEventArgs e)
        {
            _licenceSetting.CompanyName = CompanyNameTb.Text;
            _licenceSetting.Password = PasswordBox.Password;
            Core.JsonSetting<LicenseSetting>.Save(_licenceSetting);

            if (ValidatePassword())
            {
                MessageBox.Show(this, $"{LP.Resources.LicenseValid}. {LP.Resources.RestartRevitForCorrectAddin}", LP.Resources.BTLicense);
                Close();
            }
            else
            {
                MessageBox.Show(this, $"{LP.Resources.IncorrectKey}. {LP.Resources.ForLicencePurchasingGoToBT}", LP.Resources.BTLicense);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox.Background = brushWhite;
        }
    }
}
