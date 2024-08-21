using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Domain.Entities;
using FireBoost.Features.Manager.ExternalEvents;

namespace FireBoost.Features.Manager
{
    /// <summary></summary>
    public class SealingElement : INotifyPropertyChanged
    {
        private readonly Document _doc;
        private readonly ChangeCommentsEvent _changeComments;
        private readonly ChangeTypeEvent _changeType;

        private string _comments;
        private FamilySymbol _currentType;
        private Family _currentFamily;
        private FamilySymbol[] _validTypes;
        private ICommand _select;

        /// <summary></summary>
        public Element Element { get; }
        /// <summary></summary>
        public string Comments 
        {
            get => _comments;
            set
            { 
                if (_comments != value)
                { 
                    var p = Element.LookupParameter("Статус");
                    if (p != null && !p.IsReadOnly && p.StorageType == StorageType.String)
                    {
                        TryChangeProperty(ref _comments, value);
                        _changeComments.Parameter = p;
                        _changeComments.CurrentValue = value;
                        ExComment.Raise();
                    }
                }
            }
        }
        /// <summary></summary>
        public FamilySymbol CurrentType 
        {
            get => _currentType;
            set
            {
                if (TryChangeProperty(ref _currentType, value))
                { 
                    _changeType.CurrentValue = _currentType.Id;
                    _changeType.CurrentElement = Element;
                    ExChangeType.Raise();
                }
            }
        }
        /// <summary></summary>
        public Family CurrentFamily 
        {
            get => _currentFamily;
            set
            {
                if (TryChangeProperty(ref _currentFamily, value))
                {
                    ValidTypes = GetValidFamilySymbols();
                    _currentType = _doc.GetElement(_currentFamily.GetTypeId()) as FamilySymbol;
                    CurrentType = _currentType;
                }
            }
        }
        /// <summary></summary>
        public FamilySymbol[] ValidTypes 
        {
            get => _validTypes;
            set => TryChangeProperty(ref _validTypes, value);
        }
        /// <summary></summary>
        public Family[] ValidFamilies { get; set; }
        /// <summary></summary>
        public string[] States { get; }
        /// <summary></summary>
        public ICommand Select => _select ?? (_select = new VMCommand(obj =>
        {
            ExEvent.Raise();
        }));
        /// <summary></summary>
        public ExternalEvent ExEvent { get; }
        
        /// <summary></summary>
        public ExternalEvent ExComment { get; }
        /// <summary></summary>
        public ExternalEvent ExChangeType { get; }

        /// <summary></summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        
        /// <summary></summary>
        public SealingElement(Element element, Document doc, string[] families)
        {
            _doc = doc;
            Element = element;
            _comments = element.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
            Comments = _comments;
            _currentType = (element as FamilyInstance)?.Symbol;
            CurrentType = _currentType;
            _currentFamily = _currentType?.Family;
            CurrentFamily = _currentFamily;
            ValidTypes = GetValidFamilySymbols();

            List<Family> f = new List<Family>();
            var arr = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType();
            foreach (var ffs in arr)
            {
                if (ffs is FamilySymbol symbol && families.Contains(symbol.FamilyName))
                {
                    if (f.Any(x => x.Id == symbol.Family.Id)) continue;

                    f.Add(symbol.Family);
                }
            }

            ValidFamilies = f.ToArray();

            States = new string[]
            {
                "Не проверено",
                "Проверено"
            };
            
            ExEvent = ExternalEvent.Create(new ElementSelectionEvent(Element));
            _changeComments = new ChangeCommentsEvent();
            ExComment = ExternalEvent.Create(_changeComments);
            _changeType = new ChangeTypeEvent();
            ExChangeType = ExternalEvent.Create(_changeType);
        }

        private FamilySymbol[] GetValidFamilySymbols() => _currentFamily?.GetFamilySymbolIds()?
                .Select(x => _doc.GetElement(x))?
                .Cast<FamilySymbol>()
                .OrderBy(x => x.Name)
                .ToArray();

        private bool TryChangeProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            bool result = !ReferenceEquals(oldValue, newValue);
            if (result)
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            return result;
        }
    }
}
