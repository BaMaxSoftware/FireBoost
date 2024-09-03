using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FireBoost.Features.Selection.Models
{
    internal class CreatorWithMEP : CreatorBase
    {
        private readonly SizeChanger _sizeChanger = new SizeChanger();
        private SolidCurveIntersectionOptions IntersectionOptions { get; } = new SolidCurveIntersectionOptions()
        {
            ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
        };
        
        

        public CreatorWithMEP(
            SelectionVM viewModel, 
            SettingsVM _settingsViewModel, 
            Document activeDoc, 
            FamilySymbol familySymbol, 
            (double Height, double Width, double Diameter) dimensions,
            double offset,
            (int dimensions, int elevation) roundTo)
            : base(viewModel, _settingsViewModel, activeDoc, familySymbol, dimensions, offset, roundTo)
        {
        }

        public void CreateInstances()
        {
            (Element, Transform)[] refsElements = CollectElements();
            (Element, Transform, XYZ)[] refsHosts = CollectHosts();
            JoinWallOpenings jo;
            Line hostLine, intersectionLine;
            FamilyInstanceList = new List<FamilyInstance>();
            foreach (var host in refsHosts)
            {
                CurrentHost = host;
                if (CurrentHost.Element == default || !CurrentHost.Element.IsValidObject || CurrentHost.Element is FamilyInstance) continue;

                Solid hostSolid = GetSolid(host);
                if (hostSolid == null) continue;
                foreach (var element in refsElements)
                {
                    CurrentElement = element;
                    if (CurrentElement.Instance == default || !CurrentElement.Instance.IsValidObject) continue;

                    Curve elementCurve = GetCurve(element);
                    if (elementCurve == null) continue;

                    SolidCurveIntersection intersections = hostSolid.IntersectWithCurve(elementCurve, IntersectionOptions);
                    if (intersections.SegmentCount == 0) continue;

                    foreach (Curve intersection in intersections)
                    {
                        if (intersection is Line)
                        {
                            intersectionLine = intersection as Line;

                            XYZ point0 = intersectionLine.GetEndPoint(0);
                            XYZ point1 = intersectionLine.GetEndPoint(1);
                            double slopeOffset = 0d;
                            if (CurrentHost.Element is Wall)
                            {
                                Wall wall = CurrentHost.Element as Wall;
                                Curve wallCurve = (wall.Location as LocationCurve).Curve;
                                if (wallCurve is Line)
                                {
                                    // todo : add slope
                                    hostLine = intersectionLine;
                                }
                                else continue;
                            }
                            else
                            {
                                hostLine = intersectionLine;
                            }
                            
                            Level currentLevel = null;
                            double elevation = 0;
                            switch (SelectionViewModel.SelectedHost.DBId)
                            {
                                case 1:
                                    currentLevel = GetLevel(CurrentElement.Instance, CurrentElement.Transform);
                                    elevation = (point0.Z + point1.Z) / 2 - currentLevel?.Elevation ?? 0;
                                    break;
                                case 2:
                                    if (CurrentHost.Element is DirectShape dShape)
                                    {
                                        Transform transform = null;
                                        foreach(var g in dShape.get_Geometry(Options))
                                        {
                                            if (g is GeometryInstance gi)
                                            { 
                                                transform = CurrentHost.Transform == null ? CurrentHost.Transform : CurrentHost.Transform.Multiply(gi.Transform);
                                                currentLevel = GetNearestLevel(transform.Origin.Z);
                                                elevation = transform.Origin.Z - currentLevel?.Elevation ?? 0;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var el = CurrentHost.Element.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble();
                                        currentLevel = GetLevel(ActiveDoc.GetElement(CurrentHost.Element.LevelId) as Level);
                                        elevation = CurrentHost.Element.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble() - currentLevel?.ProjectElevation ?? 0;
                                        if (CurrentHost.Element is Floor floor)
                                        {
                                            var pp = floor.FloorType.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM);
                                            if (pp != null)
                                            {
                                                var thickness = pp.AsDouble();
                                                elevation -= thickness * .5;
                                            }
                                        }
                                    }
                                    break;
                                default: continue;
                            }
                            if (currentLevel == default || !currentLevel.IsValidObject) continue;

                            FamilyInstance newInstance = Transactions.CreateNewInstance(
                                FamilySymbol,
                                hostLine.Evaluate(0.5, true),
                                currentLevel);

                            if (newInstance == default) continue;

                            if (!(CurrentHost.Element is DirectShape))
                            { 
                                ChangeInstanceElevation(newInstance, elevation);
                            }
                            ChangeSize(newInstance, slopeOffset);
                            Rotate(newInstance);
                            Transactions.ChangeSelectedParams(ActiveDoc, _sizeChanger.SetOtherParams(newInstance, CurrentElement.Instance as MEPCurve, SelectionViewModel.SelectedShape.Shape));
                            ChangeProjectParameters(newInstance);

                            if (newInstance != default && newInstance.IsValidObject)
                            {
                                if (FamilyInstanceList.FirstOrDefault(x => x.IsValidObject && x.Id == newInstance.Id) == default)
                                {
                                    FamilyInstanceList.Add(newInstance);
                                }
                            }
                        }
                    }
                }

                if (SelectionViewModel.IsJoin && CurrentHost.Element != default && CurrentHost.Element is Wall)
                {
                    jo = new JoinWallOpenings(SettingsViewModel, ActiveDoc, CurrentHost.Element as Wall, RoundTo);
                    jo.Join(FamilyInstanceList.ToArray(), FamilySymbol, CurrentHost);
                }
                Transactions.Regenerate();
            }
        }

        private Level GetLevel(Element instance, Transform trans = null)
        {
            Level result;
            switch (instance.GetType().Name)
            {
                case nameof(Duct):
                case nameof(Pipe):
                case nameof(CableTray):
                case nameof(Conduit):
                    if ((instance as MEPCurve).ReferenceLevel is Level refLvl)
                    {
                        result = trans != null ? GetNearestLevel(refLvl.Elevation) : refLvl;
                    }
                    else
                    {
                        result = null;
                    }
                    break;
                default: 
                    result = null;
                    break;
            }

            return result;
        }

        private Level GetLevel(Level hostLevel)
        {
            Level[] levels = new FilteredElementCollector(ActiveDoc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .ToArray();

            return levels
                .OrderBy(level => level.ProjectElevation)
                .FirstOrDefault(x => Math.Round(x.ProjectElevation, 3) >= Math.Round(hostLevel.ProjectElevation, 3));
        }

        private Solid GetSolid((Element Element, Transform Transform, XYZ globalPoint) data)
        {
            Solid solid = default;

            GeometryElement geometry = data.Element.get_Geometry(Options);
            if (data.Transform != null)
            {
                geometry = geometry.GetTransformed(data.Transform);
            }
            foreach (GeometryObject geometryObject in geometry)
            {
                if (geometryObject is GeometryElement gElement)
                {
                    foreach (GeometryObject item in gElement)
                    {
                        if (item is Solid newSolid)
                        {
                            if (solid == default)
                                solid = newSolid;
                            else
                            {
                                if (solid.Volume < newSolid.Volume)
                                    solid = newSolid;
                            }
                        }
                    }
                }
                else if (geometryObject is Solid)
                {
                    Solid solid2 = geometryObject as Solid;

                    if (solid == default)
                        solid = solid2;
                    else
                    {
                        if (solid.Volume < solid2.Volume)
                            solid = solid2;
                    }

                }
            }

            return solid;
        }
        private Curve GetCurve((Element Element, Transform Transform) data)
        {
            Curve curve = default;

            if (data.Element.Location is LocationCurve lc)
            {
                curve = lc.Curve;
                if (data.Transform != null)
                {
                    curve = curve.CreateTransformed(data.Transform);
                }
            }

            return curve;
        }

        private (Element, Transform)[] CollectElements()
        {
            (Element, Transform)[] elements = new (Element, Transform)[SelectionViewModel.DocElementReferences.Count + SelectionViewModel.LinkElementReferences.Count];
            Element element;
            int count = 0;
            if (SelectionViewModel.DocElementReferences.Count > 0)
            {
                foreach (Reference host in SelectionViewModel.DocElementReferences)
                {
                    elements[count] = (ActiveDoc.GetElement(host.ElementId), null);
                    count++;
                }
            }
            if (SelectionViewModel.LinkElementReferences.Count > 0)
            {
                foreach (Reference host in SelectionViewModel.LinkElementReferences)
                {
                    element = ActiveDoc.GetElement(host.ElementId);
                    if (element is RevitLinkInstance link && host.LinkedElementId != ElementId.InvalidElementId)
                    {
                        elements[count] = (link.GetLinkDocument().GetElement(host.LinkedElementId), (element as RevitLinkInstance).GetTotalTransform());
                        count++;
                    }
                }
            }

            return elements;
        }
    }
}
