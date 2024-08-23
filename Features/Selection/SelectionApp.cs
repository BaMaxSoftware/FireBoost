using System.IO;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Domain.Data;
using FireBoost.Domain.Enums;
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
        private readonly DBContext _dbContext;
        private readonly ErrorsHandler _errorsHandler;
        private readonly SelectionVM _viewModel;
        
        private readonly JsonHandler _json;

        private SettingsVM _settingsViewModel;
        private SettingsWindow _settingsWindow;
       
        private FamilySymbol _familySymbol;
        private MainWindow _mainWindow;

        private (string, string) _familyName;
        private (double Height, double Width, double Diameter) _dimensions = default;
        private double _offset;

        private (int dimensions, int elevation) _roundTo;

        /// <summary></summary>
        public UIDocument ActiveUIDoc { get; }
        /// <summary></summary>
        public Document ActiveDoc { get; }
        
        /// <summary></summary>
        public SelectionApp(UIDocument uIDoc)
        {
            _dbContext = new DBContext();
            _errorsHandler = new ErrorsHandler();
            _settingsViewModel = new SettingsVM();
            ActiveUIDoc = uIDoc;
            ActiveDoc = ActiveUIDoc.Document;
            _viewModel = new SelectionVM(this);
            _mainWindow = new MainWindow(_viewModel);

            _json = new JsonHandler();
            _json.Deserialize(ref _settingsViewModel);
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
            _mainWindow.Hide();

            if (_viewModel.IsValidData())
            {
                _familyName = (_viewModel.FamilyFromDb, _viewModel.TypeFromDb);

                if (_familyName == default)
                {
                    if (_errorsHandler.ShowQuestion(_errorsHandler.InvalidFamily) == MessageBoxResult.No) return;
                }
                else
                {
                    
                    if (LoadFamilySymbol(_familyName) == TransactionStatus.Committed)
                    {
                        if (double.TryParse(_viewModel.Offset, out _offset))
                        {
                            _offset = _offset * 0.0032808d * 2;
                            if (_viewModel.IsDimensionsManually)
                            {
                                if (_viewModel.SelectedShape.Shape == SealingShapeType.Round)
                                {
                                    if (double.TryParse(_viewModel.Diameter, out _dimensions.Diameter))
                                    {
                                        if (_dimensions.Diameter > 0)
                                        {
                                            _dimensions.Diameter = _dimensions.Diameter * 0.0032808d;
                                            Create();
                                        }
                                        else
                                        {
                                            if (_errorsHandler.ShowZeroValue("Диаметр") == MessageBoxResult.No) return;
                                        }
                                    }
                                    else
                                    {
                                        if (_errorsHandler.ShowQuestion(_errorsHandler.InvalidDiameter) == MessageBoxResult.No) return;
                                    }
                                }
                                else
                                {
                                    if (double.TryParse(_viewModel.Height, out _dimensions.Height))
                                    {
                                        if (_dimensions.Height > 0)
                                        {
                                            if (double.TryParse(_viewModel.Width, out _dimensions.Width))
                                            {
                                                if (_dimensions.Width > 0)
                                                {
                                                    _dimensions.Height = _dimensions.Height * 0.0032808d;
                                                    _dimensions.Width = _dimensions.Width * 0.0032808d;
                                                    Create();
                                                }
                                                else
                                                {
                                                    if (_errorsHandler.ShowZeroValue("Ширина") == MessageBoxResult.No) return;
                                                }
                                            }
                                            else
                                            {
                                                if (_errorsHandler.ShowQuestion(_errorsHandler.InvalidHeight) == MessageBoxResult.No) return;
                                            }
                                        }
                                        else
                                        {
                                            if (_errorsHandler.ShowZeroValue("Высота") == MessageBoxResult.No) return;
                                        }
                                    }
                                    else
                                    {
                                        if (_errorsHandler.ShowQuestion(_errorsHandler.InvalidWidth) == MessageBoxResult.No) return;
                                    }
                                }
                            }
                            else
                            {
                                if (_viewModel.SelectedMepType.AllowCategories.Length > 0)
                                { 
                                    _dimensions = default;
                                    Create();
                                }
                            }
                        }
                        else
                        {
                            if (_errorsHandler.ShowQuestion(_errorsHandler.InvalidOffset) == MessageBoxResult.No)
                                return;
                        }
                    }
                }
            }

            _mainWindow.Show();
        }

        private string[] GetFiles()
        {
            string[] result = GetFiles(
                _settingsViewModel.GetPath(
                    _viewModel.SelectedHost.BuiltInCategory, _viewModel.SelectedShape.Shape == SealingShapeType.Round));

            while (result == null || result.Length == 0)
            {
                if (_errorsHandler.ShowQuestion(_errorsHandler.HasFamilies) == MessageBoxResult.No) break;

                SettingsShowDialog();
                result = GetFiles();
            };

            return result;
        }
        /// <summary></summary>
        public string[] GetFiles(string path) => !string.IsNullOrEmpty(path) ? Directory.Exists(path) ? Directory.GetFiles(path).Where(x => Path.GetExtension(x) == ".rfa").ToArray() : null : null;
       
        private TransactionStatus LoadFamilySymbol((string name, string typeName) family)
        {
            _familySymbol = GetFamilySymbol(family);

            if (_familySymbol != null)
            {
                return TransactionStatus.Committed;
            }

            TransactionStatus result = TransactionStatus.Uninitialized;
            string[] files = GetFiles();
            if (files != default && files.Length > 0)
            {
                foreach (var file in files)
                { 
                    if (Path.GetFileNameWithoutExtension(file) == family.name)
                    {
                        result = new Transactions(ActiveDoc).LoadFamilySymbol(file, family.typeName, _errorsHandler, out _familySymbol);
                        
                        if (_familySymbol == null && result == TransactionStatus.Committed)
                        { 
                            _familySymbol = GetFamilySymbol(family);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private FamilySymbol GetFamilySymbol((string Name, string TypeName) family) => new FilteredElementCollector(ActiveDoc)
            .OfCategory(BuiltInCategory.OST_GenericModel)
            .OfClass(typeof(FamilySymbol))
            .WhereElementIsElementType()
            .Cast<FamilySymbol>()
            .FirstOrDefault(x => x.FamilyName == family.Name && x.Name == family.TypeName);

        private void Create()
        {
            _roundTo = (
                int.TryParse(_viewModel.DimensionsRoundTo, out int dRoundTo) ? dRoundTo : 0,
                int.TryParse(_viewModel.ElevationRoundTo, out int eRoundTo) ? eRoundTo : 0);

            if (_viewModel.SelectedMepType.AllowCategories.Length == 0)
            {
                new CreatorWithoutMEP(_viewModel, _settingsViewModel, ActiveDoc, _familySymbol, _dimensions, _offset, _roundTo).CreateInstances();
            }
            else
            {
                new CreatorWithMEP(_viewModel, _settingsViewModel, ActiveDoc, _familySymbol, _dimensions, _offset, _roundTo).CreateInstances();
            }
        }
    }
}
