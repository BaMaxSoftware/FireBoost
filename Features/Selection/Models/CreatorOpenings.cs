using Autodesk.Revit.DB;
using FireBoost.Domain.Entities;

namespace FireBoost.Features.Selection.Models
{
    internal class CreatorOpenings : CreatorBase
    {
        public CreatorOpenings(CreationData creationData) : base(creationData)
        {
        }

        public void CreateInstances()
        {
            FamilyInstance instance;
            (Element, Transform)[] elements = CollectElements();
            LocationPoint lp;
            XYZ location;
            Level level;
            foreach ((Element Instance, Transform Transform) element in elements)
            {
                if (element.Instance == default || !element.Instance.IsValidObject || element.Instance is FamilyInstance)
                    continue;

                if (!(element.Instance.Location is LocationPoint))
                    continue;

                CurrentElement = element;
                lp = element.Instance.Location as LocationPoint;
                location = element.Transform == null ? lp.Point : element.Transform.OfPoint(lp.Point);
                level = GetNearestLevel(location.Z);
                instance = Transactions.CreateNewInstance(CreationData.FamilySymbol, location, level);
                ChangeInstanceElevation(instance, location.Z - level.Elevation);
                ChangeSize(instance);
                ChangeProjectParameters(instance);
                Move(instance);
            }
        }
    }
}
