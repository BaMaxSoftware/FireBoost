using System;
using System.Windows;
using System.Windows.Forms;

namespace FireBoost.Features.Settings
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary></summary>
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void RectangularWallPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetPath(out string text))
                RectangularWallPath.Text = text;
        }

        private void RectangularFloorPathButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (TryGetPath(out string text))
                RectangularFloorPath.Text = text;
        }

        private void RoundWallPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetPath(out string text))
                RoundWallPath.Text = text;
        }

        private void RoundFloorPathButton_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetPath(out string text))
                RoundFloorPath.Text = text;
        }

        /// <summary></summary>
        public bool TryGetPath(out string path)
        {
            bool result = false;
            FolderBrowserDialog fbd = new FolderBrowserDialog()
            {
                RootFolder = Environment.SpecialFolder.MyComputer
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = fbd.SelectedPath;
                result = true;
            }
            else
            {
                path = null;
            }
            return result;
        }

        private void SchedulesPathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "Открыть",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer),
                Filter = "Файлы проектов (*.rvt)|*.rvt", 
                Multiselect = false
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SchedulesPath.Text = ofd.FileName;
            }
        }
    }
}
