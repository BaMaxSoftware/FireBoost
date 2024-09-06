using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Visibility = System.Windows.Visibility;

namespace FireBoost.Features.Selection.Models
{
    /// <summary></summary>
    public class PickObjectsEventHandler : IExternalEventHandler, INotifyPropertyChanged
    {
        private readonly string _mepSelection = "Выберите линейные элементы инженерных коммуникаций (Воздуховоды/Трубы/Лотки/Короба).";
        private readonly string _hostSelection = "Выберите элементы основы (Стены/Перекрытия).";
        private readonly bool _isHostObject;

        private ObjectType _objectType;
        private SelectionFilter _selectionFilter;
        private IList<Reference> _refList;
       
        /// <summary></summary>
        public int Count => _refList.Count;

        /// <summary></summary>
        public IList<Reference> RefList 
        {
            get => _refList;
            private set 
            {
                if (!ReferenceEquals(_refList, value))
                {
                    _refList = value;
                    ChangeProperty();
                    ChangeProperty(nameof(this.Count));
                }
            }
        }

        /// <summary></summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary></summary>
        /// <param name="objectType"></param>
        /// <param name="isHostObject"></param>
        public PickObjectsEventHandler(ObjectType objectType, bool isHostObject)
        { 
            _objectType = objectType;
            _isHostObject = isHostObject;
            RefList = new List<Reference>();
        }


        /// <summary></summary>
        public string GetName() => nameof(PickObjectsEventHandler);

        /// <summary></summary>
        public void Execute(UIApplication app)
        {
            SingletonWindow.SetVisibility(Visibility.Hidden);
            _selectionFilter.SetDocument(app.ActiveUIDocument.Document);

            try
            {
                RefList = app.ActiveUIDocument.Selection.PickObjects(
                    _objectType,
                    _selectionFilter,
                    _isHostObject ? _hostSelection : _mepSelection,
                    _refList ?? new List<Reference>());
            }
            catch { }

            SingletonWindow.SetVisibility(Visibility.Visible);
        }

        /// <summary></summary>
        public void RefListClear() => RefList = new List<Reference>();

        /// <summary></summary>
        public void SetSelectionFilter(BuiltInCategory[] bic, ObjectType type = ObjectType.Nothing)
        {
            if (type != ObjectType.Nothing)
                _objectType = type;

            _selectionFilter = new SelectionFilter(bic, _objectType == ObjectType.LinkedElement);
        }

        private void ChangeProperty([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
