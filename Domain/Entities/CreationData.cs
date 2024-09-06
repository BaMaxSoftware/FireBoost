using Autodesk.Revit.DB;
using FireBoost.Features.Selection.ViewModels;
using FireBoost.Features.Settings;

namespace FireBoost.Domain.Entities
{
    /// <summary></summary>
    public class CreationData
    {
        /// <summary></summary>
        public SelectionVM SelectionViewModel { get;  }
        /// <summary></summary>
        public SettingsVM SettingsViewModel { get; }
        /// <summary></summary>
        public Document ActiveDoc { get; }
        /// <summary></summary>
        public FamilySymbol FamilySymbol { get; }
        /// <summary></summary>
        public double Offset { get; }
        /// <summary></summary>
        public (int Dimensions, int Elevation) RoundTo { get; }
        /// <summary></summary>
        public (double Height, double Width, double Diameter) Dimensions { get; set; }

        /// <summary></summary>
        public (string Height, string Width, string Diameter) DimensionsParams { get; set; }

        /// <summary></summary>
        public CreationData(SelectionVM viewModel, SettingsVM settingsViewModel, Document activeDoc, FamilySymbol familySymbol, 
            double offset,
            (int Dimensions, int Elevation) roundTo, 
            (double Height, double Width, double Diameter) dimensions, 
            (string Height, string Width, string Diameter) dimensionsParams = default)
        {
            SelectionViewModel = viewModel;
            SettingsViewModel = settingsViewModel;
            ActiveDoc = activeDoc;
            FamilySymbol = familySymbol;
            Dimensions = dimensions;
            Offset = offset;
            RoundTo = roundTo;
            DimensionsParams = dimensionsParams;
        }
    }
}
