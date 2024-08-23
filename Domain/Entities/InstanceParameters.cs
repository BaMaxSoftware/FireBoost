using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace FireBoost.Domain.Entities
{
    internal class InstanceParameters
    {
        public BuiltInParameter Height { get; }
        public BuiltInParameter Width { get; }
        public BuiltInParameter Diameter { get; }
        public bool IsValid { get; private set; }
        public InstanceParameters(Element element)
        {
            if (element == null)
            {
                IsValid = false;
                return;
            }
            IsValid = true;
            switch (element.Category.Id.IntegerValue)
            {
                case -2008044:  // трубы
                    Height = 
                    Width = BuiltInParameter.INVALID;
                    Diameter = BuiltInParameter.RBS_PIPE_OUTER_DIAMETER;
                    break;
                case -2008000:  // воздуховоды
                    if (element is Duct duct)
                    {
                        switch (duct.DuctType.Shape)
                        {
                            case ConnectorProfileType.Rectangular:
                                Height = BuiltInParameter.RBS_CURVE_HEIGHT_PARAM;
                                Width = BuiltInParameter.RBS_CURVE_WIDTH_PARAM;
                                Diameter = BuiltInParameter.INVALID;
                                break;

                            case ConnectorProfileType.Round:
                                Height = 
                                Width = BuiltInParameter.INVALID;
                                Diameter = BuiltInParameter.RBS_CURVE_DIAMETER_PARAM;
                                break;
                            default:
                                IsValid = false;
                                break;
                        }
                    }
                    break;
                case -2008130:  // лотки
                    Height = BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM;
                    Width = BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM;
                    Diameter = BuiltInParameter.INVALID;
                    break;
                case -2008132:  // короба
                    Height = 
                    Width = BuiltInParameter.INVALID;
                    Diameter = BuiltInParameter.RBS_CONDUIT_OUTER_DIAM_PARAM;
                    break;
                default:
                    IsValid = false;
                    break;
            }
        }
    }
}
