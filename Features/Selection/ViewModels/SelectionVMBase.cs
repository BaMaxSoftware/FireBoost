using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using FireBoost.Domain.Data;
using FireBoost.Domain.Entities;
using FireBoost.Features.Selection.Models;

namespace FireBoost.Features.Selection.ViewModels
{
    public partial class SelectionVM : INotifyPropertyChanged, IDataErrorInfo
    {

        #region Fields
        private readonly string _familyNameDefault = "<Не найдено>";
        private readonly string _familyTypeDefault = "<Не найден>";
        private readonly SealingFireResistance[] _fireResistancesArray;
        private readonly SealingMaterial[] _materialsArray;
        private readonly SealingMEPType[] _mEPTypesArray;
        private readonly SealingShape[] _shapesArray;
        private readonly SealingStructuralDesign[] _structuralDesignsArray;
        private readonly SelectionApp _selectionApp;
        private readonly SealingHost[] _hostsArray;
        private readonly DBContext _dBContext = new DBContext();
        private readonly PickObjects _pickObjects = new PickObjects();
        private readonly SealingData _sealingData = new SealingData();

        private bool _isJoin, _isDimensionsManually, _buttonOKIsEnabled;
        private int _docMepCount, _docHostCount, _linkMepCount, _linkHostCount;
        private string _dimensionsRoundTo, _elevationRoundTo, _diameter, _height, _width, _offset, _familyFromDb, _typeFromDb;
        private ListCollectionView _communicationsTypes;
        private SealingFireResistance _selectedFireResistances;
        private SealingHost _selectedHost;
        private SealingMaterial _selectedMaterial;
        private SealingMEPType _selectedMepType;
        private SealingShape _selectedShape;
        private SealingStructuralDesign _selectedStructuralDesign;
        private IList<Reference> _docElementReferences, _docHostReferences, _linkElementReferences, _linkHostReferences;
        private ICommand _docElementSelection, _docHostSelection, _linkElementSelection, _linkHostSelection;
        #endregion Fields

        #region Properties
        /// <summary></summary>
        public bool IsJoin 
        {
            get => _isJoin;
            set => ChangeProperty(ref _isJoin, value); 
        }
        /// <summary></summary>
        public IList<Reference> DocElementReferences
        {
            get => _docElementReferences;
            set
            {
                TryChangeProperty(ref _docElementReferences, value);
                DocMepCount = _docElementReferences.Count;
            }
        }
        /// <summary></summary>
        public IList<Reference> DocHostReferences
        {
            get => _docHostReferences;
            set
            {
                TryChangeProperty(ref _docHostReferences, value);
                DocHostCount = _docHostReferences.Count;
            }
        }
        /// <summary></summary>
        public IList<Reference> LinkElementReferences
        {
            get => _linkElementReferences;
            set
            {
                TryChangeProperty(ref _linkElementReferences, value);
                LinkMepCount = _linkElementReferences.Count;
            }
        }
        /// <summary></summary>
        public IList<Reference> LinkHostReferences
        {
            get => _linkHostReferences;
            set
            {
                TryChangeProperty(ref _linkHostReferences, value);
                LinkHostCount = _linkHostReferences.Count;
            }
        }

        /// <summary></summary>
        public string FamilyFromDb { get => _familyFromDb; set => ChangeProperty(ref _familyFromDb, value); }
        /// <summary></summary>
        public string TypeFromDb { get => _typeFromDb; set => ChangeProperty(ref _typeFromDb, value); }
        /// <summary></summary>
        public bool ButtonOKIsEnabled { get => _buttonOKIsEnabled; set => ChangeProperty(ref _buttonOKIsEnabled, value); }

