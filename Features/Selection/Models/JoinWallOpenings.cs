using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Windows;
using Autodesk.Revit.DB;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Settings;
using Newtonsoft.Json.Linq;

namespace FireBoost.Features.Selection.Models
{
    internal class JoinWallOpenings
    {
        /// <summary>
        /// Внутренний диаметр отверстия
        /// </summary>
        private readonly Guid _openingDiameter = new Guid("fac76ca8-0f8a-4b5f-91ff-018aff5bad25");
        /// <summary>
        /// Высота проема
        /// </summary>
        private readonly Guid _openingHeight = new Guid("43b69be4-81ef-4528-95dc-6f5dd4d1c041");
        /// <summary>
        /// Ширина проема
        /// </summary>
        private readonly Guid _openingWidth = new Guid("45b14688-97e2-4f09-ba8a-2ddc1bd5cbf3");
        private readonly Options _options;

        private int busbarHeight, busbarWidth;
        private int cableTrayHeight, cableTrayWidth;
        private int ductHeight, ductWidth;
        private int mepDiameter;
        private int moduleHeight, moduleWidth;
        private XYZ _location;
        private XYZ _maxX, _minX;
        private XYZ _maxY, _minY;
        private XYZ _maxZ, _minZ;
        private SettingsVM _settings;
        
        private Document Doc { get; }
        private Wall SelectedWall { get; }
        private FamilyInstance[] _familyInstances;
        private Transactions _transactions;
        (int dimensions, int elevation) _roundTo;

        public JoinWallOpenings(SettingsVM settings, Document doc, Wall selectedWall, (int dimensions, int elevation) roundTo)
        {
            _settings = settings;
            Doc = doc;
            SelectedWall = selectedWall;
            _options = new Options();
            _transactions = new Transactions(Doc);
            _roundTo = roundTo;
        }

        private void GetLocation()
        {
            IEnumerable<BoundingBoxXYZ> boundingBoxes = _familyInstances.Select(elem => elem.get_Geometry(_options).GetBoundingBox());

            _minX = boundingBoxes.First(b => b.Min.X == boundingBoxes.Select(xyz => xyz.Min.X).Min()).Min;
            _maxX = boundingBoxes.First(b => b.Max.X == boundingBoxes.Select(xyz => xyz.Max.X).Max()).Max;
            _minY = boundingBoxes.First(b => b.Min.Y == boundingBoxes.Select(xyz => xyz.Min.Y).Min()).Min;
            _maxY = boundingBoxes.First(b => b.Max.Y == boundingBoxes.Select(xyz => xyz.Max.Y).Max()).Max;
            _minZ = boundingBoxes.First(b => b.Min.Z == boundingBoxes.Select(xyz => xyz.Min.Z).Min()).Min;
            _maxZ = boundingBoxes.First(b => b.Max.Z == boundingBoxes.Select(xyz => xyz.Max.Z).Max()).Max;

            _location = new XYZ(_maxX.Add(_minX).Divide(2).X, _maxY.Add(_minY).Divide(2).Y, _maxZ.Add(_minZ).Divide(2).Z);
        }

        public (double width, double height) GetDimensions(bool inWall)
        {
            double width, height;

            if (inWall)
            {
                width = new List<double>()
                {
                    _maxX.DistanceTo(new XYZ(_minX.X, _maxX.Y, _maxX.Z)),
                    _maxY.DistanceTo(new XYZ(_maxY.X, _minY.Y, _maxY.Z)),
                }.Max();
                height = _maxZ.DistanceTo(new XYZ(_maxZ.X, _maxZ.Y, _minZ.Z));
            }
            else
            {
                width = _maxX.DistanceTo(new XYZ(_minX.X, _maxX.Y, _maxX.Z));
                height = _maxY.DistanceTo(new XYZ(_maxY.X, _minY.Y, _maxY.Z));
            }

            return (width, height);
        }

