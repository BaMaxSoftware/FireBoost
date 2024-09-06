using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace FireBoost.Features.Selection.Models
{
    internal class SelectionFilter : ISelectionFilter
    {
        private readonly int[] _builtInCategoryIds;
        private readonly int _builtInCategoryId = -1;
        private readonly bool _linkChecking;
        private Document _document;
        private Document _linkDocument;
        private Element _currentElement;
        private RevitLinkInstance _linkInstance;

        public SelectionFilter(BuiltInCategory[] builtInCategories, bool linkChecking, Document document = null)
        {
            _linkChecking = linkChecking;
            _document = document;
            _builtInCategoryIds = builtInCategories.Select(x => (int)x).ToArray();
        }
        public SelectionFilter(BuiltInCategory builtInCategory, bool linkChecking, Document document = null)
        {
            _linkChecking = linkChecking;
            _document = document;
            _builtInCategoryId = (int)builtInCategory;
        }

        public void SetDocument(Document doc) => _document = doc;

        public virtual bool AllowElement(Element elem) => elem != null && _linkChecking & elem is RevitLinkInstance || CheckElementCategory(elem);


        public virtual bool AllowReference(Reference reference, XYZ position)
        {
            bool ret = false;
            if (reference.LinkedElementId != ElementId.InvalidElementId)
            {
                _currentElement = _document.GetElement(reference.ElementId);
                if (_currentElement is RevitLinkInstance)
                {
                    _linkInstance = _currentElement as RevitLinkInstance;
                    _linkDocument = _linkInstance.GetLinkDocument();
                    ret = CheckElementCategory(_linkDocument.GetElement(reference.LinkedElementId));
                }
            }
            else
            {
                ret = CheckElementCategory(_document.GetElement(reference.ElementId));
            }

            return ret;
        }

        private bool CheckElementCategory(Element element)
        {
            if (element != null)
            {
                if (element.Category is Category category )
                {
                    if (_builtInCategoryId == -1)
                    {
                        if (_builtInCategoryIds.Contains(category.Id.IntegerValue))
                        { 
                            switch (element)
                            {
                                case Wall wall:
                                    return wall.CurtainGrid == null;
                                case CurtainSystem cs:
                                    return cs.CurtainGrids.Size == 0;
                                case Panel panel:
                                    return panel.Host != null && _builtInCategoryIds.Contains(panel.Host.Category.Id.IntegerValue);
                                default: return true;
                            }
                        }
                    }
                    else
                    {
                        return _builtInCategoryId == category.Id.IntegerValue;
                    }
                }
            }
            return false;
        }
    }
}
