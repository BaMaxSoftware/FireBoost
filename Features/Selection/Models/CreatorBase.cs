using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using FireBoost.Domain.Entities;
using FireBoost.Domain.Enums;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FireBoost.Features.Selection.Models
{
    internal class CreatorBase
    {
        public Options Options { get; } = new Options()
        {
            DetailLevel = ViewDetailLevel.Fine,
            IncludeNonVisibleObjects = true,
            ComputeReferences = true
        };
        public SettingsVM SettingsViewModel { get; }
        public FamilySymbol FamilySymbol { get; }
        public SelectionVM SelectionViewModel { get; }
        public Document ActiveDoc { get; }
        public List<FamilyInstance> FamilyInstanceList { get; set; } = new List<FamilyInstance>();
        public Transactions Transactions { get; }
        public (Element Element, Transform Transform, XYZ GlobalPoint) CurrentHost { get; set; }
        public (Element Instance, Transform Transform) CurrentElement { get; set; }

        public (double Height, double Width, double Diameter) Dimensions { get; set; }
        public (int dimensions, int elevation) RoundTo { get; set; }
        public (double Offset, double Thickness) Offsets { get; set; }


        public CreatorBase(
            SelectionVM viewModel, 
            SettingsVM settingsViewModel, 
            Document activeDoc, 
            FamilySymbol familySymbol, 
            (double, double, double) dimensions,
            (double, double) offsets, 
            (int, int) roundTo)
        {
            SelectionViewModel = viewModel;
            SettingsViewModel = settingsViewModel;
            ActiveDoc = activeDoc;
            FamilySymbol = familySymbol;
            Transactions = new Transactions(ActiveDoc);
            Dimensions = dimensions;
            Offsets = offsets;
            RoundTo = roundTo;
        }

        public virtual void CreateInstances() { }

        public bool TryCollectHosts(out (Element, Transform, XYZ)[] elements)
        {
            bool result = true;
            elements = new (Element, Transform, XYZ)[SelectionViewModel.SelectedDocHosts.RefList.Count + SelectionViewModel.SelectedLinkHosts.RefList.Count];
            Element element;
            int count = 0;
            bool isValidThickness = Offsets.Thickness > 0;
            if (SelectionViewModel.SelectedDocHosts.RefList.Count > 0)
            {
                foreach (Reference host in SelectionViewModel.SelectedDocHosts.RefList)
                {
                    elements[count] = (ActiveDoc.GetElement(host.ElementId), null, host.GlobalPoint);
                    if (!isValidThickness & elements[count].Item1 is DirectShape)
                    {
                        result = false;
                        break;
                    }
                    count++;
                }
            }
            if (SelectionViewModel.SelectedLinkHosts.RefList.Count > 0 & result)
            {
                foreach (Reference host in SelectionViewModel.SelectedLinkHosts.RefList)
                {
                    element = ActiveDoc.GetElement(host.ElementId);
                    if (element is RevitLinkInstance link && host.LinkedElementId != ElementId.InvalidElementId)
                    {
                        elements[count] = (link.GetLinkDocument().GetElement(host.LinkedElementId), (element as RevitLinkInstance).GetTotalTransform(), host.GlobalPoint);
                        if (!isValidThickness & elements[count].Item1 is DirectShape)
                        {
                            result = false;
                            break;
                        }
                        count++;
                    }
                }
            }
            if (!result)
            {
                MessageBox.Show("Необходимо заполнить поле \"Толщина\", т.к. среди выбранных элементов присутствует категория, из которой невозможно получить толщину.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
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


        public void ChangeSize(FamilyInstance newInstance, double slopeOffset = 0)
        {
            double thickness = 1;
            if (SelectionViewModel.IsDimensionsManually)
            {
                thickness = Offsets.Thickness > 0 ? Offsets.Thickness : thickness;
                if (Offsets.Offset != 0)
                {
                    Dimensions = (
                        Dimensions.Height == 0 ? 0 : Dimensions.Height + Offsets.Offset + slopeOffset,
                        Dimensions.Width,
                        Dimensions.Diameter == 0 ? 0 : Dimensions.Diameter + Offsets.Offset + slopeOffset);
                }
                if (RoundTo.dimensions != 0)
                {
                    Dimensions = RoundDimensions(Dimensions);
                }
            }
            else
            {
                if (CurrentElement != default)
                {
                    var result = GetDimensions(CurrentElement.Instance);

                    if (result == (0, 0, 0))
                        return;

                    var parameters = new InstanceParameters(CurrentElement.Instance);
                    if (parameters.IsValid)
                    {
                        switch (SelectionViewModel.SelectedShape.Shape)
                        {
                            case SealingShapeType.Reachtangle:

                                if (parameters.Diameter == BuiltInParameter.INVALID)
                                {
                                    Dimensions = (
                                        CurrentElement.Instance.get_Parameter(parameters.Height).AsDouble() + Offsets.Offset + slopeOffset,
                                        CurrentElement.Instance.get_Parameter(parameters.Width).AsDouble() + Offsets.Offset,
                                        0);
                                }
                                else
                                {
                                    double size = CurrentElement.Instance.get_Parameter(parameters.Diameter).AsDouble();
                                    Dimensions = (
                                        size + Offsets.Offset + slopeOffset,
                                        size + Offsets.Offset,
                                        0);
                                }
                                break;
                            case SealingShapeType.Round:
                                Dimensions = (
                                        0,
                                        0,
                                        result.Diameter == 0 ? Math.Sqrt(Math.Pow(result.Height, 2) + Math.Pow(result.Width, 2)) : result.Diameter + Offsets.Offset + slopeOffset);
                                break;
                        }
                    }
                    
                    switch (CurrentHost.Element)
                    {
                        case Wall wall:
                            thickness = wall.WallType?.Width ?? thickness;
                            break;
                        case Floor floor:
                            thickness = floor?.FloorType?.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM)?.AsDouble() ?? thickness;
                            break;
                        case Panel panel:
                            thickness = panel?.PanelType?.get_Parameter(BuiltInParameter.CURTAIN_WALL_SYSPANEL_THICKNESS)?.AsDouble() ?? thickness;
                            break;
                        case DirectShape _:
                            thickness = Offsets.Thickness > 0 ? Offsets.Thickness : thickness;
                            break;
                    }
                }
            }

            Transactions.ChangeOpeningsDimensions(SelectionViewModel.SelectedShape.Shape, newInstance,
                Dimensions.Height,
                Dimensions.Width,
                Dimensions.Diameter);

            if (TryGetRotationParams(newInstance, out (Line Axis, double Angle) rotation))
            { 
                Transactions.RotateInstance(newInstance, rotation.Axis, rotation.Angle);
            }

            
            
            Transactions.ChangeOtherParams(newInstance,
                SelectionViewModel.SelectedFireResistance.Depth.ToString(),
                SelectionViewModel.SelectedFireResistance.Minutes.ToString(),
                SelectionViewModel.SelectedMaterial.SealingMaterialType);
        }

        public void Move(FamilyInstance instance)
        {
            if (CurrentHost.Element is Wall wall)
            {
                Transactions.Move(instance, (CurrentHost.Transform == null ? wall.Orientation : CurrentHost.Transform.OfVector(wall.Orientation)) * wall.Width / 2);
            }
            else if (CurrentHost.Element is Floor floor)
            {
                Transactions.Move(instance, new XYZ(0, 0, -1) * floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble() / 2);
            }
        }

        private bool TryGetRotationParams(FamilyInstance instance, out (Line Axis, double Angle) rotation)
        {
            XYZ orient = null;
            
            GetNormalVectors(geometryElement);
            if (globalPoints.Count > 0)
            {
                var temp = globalPoints.Min(x => x.Distance);
                orient = globalPoints.FirstOrDefault(x => x.Distance == temp).GlobalPoint;
            }
            return orient;
        }

        public void Rotate(FamilyInstance instance, out XYZ orient, Line line = null)
        {
            orient = null;
            globalPoints = new List<(XYZ, double)>();

            if (CurrentElement.Instance == null)
            {
                var geom = CurrentHost.Transform == null ?
                    CurrentHost.Element.get_Geometry(Options) :
                    CurrentHost.Element.get_Geometry(Options).GetTransformed(CurrentHost.Transform);
                orient = NormalVectorFromGeometry(geom);
            }
            else
            { 
                switch (CurrentHost.Element)
                {
                    case Wall wall:
                        orient = CurrentHost.Transform == null ? wall.Orientation : CurrentHost.Transform.OfVector(wall.Orientation);
                        break;

                case Floor _:
                    if (CurrentElement.Instance != null && CurrentElement.Instance is MEPCurve mepCurve)
                    {
                        foreach (object c in mepCurve.ConnectorManager.Connectors)
                        {
                            if (c is Connector connector)
                            {
                                if (connector.CoordinateSystem.BasisZ.Z > 0)
                                {
                                    orient = connector.CoordinateSystem.BasisY;
                                    if (CurrentElement.Transform != null)
                                    {
                                        orient = CurrentElement.Transform.OfVector(orient);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }

            if (orient != null && instance.Location is LocationPoint location)
            {
                var angle = Math.PI * 2 - orient.AngleOnPlaneTo(ActiveDoc.ActiveProjectLocation.GetTotalTransform().BasisY, XYZ.BasisZ);
                var point = location.Point;
                var axis = Line.CreateBound(point, point.Add(XYZ.BasisZ));
                Transactions.RotateInstance(instance, axis, angle);
            }
        }

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
                    result = (0, 0, element.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER)?.AsDouble() + insulation ?? 10);
                    break;

                case Duct duct:
                    switch (duct.DuctType.Shape)
                    {
                        case ConnectorProfileType.Rectangular:
                            result = (
                                element.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM)?.AsDouble() + insulation ?? 10,
                                element.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM)?.AsDouble() + insulation ?? 10,
                                0);
                            break;

                        case ConnectorProfileType.Round:
                            result = (0, 0, element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM)?.AsDouble() + insulation ?? 10);
                            break;
                    }
                    break;

                case Conduit _:
                    result = (0, 0, element.get_Parameter(BuiltInParameter.RBS_CONDUIT_OUTER_DIAM_PARAM)?.AsDouble() + insulation ?? 10);
                    break;

                case CableTray _:
                    result = (
                        element.get_Parameter(BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM)?.AsDouble() + insulation ?? 10,
                        element.get_Parameter(BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM)?.AsDouble() + insulation ?? 10,
                        0);
                    break;
            }

            if (RoundTo.dimensions != 0)
            {
                result = RoundDimensions(result);
            }

            return result;
        }

        public void ChangeProjectParameters(FamilyInstance instance)
        {
            if (SettingsViewModel.Parameters != default && SettingsViewModel.Parameters.Length > 0)
            {
                Transactions.ChangeProjectParams(instance, SettingsViewModel.Parameters);
            }
        }

        public void ChangeInstanceElevation(FamilyInstance instance, double elevation)
        {
            if (RoundTo.elevation != 0)
            {
                elevation = RoundToMM(RoundTo.elevation, elevation);
            }
            Transactions.ChangeInstanceElevation(instance, elevation);
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
