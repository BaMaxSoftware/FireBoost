using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FireBoost.Features.Selection.Models
{
    internal class GetUIDocumentEvent : IExternalEventHandler
    {
        public UIDocument ActiveUIDocument { get; private set; }
        public Document ActiveDocument { get; private set; }

        public void Execute(UIApplication app)
        {
            ActiveUIDocument = app.ActiveUIDocument;
            ActiveDocument = ActiveUIDocument.Document;
        }

        public string GetName() => "GetUIDocumentEvent";
    }
}
