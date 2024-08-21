using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using FireBoost.Domain.Data;
using FireBoost.Domain.Entities;
using FireBoost.Domain.Enums;
using FireBoost.Features.Selection.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FireBoost.Features.Selection.Models
{
    internal class CreatorBase
    {
        public FamilySymbol _familySymbol { get; }
        public SelectionVM _viewModel { get; }
        public Document ActiveDoc { get; }
        public List<FamilyInstance> _familyInstances { get; set; } = new List<FamilyInstance>();
        public Transactions Transactions { get; }
        public (Element Element, Transform Transform, XYZ GlobalPoint) _currentHost { get; set; }
        public (Element Instance, Transform Transform) _currentElement { get; set; }

        public (double Height, double Width, double Diameter) Dimensions { get; set; }
        public (int dimensions, int elevation) RoundTo { get; set; }
        public double Offset { get; set; }
        public CreatorBase(SelectionVM viewModel, Document activeDoc, FamilySymbol familySymbol, (double Height, double Width, double Diameter) dimensions, double offset, (int dimensions, int elevation) roundTo)
        {
            _viewModel = viewModel;
            ActiveDoc = activeDoc;
            _familySymbol = familySymbol;
            Transactions = new Transactions(ActiveDoc);
            Dimensions = dimensions;
            Offset = offset;
            RoundTo = roundTo;
        }

        public (Element, Transform, XYZ)[] CollectHosts()
        {
            (Element, Transform, XYZ)[] elements = new (Element, Transform, XYZ)[_viewModel.DocHostReferences.Count + _viewModel.LinkHostReferences.Count];
            Element element;
            int count = 0;
            if (_viewModel.DocHostReferences.Count > 0)
            {
                foreach (Reference host in _viewModel.DocHostReferences)
                {
                    elements[count] = (ActiveDoc.GetElement(host.ElementId), null, host.GlobalPoint);
                    count++;
                }
            }
            if (_viewModel.LinkHostReferences.Count > 0)
            {
                foreach (Reference host in _viewModel.LinkHostReferences)
                {
                    element = ActiveDoc.GetElement(host.ElementId);
                    if (element is RevitLinkInstance link && host.LinkedElementId != ElementId.InvalidElementId)
                    {
                        elements[count] = (link.GetLinkDocument().GetElement(host.LinkedElementId), (element as RevitLinkInstance).GetTotalTransform(), host.GlobalPoint);
                        count++;
                    }
                }
            }

            return elements;
        }
        
        public Level GetNearestLevel(double elevation)
        {
            List<Level> levels = new FilteredElementCollector(ActiveDoc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .ToList();

            foreach (Level level in levels)
            {
                if (level.Elevation > elevation)
                {
                    int levelIndex = levels.IndexOf(level);
                    return levels[levelIndex > 0 ? levelIndex - 1 : 0];
                }
            }

            return levels.Last();
        }


        public void ChangeSize(ref FamilyInstance newInstance, double slopeOffset = 0)
        {
            if (!_viewModel.IsDimensionsManually)
            {
                if (_currentElement != default)
                {
                    var result = GetDimensions(_currentElement.Instance);

                    if (result == (0, 0, 0))
                        return;

                    var parameters = new InstanceParameters(_currentElement.Instance);
                    if (parameters.IsValid)
                    {
                        switch (_viewModel.SelectedShape.Shape)
                        {
                            case OpeningShape.Reachtangle:

                                if (parameters.Diameter == BuiltInParameter.INVALID)
                                {
                                    Dimensions = (
                                        _currentElement.Instance.get_Parameter(parameters.Height).AsDouble() + Offset + slopeOffset,
                                        _currentElement.Instance.get_Parameter(parameters.Width).AsDouble() + Offset,
                                        0);
                                }
                                else
                                {
                                    double size = _currentElement.Instance.get_Parameter(parameters.Diameter).AsDouble() + Offset;
                                    Dimensions = (
                                        size + Offset + slopeOffset,
                                        size + Offset,
                                        0);
                                }
                                break;
                            case OpeningShape.Round:
                                Dimensions = (
                                        0,
                                        0,
                                        result.Diameter == 0 ? Math.Sqrt(Math.Pow(result.Height, 2) + Math.Pow(result.Width, 2)) : result.Diameter + Offset + slopeOffset);
                                break;
                        }
                    }
                }
            }
            else
            {
                if (Offset != 0)
                {
                    Dimensions = (
                        Dimensions.Height == 0 ? 0 : Dimensions.Height + Offset + slopeOffset,
                        Dimensions.Width,
                        Dimensions.Diameter == 0 ? 0 : Dimensions.Diameter + Offset + slopeOffset);
                }
                if (RoundTo.dimensions != 0)
                {
                    Dimensions = RoundDimensions(Dimensions);
                }
            }

            Transactions.ChangeOpeningsDimensions(_viewModel.SelectedShape.Shape, ref newInstance,
                Dimensions.Height,
                Dimensions.Width,
                Dimensions.Diameter);

            if (TryGetRotationParams(newInstance, out (Line Axis, double Angle) rotation))
            { 
                Transactions.RotateInstance(ref newInstance, rotation.Axis, rotation.Angle);
            }

            if (_currentHost.Element is Wall wall)
            {
                Transactions.Move(ref newInstance, (_currentHost.Transform == null ? wall.Orientation : _currentHost.Transform.OfVector(wall.Orientation)) * wall.Width / 2);
            }
            else if (_currentHost.Element is Floor floor)
            {
                Transactions.Move(ref newInstance, new XYZ(0, 0, -1) * floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble() / 2);
            }
            
            Transactions.ChangeOtherParams(ref newInstance,
                _viewModel.SelectedFireResistance.Depth.ToString(),
                _viewModel.SelectedFireResistance.Minutes.ToString(),
                _viewModel.SelectedMaterial.SealingMaterialType);
        }

        private bool TryGetRotationParams(FamilyInstance instance, out (Line Axis, double Angle) rotation)
        {
            XYZ orient = null;
            switch (_currentHost.Element)
            {
                case Wall wall:
                    orient = _currentHost.Transform == null ? wall.Orientation : _currentHost.Transform.OfVector(wall.Orientation);
                    break;

                case Floor _:
                    if (_currentElement.Instance != null && _currentElement.Instance is MEPCurve mepCurve)
                    {
                        foreach (object c in mepCurve.ConnectorManager.Connectors)
                        {
                            if (c is Connector connector)
                            {
                                if (connector.CoordinateSystem.BasisZ.Z > 0)
                                {
                                    orient = connector.CoordinateSystem.BasisY;
                                    if (_currentElement.Transform != null)
                                    {
                                        orient = _currentElement.Transform.OfVector(orient);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }

            if (orient != null)
            {
                var angle = Math.PI * 2 - orient.AngleOnPlaneTo(ActiveDoc.ActiveProjectLocation.GetTotalTransform().BasisY, XYZ.BasisZ);
                var point = (instance.Location as LocationPoint).Point;
                var axis = Line.CreateBound(point, point.Add(XYZ.BasisZ));

                rotation = (axis, angle);
                return true;
            }
            else
            {
                rotation = default;
                return false;
            }
        }


        public (double Height, double Width, double Diameter) GetDimensions(Element element)
        {
            (double Height, double Width, double Diameter) result = (0,0,0);

            switch (element)
            {
                case Pipe _:
                    result = (0, 0, element.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER)?.AsDouble() ?? 10);
                    break;

                case Duct duct:
                    switch (duct.DuctType.Shape)
                    {
                        case ConnectorProfileType.Rectangular:
                            result = (
                                element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM)?.AsDouble() ?? 10,
                                element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM)?.AsDouble() ?? 10,
                                0);
                            break;

                        case ConnectorProfileType.Round:
                            result = (0, 0, element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM)?.AsDouble() ?? 10);
                            break;
                    }
                    break;

                case Conduit _:
                    result = (0, 0, element.get_Parameter(BuiltInParameter.RBS_CONDUIT_OUTER_DIAM_PARAM)?.AsDouble() ?? 10);
                    break;

                case CableTray _:
                    result = (
                        element.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM)?.AsDouble() ?? 10,
                        element.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM)?.AsDouble() ?? 10,
                        0);
                    break;
            }

            if (RoundTo.dimensions != 0)
            {
                result = RoundDimensions(result);
            }

            return result;
        }

        public void ChangeInstanceElevation(ref FamilyInstance instance, double elevation)
        {
            if (RoundTo.elevation != 0)
            {
                elevation = RoundToMM(RoundTo.elevation, elevation);
            }
            Transactions.ChangeInstanceElevation(ref instance, elevation);
        }

        public double RoundToMM(int roundTo, double value)
        {
            double feetToMm = Math.Round(value * 304.8d, 2);
            return (feetToMm - Math.Round(feetToMm % roundTo, 2) + roundTo) * 0.0032808d;
        }

        public (double Height, double Width, double Diameter) RoundDimensions((double Height, double Width, double Diameter) dimensions) => (
            dimensions.Height != 0 ? RoundToMM(RoundTo.dimensions, dimensions.Height) : 0,
            dimensions.Width != 0 ? RoundToMM(RoundTo.dimensions, dimensions.Width) : 0,
            dimensions.Diameter != 0 ? RoundToMM(RoundTo.dimensions, dimensions.Diameter) : 0);
        
    }
}
