using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Features.Json;
using FireBoost.Features.Settings;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace FireBoost.Features.Specifications
{
    /// <summary></summary>
    public class SpecificationsApp
    {
        private readonly ExternalCommandData _commandData;
        private readonly JsonHandler _json = new JsonHandler();
        private readonly Document _activeDoc;

        private SpecificationsVM _vm;
        private SettingsVM _settingsViewModel = new SettingsVM();
        private SettingsWindow _settingsWindow;
        private SpecificationsWindow _view;
        private Document _sourceDoc;
        
        /// <summary></summary>
        public ViewSchedule[] Schedules { get; private set; }

        /// <summary></summary>
        public SpecificationsApp(ExternalCommandData commandData) 
        {
            _commandData = commandData;
            _activeDoc = _commandData.Application.ActiveUIDocument.Document;
        }

        /// <summary></summary>
        public void Run()
        {
            bool fileExists = false;
            do
            {
                _json.Deserialize(ref _settingsViewModel);
                if (string.IsNullOrEmpty(_settingsViewModel.SchedulesPath) || !fileExists)
                {
                    if (!ShowSettings())
                    {
                        break;
                    }
                }

                fileExists = File.Exists(_settingsViewModel.SchedulesPath);
            }
            while (fileExists == false);

            if (fileExists)
            {
                do
                { 
                    try
                    { 
                        _sourceDoc = _commandData.Application.Application.OpenDocumentFile(_settingsViewModel.SchedulesPath);
                    }
                    catch (Autodesk.Revit.Exceptions.CorruptModelException nse)
                    {
                        if (MessageBox.Show($"{nse.Message}\n\nВыбрать другой шаблон?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                        {
                            ShowSettings();
                        }
                        else
                        { 
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                        break;
                    }
                }
                while (_sourceDoc == null || !_sourceDoc.IsValidObject);

                if (_sourceDoc == null || !_sourceDoc.IsValidObject) return;

                Schedules = new FilteredElementCollector(_sourceDoc)
                    .OfClass(typeof(ViewSchedule))
                    .Cast<ViewSchedule>()
                    .ToArray();

                if (Schedules?.Length > 0)
                {
                    _vm = new SpecificationsVM(this);
                    _view = new SpecificationsWindow(_vm);
                    if (_view.ShowDialog() == true)
                    {
                        if (_vm.SelectedSchedules?.Count > 0 && _sourceDoc?.IsValidObject == true && _activeDoc?.IsValidObject == true)
                        {
                            using (Transaction t = new Transaction(_activeDoc))
                            {
                                try
                                {
                                    t.Start("Копирование видов");
                                    ElementTransformUtils.CopyElements(_sourceDoc, _vm.SelectedSchedules.Select(x => x.Id).ToArray(), _activeDoc, null, new CopyPasteOptions());
                                    t.Commit();
                                }
                                catch (Exception ex)
                                {
                                    if (t.HasStarted())
                                        t.RollBack();

                                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Не найдены спецификации в документе \"{_sourceDoc.Title}\".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                _sourceDoc.Close(false);
            }
        }

        /// <summary></summary>
        public void SettingsShowDialog()
        {
            _json.Deserialize(ref _settingsViewModel);
            _settingsWindow = new SettingsWindow()
            {
                DataContext = _settingsViewModel
            };
            if (_settingsWindow.ShowDialog() == true)
            {
                _json.Serialize(_settingsViewModel);
            }
        }

        private bool ShowSettings()
        {
            _settingsWindow = new SettingsWindow()
            {
                DataContext = _settingsViewModel
            };
            bool result = _settingsWindow.ShowDialog() == true;
            if (result)
            {
                _json.Serialize(_settingsViewModel);
            }
            return result;
        }
    }
}
