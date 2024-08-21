using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using FireBoost.Domain.Data;

namespace FireBoost.Features.Manager
{
    internal class ManagerVM : INotifyPropertyChanged
    {
        private Document _document;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SealingElement> _sealingCollection;
        public ObservableCollection<SealingElement> SealingCollection 
        { 
            get => _sealingCollection;
            private set => TryChangeProperty(ref _sealingCollection, value);
        }

        public ManagerVM(Document document) 
        {
            _document = document;
            SealingCollection = new ObservableCollection<SealingElement>();
            var families = new DBContext().GetFamilies();
            
            var elements = new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(x => families.Contains(x.Symbol.FamilyName))
                .ToArray();
            
            Stack<string> familiesStack = new Stack<string>();

            foreach (var e in elements)
            {
                if (familiesStack.Count == 0 || !familiesStack.Contains(e.Symbol.FamilyName))
                {
                    familiesStack.Push(e.Symbol.FamilyName);
                }
            }
            var fs = familiesStack.OrderBy(x=>x).ToArray();
            foreach (var e in elements)
            {
                SealingCollection.Add(new SealingElement(e, document, fs));
            }

        }

        protected virtual bool TryChangeProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
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