        public void Join(FamilyInstance[] familyInstances, FamilySymbol symbol, (Element Element, Transform Transform, XYZ GlobalPoint) _currentHost)
        {

            if (familyInstances == null || familyInstances.Length == 0) return;
            busbarHeight =
            busbarWidth =
            cableTrayHeight =
            cableTrayWidth =
            ductHeight =
            ductWidth =
            mepDiameter =
            moduleHeight =
            moduleWidth = 0;

            _familyInstances = familyInstances.Where(x => x.IsValidObject).ToArray();
            if (_familyInstances == null || _familyInstances.Length == 0) return;
            GetLocation();
            double angle2 = (_familyInstances[0].Location as LocationPoint).Rotation;

            double width = GetWidth(out XYZ mid);
            if (width == -1) return;
            double height = GetHeight(out double elevation);
            if (height == -1) return;

            if (_roundTo.dimensions != 0)
            { 
                width = RoundToMM(_roundTo.dimensions, width);
                height = RoundToMM(_roundTo.dimensions, height);
            }

            Level lvl = SelectCurrentLevel(ref elevation);
            if (lvl == default) return;

            XYZ wallHalfWidth = SelectedWall.Orientation.Multiply(SelectedWall.Width / 2);
            Line hostLine = Line.CreateBound(_location - wallHalfWidth, _location + wallHalfWidth);
            XYZ loc = hostLine.GetEndPoint(0) + (hostLine.Length / 2 * hostLine.Direction);

            List<(string, double)> dimensions = new List<(string, double)>();

            using (Transaction t = new Transaction(Doc, "Удалить выбранные"))
            {
                t.Start();
                foreach (FamilyInstance element in _familyInstances)
                {
                    (string, double)[] temp = GetOtherParams(element);
                    if (temp.Any(x => !string.IsNullOrEmpty(x.Item1)))
                    {
                        dimensions.AddRange(temp);
                    }
                    Doc.Delete(element.Id);
                }
                t.Commit();
            }
            FamilyInstance newInstance = _transactions.CreateNewInstance(symbol, loc, lvl);

            if (_roundTo.elevation != 0)
            {
                elevation = RoundToMM(_roundTo.elevation, elevation);
            }

            _transactions.ChangeInstanceElevation(ref newInstance, elevation);
            _transactions.ChangeJoinOpeningSize(Doc, ref newInstance, height, width);

            if (_currentHost.Element is Wall wall)
            {
                //_transactions.Move(ref newInstance, (_currentHost.Transform == null ? wall.Orientation : _currentHost.Transform.OfVector(wall.Orientation)) * wall.Width / 2);
            }
            else if (_currentHost.Element is Floor floor)
            {
                //_transactions.Move(ref newInstance, new XYZ(0, 0, -1) * floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble() / 2);
            }

            if (_settings.Parameters != default && _settings.Parameters.Length > 0)
            {
                _transactions.ChangeProjectParams(newInstance, _settings.Parameters);
            }

            using (Transaction t = new Transaction(Doc))
            {
                t.Start("Редактировать атрибуты элемента");
                if (dimensions.Count > 0)
                {
                    foreach ((string, double) d in dimensions)
                    {
                        newInstance.LookupParameter(d.Item1)?.Set(d.Item2);
                    }
                }

                newInstance.Location.Rotate(Line.CreateBound(loc, loc.Add(XYZ.BasisZ)), angle2);
                t.Commit();
            }
        }

        private double GetWidth(out XYZ mid)
        {
            if (!((SelectedWall.Location as LocationCurve).Curve is Line))
            {
                mid = default;
                return -1;
            }

            FamilyInstance fi1 = default, fi2 = default;
            XYZ c1, c2;
            XYZ cc1 = null, cc2 = null;
            double dist;
            double horizontalDistance = 0;
            foreach (FamilyInstance ia in _familyInstances)
            {
                foreach (FamilyInstance ia2 in _familyInstances)
                {
                    if (ia.Id == ia2.Id) continue;
                    c1 = (ia.Location as LocationPoint).Point;
                    c2 = (ia2.Location as LocationPoint).Point;

                    dist = Math.Abs(new XYZ(c1.X, c1.Y, 0).DistanceTo(new XYZ(c2.X, c2.Y, 0)));
                    if (dist > horizontalDistance)
                    {
                        cc1 = new XYZ(c1.X, c1.Y, 0);
                        cc2 = new XYZ(c2.X, c2.Y, 0);
                        fi1 = ia;
                        fi2 = ia2;
                        horizontalDistance = dist;
                    }
                }
            }
            if (fi1 == default || fi2 == default || cc1 == default || cc2 == default)
            {
                mid = default;
                return -1;
            }

            Line newLine = Line.CreateBound(cc1, cc2);
            newLine = Line.CreateBound(
                newLine.GetEndPoint(0).Add(newLine.Direction.Negate().Multiply(fi1.get_Parameter(_openingWidth).AsDouble() / 2)),
                newLine.GetEndPoint(1).Add(newLine.Direction.Multiply(fi2.get_Parameter(_openingWidth).AsDouble() / 2)));
            mid = newLine.Evaluate(newLine.Length / 2, false);
            return newLine.Length;
        }


        private double GetHeight(out double elevation)
        {
            double tempMax, tempMin, elevationZ, instanceHeight;
            double hMax = double.MinValue;
            double hMin = double.MaxValue;
            BoundingBoxXYZ bBox;
            foreach (FamilyInstance e in _familyInstances)
            {
                instanceHeight = e.get_Parameter(_openingHeight).AsDouble() / 2;
                bBox = e.get_BoundingBox(Doc.ActiveView);
                elevationZ = (bBox.Max.Z + bBox.Min.Z) / 2;

                tempMax = elevationZ + instanceHeight;
                if (tempMax > hMax)
                {
                    hMax = tempMax;
                }
                tempMin = elevationZ - instanceHeight;
                if (tempMin < hMin)
                {
                    hMin = tempMin;
                }
            }
            elevation = (hMax + hMin) / 2;
            return Math.Abs(hMax - hMin);
        }

