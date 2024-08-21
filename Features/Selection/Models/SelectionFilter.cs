using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace FireBoost.Features.Selection.Models
{
    internal class SelectionFilter : ISelectionFilter
    {
        private readonly Document _document;
        private readonly List<BuiltInCategory> _builtInCategories = new List<BuiltInCategory>();
        private readonly BuiltInCategory _builtInCategory = BuiltInCategory.INVALID;
        private readonly bool _linkChecking;
        public SelectionFilter(IEnumerable<BuiltInCategory> builtInCategories, bool linkChecking, Document document = null)
        {
            _linkChecking = linkChecking;
            _document = document;
            _builtInCategories.AddRange(builtInCategories);
        }
        public SelectionFilter(BuiltInCategory builtInCategories, bool linkChecking, Document document = null)
        {
            _linkChecking = linkChecking;
            _document = document;
            _builtInCategory = builtInCategories;
        }

        public virtual bool AllowElement(Element elem)
        {
            if (elem != null)
            {
                if (_linkChecking & elem is RevitLinkInstance)
                {
                    return true;
                }
                else
                {
                    return CheckElementCategory(elem);
                }
            }
            return false;
        }

        private Document linkDocument;

        public virtual bool AllowReference(Reference reference, XYZ position)
        {
            bool ret = false;
            if (reference.LinkedElementId != ElementId.InvalidElementId)
            {
                if (_document.GetElement(reference.ElementId) is RevitLinkInstance link)
                {
                    linkDocument = link.GetLinkDocument();
                    ret = CheckElementCategory(linkDocument.GetElement(reference.LinkedElementId));
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
                if (element.Category is Category category)
                {
                    if (_builtInCategory == BuiltInCategory.INVALID)
                    {
                        return _builtInCategories.Any(x => (int)x == category.Id.IntegerValue);
                    }
                    else
                    {
                        return (int)_builtInCategory == category.Id.IntegerValue;
                    }
                }
            }

            return false;
        }
    }

    internal static class ReferenceExtension
    {
        public static Element GetElement(this Reference reference, Document document)
        {
            Element elementReference = document.GetElement(reference.ElementId);
            if (elementReference is RevitLinkInstance instance)
            {
                return instance.GetLinkDocument().GetElement(reference.LinkedElementId);
            }
            else
            {
                return elementReference;
            }
        }
    }
}
