using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using FireBoost.Domain.Data;
using FireBoost.Domain.Entities;
using FireBoost.Features.Selection.Models;
using WPFVisibility = System.Windows.Visibility;

namespace FireBoost.Features.Selection.ViewModels
{
    public partial class SelectionVM : INotifyPropertyChanged
    {
        #region Fields
        private readonly string _familyNameDefault = "<Не найдено>";
        private readonly string _familyTypeDefault = "<Не найден>";
        private readonly DBContext _dBContext = new DBContext();
        private readonly SealingData _sealingData = new SealingData();
        
        private readonly SelectionApp _selectionApp;
        private readonly SealingFireResistance[] _fireResistancesArray;
        private readonly SealingMaterial[] _materialsArray;
        private readonly SealingMEPType[] _mepTypesArray;
        private readonly SealingShape[] _shapesArray;
        private readonly SealingStructuralDesign[] _structuralDesignsArray;
        
        private readonly ExternalEvent _getActiveUIDocument, _selectionDocElementsEvent, _selectionDocHostsEvent, _selectionLinkElementsEvent, _selectionLinkHostsEvent;
        private readonly PickObjectsEventHandler _selectedDocElements, _selectedLinkElements, _selectedDocHosts, _selectedLinkHosts;

        private readonly GetUIDocumentEventHandler _getActiveUIEvent;

        private bool _isJoin, _isDimensionsManually, _buttonOKIsEnabled, _isMepSelectionEnabled;
        private string _dimensionsRoundTo, _elevationRoundTo, _diameter, _height, _width, _offset, _familyFromDb, _typeFromDb, _thickness;
        private string _dimensionsRoundTo, _elevationRoundTo, _diameter, _height, _width, _offset, _familyFromDb, _typeFromDb;
        private ListCollectionView _communicationsTypes;
        private SealingFireResistance _selectedFireResistances;
        private SealingHost _selectedHost;
        private SealingMaterial _selectedMaterial;
        private SealingMEPType _selectedMepType;
        private SealingShape _selectedShape;
        private SealingStructuralDesign _selectedStructuralDesign;
        
        private ICommand _docElementSelection, _docHostSelection, _linkElementSelection, _linkHostSelection;
        #endregion Fields

        /// <summary></summary>
        public PickObjectsEventHandler SelectedDocElements { get => _selectedDocElements; }
        /// <summary></summary>
        public PickObjectsEventHandler SelectedLinkElements { get => _selectedLinkElements; }
        /// <summary></summary>
        public PickObjectsEventHandler SelectedDocHosts { get => _selectedDocHosts; }
        /// <summary></summary>
        public PickObjectsEventHandler SelectedLinkHosts { get => _selectedLinkHosts; }
        #region Properties
        
