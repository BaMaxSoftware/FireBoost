using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Domain.Data;
using FireBoost.Features.Json;
using FireBoost.Features.Selection.Models;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Selection.Views;
using FireBoost.Features.Settings;

namespace FireBoost.Features.Selection
{
    /// <summary></summary>
    public class SelectionApp
    {
        private readonly ErrorsHandler _errorsHandler;
        private readonly SelectionVM _viewModel;
        private readonly JsonHandler _json;

        private SettingsVM _settingsViewModel;
        private SettingsWindow _settingsWindow;
        private MainWindow _mainWindow;
        private ExternalEvent _externalEvent;
        /// <summary></summary>
        private Document _activeDoc;
        
        /// <summary></summary>
        public SelectionApp()
        {
            _errorsHandler = new ErrorsHandler();
            _settingsViewModel = new SettingsVM();
            _viewModel = new SelectionVM(this);
            _mainWindow = new MainWindow(_viewModel);

            _json = new JsonHandler();
            _json.Deserialize(ref _settingsViewModel);

            _externalEvent = ExternalEvent.Create(new CreateEvent(
                _mainWindow,
                _viewModel,
                _json,
                _errorsHandler,
                _settingsViewModel));
        }

        /// <summary></summary>
        public void ShowWindow() => _mainWindow.Show();

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

        /// <summary></summary>
        public MainWindow GetMainWindow() => _mainWindow;

        /// <summary></summary>
        public void Start()
        {
            if (_externalEvent != null)
                _externalEvent.Raise();
        } 
    }
}
