using System;
using System.ComponentModel;
using System.Globalization;

namespace FireBoost.Features.Selection.ViewModels
{
    public partial class SelectionVM : IDataErrorInfo
    {
        /// <summary></summary>
        public string Error => throw new NotImplementedException();

        /// <summary></summary>
        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "Height":
                        TryParseToDouble(_height, out error);
                        break;
                    case "Width":
                        TryParseToDouble(_width, out error);
                        break;
                    case "Diameter":
                        TryParseToDouble(_diameter, out error);
                        break;
                    case "Offset":
                        TryParseToDouble(_offset, out error);
                        break;
                    case "DimensionsRoundTo":
                        TryParseToInt(_dimensionsRoundTo, out error);
                        break;
                    case "ElevationRoundTo":
                        TryParseToInt(_elevationRoundTo, out error);
                        break;
                    case "Thickness":
                        TryParseToInt(_thickness, out error);
                        break;
                }
                return error;
            }
        }

        private void TryParseToInt(string value, out string error) =>
            error = int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int ret) ?
                (ret < 0 || ret > int.MaxValue ? $"Допустимые значения: 0-{int.MaxValue}" : string.Empty) :
                "Не является числом";

        private void TryParseToDouble(string value, out string error) =>
            error = double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double ret) ?
                (ret < 0 || ret > double.MaxValue ? $"Допустимые значения: 0-{double.MaxValue}" : string.Empty) :
                "Не является числом";
    }
}
