using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using FireBoost.Domain.Data;
using FireBoost.Domain.Entities;
using FireBoost.Domain.Enums;

namespace FireBoost.Features.Selection.Models
{
    /// <summary></summary>
    public class SizeChanger
    {
        private readonly string familyCombined = "ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Комбинированная)";
        private readonly string familyRechtM = "ФП-БУСТ-ПКМ-01_AxB_M";
        private readonly string _familyDuct = "ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Воздуховоды)";
        private readonly string _familyCable = "ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Кабели и Лотки)";
        private readonly string _familyBusbar = "ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Шинопроводы)";
        private readonly string _familyPipe = "ФП-БУСТ-ПКМ-01_AxB_П_ПМ_(Трубы)";
        private readonly string _familyRoundM = "ФП-БУСТ-ПКМ-01_D_М";
        private readonly string _familyRoundPM = "ФП-БУСТ-ПКМ-01_D_П_ПМ";
        private readonly Parameters _parameters = new Parameters();
        private InstanceParameters _instanceParameters;

        /// <summary></summary>
        public (Parameter, double)[] SetOtherParams(FamilyInstance newInstance, MEPCurve element, SealingShapeType shape)
        {
            (Parameter, double)[] result = null;
            _instanceParameters = new InstanceParameters(element);
            if (!_instanceParameters.IsValid) 
                return result;

            if (newInstance.Symbol.FamilyName.StartsWith(familyCombined) || newInstance.Symbol.FamilyName.StartsWith(familyRechtM))
            {
                switch (element)
                {
                    case Duct _:
                        result = IsRound(element) ?
                            DiameterParams(newInstance, element, _parameters.MepDiameter) :
                            RechtangularParams(newInstance, element, _parameters.DuctHeight, _parameters.DuctWidth);
                        break;

                    case CableTray _:
                        result = RechtangularParams(newInstance, element, _parameters.CableTrayHeight, _parameters.CableTrayWidth);
                        break;

                    case Conduit _:
                    case Pipe _:
                        result = DiameterParams(newInstance, element, _parameters.MepDiameter);
                        break;
                }
            }
            else if (newInstance.Symbol.FamilyName.StartsWith(_familyDuct))
            {
                if (element is Duct)
                {
                    result = IsRound(element) ?
                        DiameterParams(newInstance, element, _parameters.MepDiameter) :
                        RechtangularParams(newInstance, element, _parameters.MepHeight, _parameters.MepWidth);
                }
            }
            else if (newInstance.Symbol.FamilyName.StartsWith(_familyCable) || newInstance.Symbol.FamilyName.StartsWith(_familyBusbar))
            {
                if (element is CableTray || element is Conduit)
                {
                    result = shape == SealingShapeType.Round ?
                        DiameterParams(newInstance, element, _parameters.MepDiameter, IsRound(element)) :
                        IsRound(element) ?
                            DiameterToRechtangularParams(newInstance, element, _parameters.MepHeight, _parameters.MepWidth) :
                            RechtangularParams(newInstance, element, _parameters.MepHeight, _parameters.MepWidth);
                }
            }
            else if (newInstance.Symbol.FamilyName.StartsWith(_familyPipe))
            {
                if (element is Pipe)
                    result = DiameterParams(newInstance, element, _parameters.MepDiameter);
            }
            else if (newInstance.Symbol.FamilyName.StartsWith(_familyRoundM))
            {
                result = DiameterParams(newInstance, element, _parameters.MepDiameter3, _instanceParameters.Diameter == BuiltInParameter.INVALID);
            }
            else if (newInstance.Symbol.FamilyName.StartsWith(_familyRoundPM))
            {
                result = DiameterParams(newInstance, element, _parameters.MepDiameter2, _instanceParameters.Diameter == BuiltInParameter.INVALID);
            }

            return result;
        }

        private bool IsRound(MEPCurve element) => element.ConnectorManager.Connectors.Cast<Connector>().First().Shape == ConnectorProfileType.Round;

        private double GetValue(Element element, BuiltInParameter bip) => element.get_Parameter(bip)?.AsDouble() ?? 0;

        private (Parameter, double)[] RechtangularParams(FamilyInstance instance, MEPCurve element, string height, string width)
        {
            return new (Parameter, double)[]
            {
                (instance.LookupParameter(height), GetValue(element, _instanceParameters.Height)),
                (instance.LookupParameter(width), GetValue(element, _instanceParameters.Width))
            };
        }

        private (Parameter, double)[] DiameterParams(FamilyInstance newInstance, MEPCurve element, string text, bool isRound = true)
        {
            if (isRound)
            {
                return new (Parameter, double)[] { (newInstance.LookupParameter(text), GetValue(element, _instanceParameters.Diameter)) };
            }
            else
            {
                double h = GetValue(element, _instanceParameters.Height);
                double w = GetValue(element, _instanceParameters.Width);
                return new (Parameter, double)[] { (newInstance.LookupParameter(text), h > w ? h : w) };
            }
        }

        private (Parameter, double)[] DiameterToRechtangularParams(FamilyInstance instance, MEPCurve element, string height, string width)
        {
            double d = GetValue(element, _instanceParameters.Diameter);
            return new (Parameter, double)[]
            {
                (instance.LookupParameter(height), d),
                (instance.LookupParameter(width), d)
            };
        }


    }
}
