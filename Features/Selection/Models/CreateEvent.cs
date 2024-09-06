using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Domain.Enums;
using FireBoost.Features.Json;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Selection.Views;
using FireBoost.Features.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Visibility = System.Windows.Visibility;

namespace FireBoost.Features.Selection.Models
{
    internal class CreateEvent : IExternalEventHandler
    {
        private readonly ErrorsHandler _errorsHandler;
        private readonly JsonHandler _json;
        private readonly SelectionVM _viewModel;

        private (double Offset, double Thickness) _offsets;
        private Document _activeDoc;
        private FamilySymbol _familySymbol;
        private SettingsVM _settingsViewModel;
        private SettingsWindow _settingsWindow;
        private (double Height, double Width, double Diameter) _dimensions = default;
        private (string Name, string TypeName) _familyName;
        
        enum ValidationStatus
        { 
            INVALID,
            Success,
            Return,
            Exit
        }

        public CreateEvent(
            SelectionVM viewModel,
            JsonHandler json,
            ErrorsHandler errorsHandler,
            SettingsVM settingsViewModel)
        {
            _viewModel = viewModel;
            _json = json;
            _errorsHandler = errorsHandler;
            _settingsViewModel = settingsViewModel;
        }


        public string GetName() => "CreateEvent";

        public void Execute(UIApplication app)
        {
            _activeDoc = app?.ActiveUIDocument?.Document;
            if (_activeDoc == null)
            {
                MessageBox.Show("Не найден объект, представляющий открытый проект Autodesk Revit.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            SingletonWindow.SetVisibility(Visibility.Hidden);
            switch (Validation())
            {
                case ValidationStatus.Success:
                    Create();
                    break;

                case ValidationStatus.Exit:
                    SingletonWindow.Close();
                    return;

                case ValidationStatus.Return:
                default:
                    break;
            }
            SingletonWindow.SetVisibility(Visibility.Visible);
        }

        private ValidationStatus CheckFamilyData()
        {
            ValidationStatus result;
            _familyName = (_viewModel.FamilyFromDb, _viewModel.TypeFromDb);
            if (_familyName == default)
            {
                result = _errorsHandler.ShowQuestion(_errorsHandler.InvalidFamily) == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
            }
            else
            { 
                result = ValidationStatus.Success;
            }
            return result;
        }

        private ValidationStatus CheckOffset()
        {
            ValidationStatus result;
            if (double.TryParse(_viewModel.Offset, out _offsets.Offset))
            {
                _offsets.Offset = _offsets.Offset * 0.0032808d * 2;
                result = ValidationStatus.Success;
            }
            else
            {
                result = _errorsHandler.ShowQuestion(_errorsHandler.InvalidOffset) == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
            }
            return result;
        }

        private ValidationStatus CheckThickness()
        {
            ValidationStatus result;
            if (double.TryParse(_viewModel.Thickness, out _offsets.Thickness))
            {
                _offsets.Thickness *= 0.0032808d;
                result = ValidationStatus.Success;
            }
            else
            {
                result = _errorsHandler.ShowQuestion(_errorsHandler.InvalidThickness) == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
            }
            return result;
        }

        private ValidationStatus CheckRoundDimensions()
        {
            ValidationStatus result;
            if (double.TryParse(_viewModel.Diameter, out _dimensions.Diameter))
            {
                if (_dimensions.Diameter > 0)
                {
                    _dimensions.Diameter *= 0.0032808d;
                    result = ValidationStatus.Success;
                }
                else
                {
                    result = _errorsHandler.ShowZeroValue("Диаметр") == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
                }
            }
            else
            {
                result = _errorsHandler.ShowQuestion(_errorsHandler.InvalidDiameter) == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
            }
            return result;
        }

        private ValidationStatus CheckRechtangularDimensions()
        {
            ValidationStatus result;
            if (double.TryParse(_viewModel.Height, out _dimensions.Height))
            {
                if (_dimensions.Height > 0)
                {
                    if (double.TryParse(_viewModel.Width, out _dimensions.Width))
                    {
                        if (_dimensions.Width > 0)
                        {
                            _dimensions.Height *= 0.0032808d;
                            _dimensions.Width *= 0.0032808d;
                            result = ValidationStatus.Success;
                        }
                        else
                        {
                            result = _errorsHandler.ShowZeroValue("Ширина") == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
                        }
                    }
                    else
                    {
                        result = _errorsHandler.ShowQuestion(_errorsHandler.InvalidHeight) == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
                    }
                }
                else
                {
                    result = _errorsHandler.ShowZeroValue("Высота") == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
                }
            }
            else
            {
                result = _errorsHandler.ShowQuestion(_errorsHandler.InvalidWidth) == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
            }
            return result;
        }

        private ValidationStatus CheckDimensionsManually()
        {
            ValidationStatus result;
            if (_offsets.Thickness <= 0)
            {
                return _errorsHandler.ShowZeroValue("Толщина") == MessageBoxResult.No ? ValidationStatus.Exit : ValidationStatus.Return;
            }

            switch (_viewModel.SelectedShape.Shape)
            {
                case SealingShapeType.Round:
                    result = CheckRoundDimensions();
                    break;
                case SealingShapeType.Reachtangle:
                    result = CheckRechtangularDimensions();
                    break;
                default: 
                    result = ValidationStatus.Return;
                    break;
            }
            return result;
        }

        private ValidationStatus Validation()
        {
            ValidationStatus result = ValidationStatus.Return;
            if (!_viewModel.IsValidData())
                return result;

            result = CheckFamilyData();
            if (result != ValidationStatus.Success)
                return result;

            if (LoadFamilySymbol(_familyName) != TransactionStatus.Committed)
                return ValidationStatus.Return;

            result = CheckOffset();
            if (result != ValidationStatus.Success)
                return result;

            result = CheckThickness();
            if (result != ValidationStatus.Success)
                return result;

            _dimensions = default;
            if (_viewModel.IsDimensionsManually)
            {
                result = CheckDimensionsManually();
            }
            else
            {
                if (_viewModel.SelectedMepType.AllowedCategories.Length > 0)
                {
                    result = ValidationStatus.Success;
                }
                else 
                {
                    new TaskDialog("Предупреждение")
                    {
                        TitleAutoPrefix = false,
                        CommonButtons = TaskDialogCommonButtons.Ok,
                        MainContent = "Для выбранного типа коммуникаций необходимо указать размеры проходки.",
                        MainIcon = TaskDialogIcon.TaskDialogIconInformation
                    }.Show();
                    
                    result = ValidationStatus.Return;
                }
            }
            return result;
            _mainWindow.Focus();
            _mainWindow.Focus();
            _mainWindow.Focus();
        }

        private string[] GetFiles()
        {
            string[] result = GetFiles(
                _settingsViewModel.GetPath(
                    _viewModel.SelectedHost.DBId, _viewModel.SelectedShape.Shape == SealingShapeType.Round));

            while (result == null || result.Length == 0)
            {
                if (_errorsHandler.ShowQuestion(_errorsHandler.HasFamilies) == MessageBoxResult.No) break;

                _json.Deserialize(ref _settingsViewModel);
                _settingsWindow = new SettingsWindow()
                {
                    DataContext = _settingsViewModel
                };
                if (_settingsWindow.ShowDialog() == true)
                {
                    _json.Serialize(_settingsViewModel);
                }
                result = GetFiles();
            };
            return result;
        }
        
        private string[] GetFiles(string path) => !string.IsNullOrEmpty(path) ? Directory.Exists(path) ? Directory.GetFiles(path).Where(x => Path.GetExtension(x) == ".rfa").ToArray() : null : null;

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
                        result = new Transactions(_activeDoc).LoadFamilySymbol(file, family.typeName, _errorsHandler, out _familySymbol);

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

        private FamilySymbol GetFamilySymbol((string Name, string TypeName) family) => new FilteredElementCollector(_activeDoc)
            .OfCategory(BuiltInCategory.OST_GenericModel)
            .OfClass(typeof(FamilySymbol))
            .WhereElementIsElementType()
            .Cast<FamilySymbol>()
            .FirstOrDefault(x => x.FamilyName == family.Name && x.Name == family.TypeName);
            (int Dimensions, int Elevation) roundUp = (
            _roundTo = (
            _roundTo = (
            _roundTo = (
            CreatorBase creator;
            if (_viewModel.SelectedMepType.AllowedCategories.Length == 0 || _viewModel.IsDimensionsManually)
            if (_viewModel.SelectedMepType.AllowCategories.Length == 0)
            if (_viewModel.SelectedMepType.AllowCategories.Length == 0)
            if (_viewModel.SelectedMepType.AllowCategories.Length == 0)
            {
                creator = new CreatorWithoutMEP(_viewModel, _settingsViewModel, _activeDoc, _familySymbol, _dimensions, _offsets, roundUp);
            }
            else
            {
                creator = new CreatorWithMEP(_viewModel, _settingsViewModel, _activeDoc, _familySymbol, _dimensions, _offsets, roundUp);
            }
            creator.CreateInstances();
        }
    }
}
