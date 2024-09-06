using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        
        private Solid _hostSolid;
        private Curve _elementCurve;
        

        public CreatorWithMEP(
            SelectionVM viewModel, 
            SettingsVM _settingsViewModel, 
            Document activeDoc, 
            FamilySymbol familySymbol, 
            (double Height, double Width, double Diameter) dimensions,
            (double, double) offset,
            (int dimensions, int elevation) roundTo)
            : base(viewModel, _settingsViewModel, activeDoc, familySymbol, dimensions, offset, roundTo)
        {
        }


        override public void CreateInstances()
        {
            if (!TryCollectHosts(out (Element, Transform, XYZ)[] refsHosts))
                return;
            (Element, Transform)[] refsElements = CollectElements();
            JoinWallOpenings jo;
            Line hostLine;
            double slopeOffset = 0d;
            SolidCurveIntersection intersections;
            foreach (var host in refsHosts)
            {
                FamilyInstanceList = new List<FamilyInstance>();
                CurrentHost = host;
                if (CurrentHost.Element == default || !CurrentHost.Element.IsValidObject) continue;

                _hostSolid = GetSolid(host);
                if (_hostSolid == null) continue;
                foreach (var element in refsElements)
                {
                    CurrentElement = element;
                    if (CurrentElement.Instance == default || !CurrentElement.Instance.IsValidObject) continue;

                    _elementCurve = GetCurve(element);
                    if (_elementCurve == null) continue;
                    try
                    {
                        intersections = _hostSolid.IntersectWithCurve(_elementCurve, IntersectionOptions);
                    }
                    catch
                    {
                        continue;
                    }
                    if (intersections.SegmentCount == 0) continue;

                    foreach (Curve intersection in intersections)
                    {
                        if (intersection is Line)
                        {
                            hostLine = intersection as Line;
                            Level currentLevel = null;
                            double elevation = 0;
                            switch (SelectionViewModel.SelectedHost.DBId)
                            {
                                case 1:
                                    currentLevel = GetLevel(CurrentElement.Instance, CurrentElement.Transform);
                                    elevation = (hostLine.GetEndPoint(0).Z + hostLine.GetEndPoint(1).Z) / 2 - currentLevel?.Elevation ?? 0;
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
                            Rotate(newInstance, out XYZ orient, SelectionViewModel.SelectedHost.DBId == 1 ? hostLine : null);
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

                if (SelectionViewModel.IsJoin && CurrentHost.Element != default && (CurrentHost.Element is Wall || CurrentHost.Element is Panel))
                {
                    jo = new JoinWallOpenings(SettingsViewModel, ActiveDoc, RoundTo);
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
            Solid tempSolid;
            GeometryElement geometry = data.Element.get_Geometry(Options);
            if (data.Transform != null)
            {
                geometry = geometry.GetTransformed(data.Transform);
            }
            foreach (GeometryObject geometryObject in geometry)
            {
                switch (geometryObject)
                { 
                    case GeometryElement geometryElement:
                        tempSolid = GetSolid(geometryElement);
                        break;

                    case GeometryInstance geometryInstance:
                        tempSolid = GetSolid(geometryInstance.GetInstanceGeometry());
                        break;

                    case Solid s:
                        tempSolid = s;
                        break;

                    default:
                        continue;
                }
               
                if (solid == default)
                { 
                    solid = tempSolid;
                }
                else
                {
                    if (solid.Volume < tempSolid.Volume)
                    { 
                        solid = tempSolid;
                    }
                }
            }

            return solid;
        }

        private Solid GetSolid(GeometryElement gelement)
        {
            Solid solid = default;
            Solid tempSolid;
            foreach (GeometryObject item in gelement)
            {
                if (item is Solid)
                {
                    tempSolid = item as Solid;
                    if (solid == default)
                    {
                        solid = tempSolid;
                    }
                    else
                    {
                        if (solid.Volume < tempSolid.Volume)
                        {
                            solid = tempSolid;
                        }
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
            (Element, Transform)[] elements = new (Element, Transform)[SelectionViewModel.SelectedDocElements.RefList.Count + SelectionViewModel.SelectedLinkElements.RefList.Count];
            Element element;
            int count = 0;
            if (SelectionViewModel.SelectedDocElements.RefList.Count > 0)
            {
                foreach (Reference host in SelectionViewModel.SelectedDocElements.RefList)
                {
                    elements[count] = (ActiveDoc.GetElement(host.ElementId), null);
                    count++;
                }
            }
            if (SelectionViewModel.SelectedLinkElements.RefList.Count > 0)
            {
                foreach (Reference host in SelectionViewModel.SelectedLinkElements.RefList)
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
