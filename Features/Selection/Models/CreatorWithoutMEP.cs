using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Settings;
using System.Linq;

namespace FireBoost.Features.Selection.Models
{
    internal class CreatorWithoutMEP : CreatorBase
    {
        public CreatorWithoutMEP(
            SelectionVM viewModel,
            SettingsVM _settingsViewModel,
            Document activeDoc,
            FamilySymbol familySymbol,
            (double Height, double Width, double Diameter) dimensions,
            (double, double) offset,
            (int dimensions, int elevation) roundTo)
            : base(viewModel, _settingsViewModel, activeDoc, familySymbol, dimensions, offset, roundTo)
        { }

        override public void CreateInstances()
        {
            if (!TryCollectHosts(out (Element Element, Transform Transform, XYZ Location)[] refsHosts))
                return;

            FamilyInstance instance;
            foreach ((Element hostElement, Transform hostTransform, XYZ globalPoint) host in refsHosts)
            {
                CurrentHost = host;
                if (host.hostElement == default || !host.hostElement.IsValidObject) continue;
                Level level = GetNearestLevel(host.globalPoint.Z);
                instance = Transactions.CreateNewInstance(FamilySymbol, host.globalPoint, level);
                ChangeInstanceElevation(instance, host.globalPoint.Z - level.Elevation);
                ChangeSize(instance);
                Rotate(instance, out XYZ orient);
                ChangeProjectParameters(instance);
                Move(instance, orient);
            }
        }
    }
}
