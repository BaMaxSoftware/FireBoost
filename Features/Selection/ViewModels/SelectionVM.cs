using System.Windows;
using FireBoost.Domain.Entities;

namespace FireBoost.Features.Selection.ViewModels
{
    /// <summary></summary>
    public partial class SelectionVM
    {
        /// <summary></summary>
        public bool IsValidData(bool showMsg = true)
        {
            if (_selectedHost == null ||
                _selectedShape == null ||
                _selectedMepType == null ||
                _selectedMaterial == null ||
                _selectedStructuralDesign == null ||
                _selectedFireResistances == null)
            {
                if (showMsg)
                    MessageBox.Show("Исходные данные не заполнены.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (showMsg)
            { 
                if ((_docElementReferences.Count == 0 &&
                    _linkElementReferences.Count == 0) &
                    _selectedMepType.AllowCategories.Length > 0)
                {
                    MessageBox.Show("Элементы коммуникаций не выбраны.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                if (_docHostReferences.Count == 0 &&
                    _linkHostReferences.Count == 0)
                {
                    MessageBox.Show("Элементы основы не выбраны.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }

            return true;
        }

        /// <summary></summary>
        public void SettingsShowDialog() => _selectionApp.SettingsShowDialog();

        /// <summary></summary>
        public void Start() => _selectionApp.Start();
    }
}
