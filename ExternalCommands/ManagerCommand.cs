using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Features.Manager;

namespace FireBoost.ExternalCommands
{
    /// <summary></summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ManagerCommand : IExternalCommand
    {
        /// <summary>
        /// Overload this method to implement and external command within Revit.
        /// </summary>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (!License.Licenser.CheckPassword(GetType().Name))
            {
                return Result.Failed;
            }
            new ManagerApp(commandData.Application.ActiveUIDocument.Document).Run();
            return Result.Succeeded;
        }
    }
}