        /// <summary></summary>
        public int DocMepCount
        {
            get => _docMepCount;
            set
            {
                TryChangeProperty(ref _docMepCount, value);
            }
        }
        /// <summary></summary>
        public int DocHostCount
        {
            get => _docHostCount;
            set
            {
                TryChangeProperty(ref _docHostCount, value);
            }
        }
        /// <summary></summary>
        public int LinkMepCount
        {
            get => _linkMepCount;
            set
            {
                TryChangeProperty(ref _linkMepCount, value);
            }
        }
        /// <summary></summary>
        public int LinkHostCount
        {
            get => _linkHostCount;
            set
            {
                TryChangeProperty(ref _linkHostCount, value);
            }
        }

        /// <summary></summary>
        public string DimensionsRoundTo
        {
            get => _dimensionsRoundTo;
            set => TryChangeProperty(ref _dimensionsRoundTo, value);
        }
        /// <summary></summary>
        public string ElevationRoundTo
        {
            get => _elevationRoundTo;
            set => TryChangeProperty(ref _elevationRoundTo, value);
        }
        /// <summary></summary>
        public string Width
        {
            get => _width;
            set => TryChangeProperty(ref _width, value);
        }
        /// <summary></summary>
        public string Height
        {
            get => _height;
            set => TryChangeProperty(ref _height, value);
        }
        /// <summary></summary>
        public string Diameter
        {
            get => _diameter;
            set => TryChangeProperty(ref _diameter, value);
        }
        /// <summary></summary>
        public string Offset
        {
            get => _offset;
            set => TryChangeProperty(ref _offset, value);
        }
                /// <summary></summary>
        public bool IsDimensionsManually
        {
            get => _isDimensionsManually;
            set
            {
                ChangeProperty(ref _isDimensionsManually, value);
            }
        }
        /// <summary></summary>
        public SealingMEPType SelectedMepType
        {
            get => _selectedMepType;
            set
            {
                if (TryChangeProperty(ref _selectedMepType, value))
                { 
                    if (_linkElementReferences.Count > 0)
                        LinkElementReferences = new List<Reference>();
                    if (_docElementReferences.Count > 0)
                        DocElementReferences = new List<Reference>();
                    SearchInDB();
                }
            }
        }
        /// <summary></summary>
        public SealingHost SelectedHost
        {
            get => _selectedHost;
            set
            {
                if (TryChangeProperty(ref _selectedHost, value))
                { 
                    if (_linkHostReferences.Count > 0)
                        LinkHostReferences = new List<Reference>();
                    if (_docHostReferences.Count > 0)
                        DocHostReferences = new List<Reference>();
                    SearchInDB();
                }
            }
        }
        /// <summary></summary>
        public SealingFireResistance SelectedFireResistance
        {
            get => _selectedFireResistances;
            set
            { 
                if (TryChangeProperty(ref _selectedFireResistances, value))
                    SearchInDB();
            }
        }
        /// <summary></summary>
        public SealingMaterial SelectedMaterial
        {
            get => _selectedMaterial;
            set
            { 
                if(TryChangeProperty(ref _selectedMaterial, value))
                    SearchInDB();
            }
        }
        /// <summary></summary>
        public SealingStructuralDesign SelectedStructuralDesign
        {
            get => _selectedStructuralDesign;
            set
            {
                if (TryChangeProperty(ref _selectedStructuralDesign, value))
                    SearchInDB();
            }
        }
        /// <summary></summary>
        public SealingShape SelectedShape
        {
            get => _selectedShape;
            set
            {
                if (TryChangeProperty(ref _selectedShape, value))
                    SearchInDB();
            }
        }

        /// <summary></summary>
        public ListCollectionView CommunicationsTypes => _communicationsTypes ??
            (_communicationsTypes = new Func<ListCollectionView>(() =>
            {
                ListCollectionView lcv = new ListCollectionView(MEPTypesArray);
                lcv.GroupDescriptions.Add(new PropertyGroupDescription("MainTypeString"));
                return lcv;
            })());

        /// <summary></summary>
        public SealingMEPType[] MEPTypesArray => _mEPTypesArray;
        /// <summary></summary>
        public SealingFireResistance[] FireResistancesArray => _fireResistancesArray;
        /// <summary></summary>
        public SealingMaterial[] MaterialsArray => _materialsArray;
        /// <summary></summary>
        public SealingStructuralDesign[] StructuralDesignsArray => _structuralDesignsArray;
        /// <summary></summary>
        public SealingShape[] ShapesArray => _shapesArray;
        /// <summary></summary>
        public SealingHost[] HostsArray => _hostsArray;