        private Level SelectCurrentLevel(ref double elevation)
        {
            double tempElev = _location.Z;
            IEnumerable<Level> levelsCollector = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .Where(x => x is Level)
                .Cast<Level>()
                .Where(x => x.Elevation <= tempElev);

            if (levelsCollector.Count() == 0)
            {
                MessageBox.Show($"Не найдены уровни ниже отметки {Math.Round(tempElev * 304.8d, 3)}.");
                return default;
            }
            Level[] levels = levelsCollector.ToArray();
            double maxElevation = levels.Max(x => x.Elevation);
            Level lvl = levels.First(x => x.Elevation == maxElevation);
            elevation -= lvl.Elevation;
            return lvl;
        }


        private bool ToDo(string name, FamilyInstance instance, out (string, double) data)
        {
            data = (null, -1);
            bool ret = false;
            double? temp = instance.LookupParameter(name)?.AsDouble();
            if (temp != null)
            {
                ret = true;
                data = (name, (double)temp);
            }
            return ret;
        }

        private (string, double)[] GetOtherParams(FamilyInstance newInstance)
        {
            double? value;
            (string, double)[] ret = new (string, double)[] { (null, 0) };
            if (newInstance.Symbol.FamilyName.StartsWith("ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Комбинированная)"))
            {
                value = newInstance.LookupParameter("Диаметр коммуникации 1")?.AsDouble();
                if (value != null && value > 0)
                {
                    ret = new (string, double)[] { ($"Диаметр коммуникации {++mepDiameter}", (double)value) };
                }
                else
                {
                    value = newInstance.LookupParameter("Высота лотка 1")?.AsDouble();
                    if (value != null && value > 0)
                    {
                        ret = new (string, double)[2]
                        {
                            ($"Высота лотка {++cableTrayHeight}", (double)value),
                            ($"Ширина лотка {++cableTrayWidth}", (double)newInstance.LookupParameter("Ширина лотка 1")?.AsDouble())
                        };
                    }
                    else
                    {
                        value = newInstance.LookupParameter("Высота воздуховода 1")?.AsDouble();
                        if (value != null && value > 0)
                        {
                            ret = new (string, double)[2]
                            {
                                ($"Высота воздуховода {++ductHeight}", (double)value),
                                ($"Ширина воздуховода {++ductWidth}", (double)newInstance.LookupParameter("Ширина воздуховода 1")?.AsDouble())
                            };
                        }
                    }
                }

                return ret;
            }
            if (newInstance.Symbol.FamilyName.StartsWith("ФП-БУСТ-ПКМ-01_AxB_M") ||
                newInstance.Symbol.FamilyName.StartsWith("ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Воздуховоды)") ||
                newInstance.Symbol.FamilyName.StartsWith("ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Кабели и Лотки)") ||
                newInstance.Symbol.FamilyName.StartsWith("ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Шинопроводы)"))
            {
                value = newInstance.LookupParameter("Диаметр коммуникации 1")?.AsDouble();
                if (value == null || value == 0)
                {
                    value = newInstance.LookupParameter("Высота модуля 1")?.AsDouble();
                    if (value != null && value > 0)
                    {
                        ret = new (string, double)[2]
                        {
                            ($"Высота модуля {++moduleHeight}", (double)value),
                            ($"Ширина модуля {++moduleWidth}", (double)newInstance.LookupParameter("Ширина модуля 1")?.AsDouble())
                        };
                    }
                }
                else
                {
                    ret = new (string, double)[] { ($"Диаметр коммуникации {++mepDiameter}", (double)value) };
                }

                return ret;
            }
            if (newInstance.Symbol.FamilyName.StartsWith("ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Трубы)") ||
                newInstance.Symbol.FamilyName.StartsWith("ФП-БУСТ-ПКМ-01_D_П_ПМ_Стена_R19(ОбобщМод)") ||
                newInstance.Symbol.FamilyName.StartsWith("ФП-БУСТ-ПКМ-01_D_М"))
            {
                value = newInstance.LookupParameter("Диаметр коммуникации 1")?.AsDouble();
                if (value != null && value > 0)
                {
                    ret = new (string, double)[] { ($"Диаметр коммуникации {++mepDiameter}", (double)value) };
                }

                return ret;
            }

            return ret;
        }

        public double RoundToMM(int roundTo, double value)
        {
            double feetToMm = Math.Round(value * 304.8d, 2);
            return feetToMm - Math.Round(feetToMm % roundTo, 2) + roundTo;
        }
    }
}
