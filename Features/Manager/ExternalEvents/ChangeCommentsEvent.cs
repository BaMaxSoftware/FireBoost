using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FireBoost.Features.Manager.ExternalEvents
{
    internal class ChangeCommentsEvent : IExternalEventHandler
    {
        public string CurrentValue { get; set; }
        public Parameter Parameter { get; set; }

        public void Execute(UIApplication app)
        {
            if (Parameter != null)
            {
                using (Transaction t = new Transaction(app.ActiveUIDocument.Document))
                {
                    try 
                    {
                        t.Start("Редактировать атрибуты элемента");
                        Parameter.Set(CurrentValue);
                        t.Commit();
                    }
                    catch 
                    {
                        if (t.HasStarted())
                            t.RollBack();
                    }
                }
            }
        }

        public string GetName() => "ChangeComments";
    }
}
