using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FireBoost.Features.Manager.ExternalEvents
{
    internal class ChangeFamilyEvent : IExternalEventHandler
    {
        public Element CurrentElement { get; set; }
        public ElementId CurrentValue { get; set; }

        public void Execute(UIApplication app)
        {
            using (Transaction t = new Transaction(app.ActiveUIDocument.Document))
            {
                try
                {
                    t.Start("Изменение типа");
                    CurrentElement.ChangeTypeId(CurrentValue);
                    t.Commit();
                }
                catch
                {
                    if (t.HasStarted())
                        t.RollBack();
                }
            }
        }

        public string GetName() => "ChangeFamily";
    }
}
