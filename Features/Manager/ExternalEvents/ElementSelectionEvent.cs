using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FireBoost.Features.Manager.ExternalEvents
{
    internal class ElementSelectionEvent : IExternalEventHandler
    {
        private readonly Element _element;

        public ElementSelectionEvent(Element element) => _element = element;

        public void Execute(UIApplication app)
        {
            if (_element == null || !_element.IsValidObject) return;
            using (Transaction t = new Transaction(app.ActiveUIDocument.Document))
            {
                try
                {
                    var uidoc = app.ActiveUIDocument;
                    t.Start("Выбор элемента");
                    uidoc.ShowElements(_element);
                    uidoc.Selection.SetElementIds(new ElementId[] { _element.Id });
                    uidoc.RefreshActiveView();
                    t.Commit();
                }
                catch
                {
                    if (t.HasStarted())
                        t.RollBack();
                }
            }
        }

        public string GetName() => "ElementSelection";
    }
}
