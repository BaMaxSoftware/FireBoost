using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Features.Json;
using FireBoost.Features.ParametersCopy.ViewModels;
using FireBoost.Features.ParametersCopy.Views;
using FireBoost.Features.Settings;
using System.Linq;

namespace FireBoost.Features.ParametersCopy
{
    internal class ParametersApp
    {
        private readonly JsonHandler _json = new JsonHandler();
        private SettingsVM _settingsVM = new SettingsVM();
        private ParametersVM _vm;
        private ParametersWindow _view;

        public Document Document { get; }
        public UIDocument UIDocument { get; }
        public UIApplication UIApplication { get; }
        public Application Application { get; }
        
        
        public ParametersApp(UIApplication uiapp)
        { 
            UIApplication = uiapp;
            Application = uiapp.Application;
            UIDocument = uiapp.ActiveUIDocument;
            Document = UIDocument.Document;
            _json.Deserialize(ref _settingsVM);
        }


        public void Run()
        {
            var bindings = Document.ParameterBindings;
            string[] projectParameters = new string[bindings.Size + 1];
            projectParameters[0] = "<Отменить выбор>";

            if (bindings.Size > 0)
            { 
                var fi = bindings.ForwardIterator();
                fi.Reset();

                for (int i = 1; i < projectParameters.Length; i++)
                { 
                    fi.MoveNext();
                    projectParameters[i]= fi.Key.Name;
                }
            }

            _vm = new ParametersVM(Document.Title, projectParameters, _settingsVM.Parameters);
            _view = new ParametersWindow() { DataContext = _vm };
            if (_view.ShowDialog() == true)
            {
                SettingsSave();
            }
        }

        private void SettingsSave()
        {
            _settingsVM.Parameters = _vm.Parameters.Where(x=> !string.IsNullOrEmpty(x.SelectedParameter))?.Select(x=> (x.Name, x.SelectedParameter)).ToArray();
            _json.Serialize(_settingsVM);
        }
    }
}