        public bool IsMepSelectionEnabled
        public bool HasInsulation
            get => _isMepSelectionEnabled;
            set => ChangeProperty(ref _isMepSelectionEnabled, value);
        }

        }
        /// <summary></summary>
        public bool IsJoin 
        {
            get => _isJoin;
            set => ChangeProperty(ref _isJoin, value); 
        }

        /// <summary></summary>
        public string Thickness 
        { 
            get => _thickness;
            set => ChangeProperty(ref _thickness, value);
        }

        /// <summary></summary>
        public string FamilyFromDb 
        { 
            get => _familyFromDb;
            set => ChangeProperty(ref _familyFromDb, value);
        }
        /// <summary></summary>
        public string TypeFromDb 
        { 
            get => _typeFromDb;
            set => ChangeProperty(ref _typeFromDb, value);
        }
        /// <summary></summary>
        public bool ButtonOKIsEnabled 
        { 
            get => _buttonOKIsEnabled;
            set => ChangeProperty(ref _buttonOKIsEnabled, value);
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
                IsMepSelectionEnabled = !_isDimensionsManually;
            }
        }
        /// <summary></summary>
        public SealingMEPType SelectedMepType
        {
            get => _selectedMepType;
            set
            {
                if (TryChangeProperty(ref _selectedMepType, value))
                    if (_selectedLinkElements.RefList.Count > 0)
                        _selectedLinkElements.RefListClear();
                    if (_selectedDocElements.RefList.Count > 0)
                        _selectedDocElements.RefListClear();
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
                    if (_selectedLinkHosts.RefList.Count > 0)
                        _selectedLinkHosts.RefListClear();
                    if (_selectedDocHosts.RefList.Count > 0)
                        _selectedDocHosts.RefListClear();
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
        public SealingMEPType[] MEPTypesArray => _mepTypesArray;
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
            if (CheckMEPs())
            var view = _selectionApp.GetMainWindow().Visibility;
                _selectedDocElements.SetSelectionFilter(_selectedMepType.AllowedCategories);
                _selectionDocElementsEvent.Raise();
                    _docElementReferences,
                    GetActiveUIEvent.ActiveUIDocument,
                    false);
            }
        }));

        /// <summary></summary>
        public ICommand DocHostSelection => _docHostSelection ?? (_docHostSelection = new VMCommand(obj =>
                _selectedDocHosts.SetSelectionFilter(_selectedHost.AllowedCategories);
                _selectionDocHostsEvent.Raise();
                    _docHostReferences,
                    GetActiveUIEvent.ActiveUIDocument,
                    true);
            }
        }));

            if (CheckMEPs())
        public ICommand LinkElementSelection => _linkElementSelection ?? (_linkElementSelection = new VMCommand(obj =>
                _selectedLinkElements.SetSelectionFilter(_selectedMepType.AllowedCategories);
                _selectionLinkElementsEvent.Raise();
                    ObjectType.LinkedElement,
                    new SelectionFilter(_selectedMepType.AllowCategories, true, GetActiveUIEvent.ActiveDocument),
                    _linkElementReferences,
                    GetActiveUIEvent.ActiveUIDocument,
                    false);
            }
        }));

                _selectedLinkHosts.SetSelectionFilter(_selectedHost.AllowedCategories);
                _selectionLinkHostsEvent.Raise();
                    ObjectType.LinkedElement,
                    new SelectionFilter(_selectedHost.BuiltInCategory, true, GetActiveUIEvent.ActiveDocument),
                    _linkHostReferences,
                    GetActiveUIEvent.ActiveUIDocument,
                    true);
            }
        }));
        #endregion Properties

        /// <summary></summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary></summary>
        public SelectionVM(SelectionApp app)
        {
            _selectionApp = app;

            _selectedLinkElements = new PickObjectsEventHandler(ObjectType.LinkedElement, false);
            _selectionLinkElementsEvent = ExternalEvent.Create(_selectedLinkElements);

            _selectedLinkHosts = new PickObjectsEventHandler(ObjectType.LinkedElement, true);
            _selectionLinkHostsEvent = ExternalEvent.Create(_selectedLinkHosts);

            _selectedDocElements = new PickObjectsEventHandler(ObjectType.Element, false);
            _selectionDocElementsEvent = ExternalEvent.Create(_selectedDocElements);
            
            _selectedDocHosts = new PickObjectsEventHandler(ObjectType.Element, true);
            _selectionDocHostsEvent = ExternalEvent.Create(_selectedDocHosts);

            _hostsArray = _sealingData.CreateHostsTypesArray();
            
            SelectedFireResistance = _fireResistancesArray[0];
            SelectedHost = _hostsArray[0];
            SelectedMaterial = _materialsArray[0];
            SelectedMepType = _mepTypesArray[0];
            SelectedShape = _shapesArray[0];
            SelectedStructuralDesign = _structuralDesignsArray[0];
            
            SelectedMepType = _mEPTypesArray?[0];
            SelectedShape = _shapesArray?[0];
            SelectedStructuralDesign = _structuralDesignsArray?[0];

            HasInsulation =
            IsDimensionsManually = true;

            _dimensionsRoundTo =
            _elevationRoundTo =
            _diameter =
            _height =
            _width =
            _thickness =
            _offset = "0";
            _getActiveUIEvent = new GetUIDocumentEventHandler();
            _getActiveUIDocument = ExternalEvent.Create(_getActiveUIEvent);
        }
        

        private void SearchInDB()
        {
            if (!IsValidData(false))
            {
                _selectedHost.DBId,
                (int)_selectedShape.Shape,
                _selectedMepType.Type,
                (int)_selectedMaterial.SealingMaterialType,
                (int)_selectedStructuralDesign.StructuralDesign,
                _selectedFireResistances.Minutes);
            (int)SelectedShape.Shape,
            SelectedMepType.Type,
            (int)SelectedMaterial.SealingMaterialType,
            (int)SelectedStructuralDesign.StructuralDesign,
            SelectedFireResistance.Minutes);

            if (data == default)
            {
                if (_familyFromDb != _familyNameDefault)
                    FamilyFromDb = _familyNameDefault;
                if (_typeFromDb != _familyTypeDefault)
                    TypeFromDb = _familyTypeDefault;
                ButtonOKIsEnabled = false;
            }
            else
            {
                FamilyFromDb = data.Family;
                TypeFromDb = data.FamilyType;
                ButtonOKIsEnabled = true;
            }
        }

        private bool CheckHosts()
        {
            if (SelectedHost == null)
            {
                MessageBox.Show("Место установки не выбрано.", "Информация", MessageBoxButton.OK, icon: MessageBoxImage.Information);
                return false;
            }

            return true;
        }

        private bool CheckMEPs()
        {
            if (SelectedMepType.AllowedCategories.Length == 0 || IsDimensionsManually)
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
        private bool TryChangeProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
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
        private void ChangeProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!ReferenceEquals(oldValue, newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
