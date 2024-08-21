using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FireBoost.Features.Selection.ViewModels;

namespace FireBoost.Features.Selection.Models
{
    internal class CreatorWithoutMEP : CreatorBase
    {
        public CreatorWithoutMEP(SelectionVM viewModel, Document activeDoc, FamilySymbol familySymbol, (double Height, double Width, double Diameter) dimensions, double offset, (int dimensions, int elevation) roundTo) 
            : base(viewModel, activeDoc, familySymbol, dimensions, offset, roundTo)
        { }

        public void CreateInstances()
        {
            FamilyInstance instance;
            (Element, Transform, XYZ)[] refsHosts = CollectHosts();


            foreach ((Element hostElement, Transform hostTransform, XYZ globalPoint) host in refsHosts)
            {
                _currentHost = host;
                if (host.hostElement == default || !host.hostElement.IsValidObject || host.hostElement is FamilyInstance) continue;
                Level level = GetNearestLevel(host.globalPoint.Z);
                instance = Transactions.CreateNewInstance(_familySymbol, host.globalPoint, level);
                ChangeInstanceElevation(ref instance, host.globalPoint.Z - level.Elevation);
                ChangeSize(ref instance);
            }
        }
    }
}
