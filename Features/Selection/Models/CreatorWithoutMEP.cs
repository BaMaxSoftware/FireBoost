using Autodesk.Revit.DB;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Settings;

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
            double offset,
            (int dimensions, int elevation) roundTo)
            : base(viewModel, _settingsViewModel, activeDoc, familySymbol, dimensions, offset, roundTo)
        { }

        public void CreateInstances()
        {
            FamilyInstance instance;
            (Element, Transform, XYZ)[] refsHosts = CollectHosts();


            foreach ((Element hostElement, Transform hostTransform, XYZ globalPoint) host in refsHosts)
            {
                CurrentHost = host;
                if (host.hostElement == default || !host.hostElement.IsValidObject || host.hostElement is FamilyInstance) continue;
                Level level = GetNearestLevel(host.globalPoint.Z);
                instance = Transactions.CreateNewInstance(FamilySymbol, host.globalPoint, level);
                ChangeInstanceElevation(instance, host.globalPoint.Z - level.Elevation);
                ChangeSize(instance);
                ChangeProjectParameters(instance);
                Move(instance);
            }
        }
    }
}
