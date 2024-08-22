using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FireBoost.Features.Selection.Models
{
    internal class CreatorWithMEP : CreatorBase
    {
        private SizeChanger _sizeChanger = new SizeChanger();
        private SolidCurveIntersectionOptions _intersectionOptions { get; } = new SolidCurveIntersectionOptions()
        {
            ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
        };
        private Options _options { get; } = new Options()
        {
            DetailLevel = ViewDetailLevel.Fine,
            IncludeNonVisibleObjects = true,
            ComputeReferences = true
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

                    SolidCurveIntersection intersections = hostSolid.IntersectWithCurve(elementCurve, _intersectionOptions);
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

                            FamilyInstance newInstance;
                            Level currentLevel;
                            XYZ location;
                            double elevation;
                            switch (SelectionViewModel.SelectedHost.BuiltInCategory)
                            {
                                case BuiltInCategory.OST_Walls:
                                    currentLevel = GetLevel(CurrentElement.Instance, CurrentElement.Transform);
                                    location = hostLine.GetEndPoint(0) + (hostLine.Length / 2 * hostLine.Direction);
                                    elevation = (point0.Z + point1.Z) / 2 - currentLevel.Elevation;
                                    break;
                                case BuiltInCategory.OST_Floors:
                                    currentLevel = GetLevel(ActiveDoc.GetElement(CurrentHost.Element.LevelId) as Level);
                                    location = point0.Z > point1.Z ? point0 : point1;
                                    elevation = CurrentHost.Element.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble() - currentLevel.ProjectElevation;
                                    break;
                                default: continue;
                            }
                            if (currentLevel == default || !currentLevel.IsValidObject || location == null) continue;

                            newInstance = Transactions.CreateNewInstance(
                                        FamilySymbol,
                                        hostLine.GetEndPoint(0) + (hostLine.Length / 2 * hostLine.Direction),
                                        currentLevel);
                            if (newInstance == default) continue;

                            ChangeInstanceElevation(ref newInstance, elevation);
                            ChangeSize(ref newInstance, slopeOffset);
                            Transactions.ChangeSelectedParams(ActiveDoc, _sizeChanger.SetOtherParams(ref newInstance, CurrentElement.Instance as MEPCurve, SelectionViewModel.SelectedShape.Shape));
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
                    jo.Join(FamilyInstanceList.ToArray(), FamilySymbol);
                }
                Transactions.Regenerate();
            }
        }

        private Level GetLevel(Element instance, Transform trans = null)
        {
            Level result;
            Reference planarReference;
            switch (instance.GetType().Name)
            {
                case nameof(Duct):
                case nameof(Pipe):
                case nameof(CableTray):
                case nameof(Conduit):
                    planarReference = (instance as MEPCurve).ReferenceLevel.GetPlaneReference();
                    break;
                default: return default;
            }
            result = ActiveDoc.GetElement(planarReference) as Level;
            if (trans != null)
            {
                result = GetNearestLevel(result.Elevation);
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

            GeometryElement geometry = data.Element.get_Geometry(_options);
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
