using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FireBoost.Features.Json
{
    /// <summary></summary>
    public class JsonHandler
    {
        private readonly string _assmblyLocation;
        private readonly string _directoryPath;
        private readonly string _settings = "Directories.json";

        /// <summary></summary>
        public JsonHandler()
        {
            _assmblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _directoryPath = Path.Combine(_assmblyLocation, "json");
        }

        /// <summary></summary>
        public void Serialize(SettingsVM arr, string name = "")
        {
            if (IsExistsDirectory())
            {
                File.WriteAllText(Path.Combine(_directoryPath, name == "" ? _settings : name), JsonConvert.SerializeObject(arr));
            }
        }

        private bool IsExistsDirectory()
        {
            if (!Directory.Exists(_directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(_directoryPath);
                }
                catch (Exception e)
                {
                    ShowErrorMessage(e.Message);
                    return false;
                }
            }
            return true;
        }

        /// <summary></summary>
        public void Deserialize(ref SettingsVM settingsVM, string fullFilePath = "")
        {
            if (string.IsNullOrEmpty(fullFilePath))
            {
                fullFilePath = Path.Combine(_directoryPath, _settings);
            }
            if (!File.Exists(fullFilePath))
            {
                ShowErrorMessage($"Файл не найден: {fullFilePath}.");
                return;
            }
            try
            {
                settingsVM = JsonConvert.DeserializeObject<SettingsVM>(File.ReadAllText(fullFilePath));
            }
            catch (Exception e)
            {
                ShowErrorMessage($"{e.Message}\n\n{e.StackTrace}");
            }
        }

        private void ShowErrorMessage(string txt) => MessageBox.Show(txt, "Предупреждение", MessageBoxButton.OK, icon: MessageBoxImage.Exclamation);
    }
}