        /// <summary></summary>
        public ICommand DocElementSelection => _docElementSelection ?? (_docElementSelection = new VMCommand(obj =>
        {
            _selectionApp.GetMainWindow().Hide();
            if (CheckMepTypes())
            {
                DocElementReferences = _pickObjects.Select(
                    ObjectType.Element,
                    new SelectionFilter(_selectedMepType.AllowCategories, false, _getActiveUIEvent.ActiveDocument),
                    _docElementReferences,
                    _getActiveUIEvent.ActiveUIDocument,
                    false);
            }
            _selectionApp.GetMainWindow().Show();
        }));

        /// <summary></summary>
        public ICommand DocHostSelection => _docHostSelection ?? (_docHostSelection = new VMCommand(obj =>
        {
            _selectionApp.GetMainWindow().Hide();
            if (CheckHosts())
            {
                DocHostReferences = _pickObjects.Select(
                    ObjectType.Element,
                    new SelectionFilter(_selectedHost.BuiltInCategory, false, _getActiveUIEvent.ActiveDocument),
                    _docHostReferences,
                    _getActiveUIEvent.ActiveUIDocument,
                    true);
            }
            _selectionApp.GetMainWindow().Show();
        }));

        /// <summary></summary>
        public ICommand LinkElementSelection => _linkElementSelection ?? (_linkElementSelection = new VMCommand(obj =>
        {
            _selectionApp.GetMainWindow().Hide();
            if (CheckMepTypes())
            {
                LinkElementReferences = _pickObjects.Select(
                    ObjectType.LinkedElement,
                    new SelectionFilter(_selectedMepType.AllowCategories, true, _getActiveUIEvent.ActiveDocument),
                    _linkElementReferences,
                    _getActiveUIEvent.ActiveUIDocument,
                    false);
            }
            _selectionApp.GetMainWindow().Show();
        }));

        /// <summary></summary>
        public ICommand LinkHostSelection => _linkHostSelection ?? (_linkHostSelection = new VMCommand(obj =>
        {
            _selectionApp.GetMainWindow().Hide();
            if (CheckHosts())
            {
                LinkHostReferences = _pickObjects.Select(
                    ObjectType.LinkedElement,
                    new SelectionFilter(_selectedHost.BuiltInCategory, true, _getActiveUIEvent.ActiveDocument),
                    _linkHostReferences,
                    _getActiveUIEvent.ActiveUIDocument,
                    true);
            }
            _selectionApp.GetMainWindow().Show();
        }));
        #endregion Properties

