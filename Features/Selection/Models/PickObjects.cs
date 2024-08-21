using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace FireBoost.Features.Selection.Models
{
    internal class PickObjects
    {
        private readonly string _mepSelection = "Выберите линейные элементы инженерных коммуникаций (Воздуховоды/Трубы/Лотки/Короба).";
        private readonly string _hostSelection = "Выберите элементы основы (Стены/Перекрытия).";

        public IList<Reference> Select(ObjectType objectType, ISelectionFilter selectionFilter, IList<Reference> refList, UIDocument uiDoc, bool isHostObject)
        {
            try
            {
                refList = uiDoc.Selection.PickObjects(
                    objectType,
                    selectionFilter,
                    isHostObject ? _hostSelection : _mepSelection,
                    refList ?? new List<Reference>());
            }
            catch { }
            return refList;
        }
    }
}
