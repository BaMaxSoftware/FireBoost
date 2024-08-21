using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using FireBoost.Features.Selection.ViewModels;
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
        

        public CreatorWithMEP(SelectionVM viewModel, Document activeDoc, FamilySymbol familySymbol, (double Height, double Width, double Diameter) dimensions, double offset, (int dimensions, int elevation) roundTo)
            : base(viewModel, activeDoc, familySymbol, dimensions, offset, roundTo)
        {
        }

        public void CreateInstances()
        {
            (Element, Transform)[] refsElements = CollectElements();
            (Element, Transform, XYZ)[] refsHosts = CollectHosts();
            JoinWallOpenings jo;
            Line hostLine, intersectionLine;
            _familyInstances = new List<FamilyInstance>();
            foreach (var host in refsHosts)
            {
                _currentHost = host;
                if (_currentHost.Element == default || !_currentHost.Element.IsValidObject || _currentHost.Element is FamilyInstance) continue;

                Solid hostSolid = GetSolid(host);
                if (hostSolid == null) continue;
                foreach (var element in refsElements)
                {
                    _currentElement = element;
                    if (_currentElement.Instance == default || !_currentElement.Instance.IsValidObject) continue;

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
                            if (_currentHost.Element is Wall)
                            {
                                Wall wall = _currentHost.Element as Wall;
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
                            switch (_viewModel.SelectedHost.BuiltInCategory)
                            {
                                case BuiltInCategory.OST_Walls:
                                    currentLevel = GetLevel(_currentElement.Instance, _currentElement.Transform);
                                    location = hostLine.GetEndPoint(0) + (hostLine.Length / 2 * hostLine.Direction);
                                    elevation = (point0.Z + point1.Z) / 2 - currentLevel.Elevation;
                                    break;
                                case BuiltInCategory.OST_Floors:
                                    currentLevel = GetLevel(ActiveDoc.GetElement(_currentHost.Element.LevelId) as Level);
                                    location = point0.Z > point1.Z ? point0 : point1;
                                    elevation = _currentHost.Element.get_Parameter(BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP).AsDouble() - currentLevel.ProjectElevation;
                                    break;
                                default: continue;
                            }
                            if (currentLevel == default || !currentLevel.IsValidObject || location == null) continue;

                            newInstance = Transactions.CreateNewInstance(
                                        _familySymbol,
                                        hostLine.GetEndPoint(0) + (hostLine.Length / 2 * hostLine.Direction),
                                        currentLevel);
                            if (newInstance == default) continue;

                            ChangeInstanceElevation(ref newInstance, elevation);
                            ChangeSize(ref newInstance, slopeOffset);

                            Transactions.ChangeSelectedParams(ActiveDoc, _sizeChanger.SetOtherParams(ref newInstance, _currentElement.Instance as MEPCurve, _viewModel.SelectedShape.Shape));

                            if (newInstance != default && newInstance.IsValidObject)
                            {
                                if (_familyInstances.FirstOrDefault(x => x.IsValidObject && x.Id == newInstance.Id) == default)
                                {
                                    _familyInstances.Add(newInstance);
                                }
                            }
                        }
                    }
                }

                if (_viewModel.IsJoin && _currentHost.Element != default && _currentHost.Element is Wall)
                {
                    jo = new JoinWallOpenings(ActiveDoc, _currentHost.Element as Wall, RoundTo);
                    jo.Join(_familyInstances.ToArray(), _familySymbol);
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
            (Element, Transform)[] elements = new (Element, Transform)[_viewModel.DocElementReferences.Count + _viewModel.LinkElementReferences.Count];
            Element element;
            int count = 0;
            if (_viewModel.DocElementReferences.Count > 0)
            {
                foreach (Reference host in _viewModel.DocElementReferences)
                {
                    elements[count] = (ActiveDoc.GetElement(host.ElementId), null);
                    count++;
                }
            }
            if (_viewModel.LinkElementReferences.Count > 0)
            {
                foreach (Reference host in _viewModel.LinkElementReferences)
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