        #region Реализация IDataErrorInfo
        /// <summary></summary>
        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "Height":
                        TryParseToDouble(_height, out error);
                        break;
                    case "Width":
                        TryParseToDouble(_width, out error);
                        break;
                    case "Diameter":
                        TryParseToDouble(_diameter, out error);
                        break;
                    case "Offset":
                        TryParseToDouble(_offset, out error);
                        break;
                    case "DimensionsRoundTo":
                        TryParseToInt(_dimensionsRoundTo, out error);
                        break;
                    case "ElevationRoundTo":
                        TryParseToInt(_elevationRoundTo, out error);
                        break;
                }
                return error;
            }
        }

        /// <summary></summary>
        public string Error => throw new NotImplementedException();
        #endregion Реализация IDataErrorInfo

        /// <summary></summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary></summary>
        public SelectionVM(SelectionApp selectionApp)
        {
            _selectionApp = selectionApp;
            _hostsArray = _sealingData.CreateHostsTypesArray();
            _mEPTypesArray = _sealingData.CreateMEPTypesArray();
            _fireResistancesArray = _sealingData.CreateFireResistancesArray();
            _materialsArray = _sealingData.CreateMaterialsArray();
            _structuralDesignsArray = _sealingData.CreateStructuralDesignsArray();
            _shapesArray = _sealingData.CreateShapesArray();

            _docElementReferences =
            _docHostReferences =
            _linkElementReferences =
            _linkHostReferences = new List<Reference>();

            SelectedFireResistance = _fireResistancesArray?[0];
            SelectedHost = _hostsArray?[0];
            SelectedMaterial = _materialsArray?[0];
            SelectedMepType = _mEPTypesArray?[0];
            SelectedShape = _shapesArray?[0];
            SelectedStructuralDesign = _structuralDesignsArray?[0];

            
            IsDimensionsManually = true;

            _docMepCount =
            _docHostCount =
            _linkMepCount =
            _linkHostCount = 0;

            _dimensionsRoundTo =
            _elevationRoundTo =
            _diameter =
            _height =
            _width =
            _offset = "0";
            _getActiveUIEvent = new GetUIDocumentEvent();
            _getActiveUIDocument = ExternalEvent.Create(_getActiveUIEvent);

            //SetFamilyAndSymbolDefault();
        }

        
        private void SearchInDB()
        {
            if (!IsValidData(false))
            {
                ButtonOKIsEnabled = false;
                return;
            }

            var data = _dBContext.Get(
            SelectedHost.DBId,
            (int)SelectedShape.Shape,
            SelectedMepType.Type,
            (int)SelectedMaterial.SealingMaterialType,
            (int)SelectedStructuralDesign.StructuralDesign,
            SelectedFireResistance.Minutes);

            if (data == default)
            {
                SetFamilyAndSymbolDefault();
                ButtonOKIsEnabled = false;
            }
            else
            {
                FamilyFromDb = data.Family;
                TypeFromDb = data.FamilyType;
                ButtonOKIsEnabled = true;
            }
        }

        private void SetFamilyAndSymbolDefault()
        {
            if (_familyFromDb != _familyNameDefault)
                FamilyFromDb = _familyNameDefault;
            if (_typeFromDb != _familyTypeDefault)
                TypeFromDb = _familyTypeDefault;
        }

        private void TryParseToInt(string value, out string error) =>
            error = int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int ret) ?
                (ret < 0 || ret > int.MaxValue ? $"Допустимые значения: 0-{int.MaxValue}" : string.Empty) :
                "Не является числом";

        private void TryParseToDouble(string value, out string error) =>
            error = double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double ret) ?
                (ret < 0 || ret > double.MaxValue ? $"Допустимые значения: 0-{double.MaxValue}" : string.Empty) :
                "Не является числом";

        private bool CheckHosts()
        {
            if (SelectedHost == null)
            {
                MessageBox.Show("Место установки не выбрано.", "Информация", MessageBoxButton.OK, icon: MessageBoxImage.Information);
                return false;
            }

            return true;
        }

        private bool CheckMepTypes()
        {
            if (SelectedMepType == null)
            {
                MessageBox.Show("Тип коммуникаций не выбран", "Информация", MessageBoxButton.OK, icon: MessageBoxImage.Information);
                return false;
            }
            if (SelectedMepType.AllowCategories.Length == 0)
            {
                MessageBox.Show("Выбор коммуникаций не требуется", "Информация", MessageBoxButton.OK, icon: MessageBoxImage.Information);
                return false;
            }

            return true;
        }

        /// <summary></summary>
        protected virtual bool TryChangeProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            bool result = !ReferenceEquals(oldValue, newValue);
            if (result)
            {
                if (newValue is string text && (string.IsNullOrEmpty(text) || !double.TryParse(text, out double _)))
                {
                    return false;
                }

                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            return result;
        }

        /// <summary></summary>
        protected virtual void ChangeProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!ReferenceEquals(oldValue, newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void GetActiveUIDocument() => _getActiveUIDocument.Raise();

        private readonly ExternalEvent _getActiveUIDocument;
        private readonly GetUIDocumentEvent _getActiveUIEvent;
    }
}
