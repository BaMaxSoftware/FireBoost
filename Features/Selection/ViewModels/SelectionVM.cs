using System.Windows;

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
                if ((_selectedDocElements.RefList.Count == 0 &&
                    _selectedLinkElements.RefList.Count == 0) &
                    _selectedMepType.AllowedCategories.Length > 0)
                {
                    MessageBox.Show("Элементы коммуникаций не выбраны.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                if (_selectedDocHosts.RefList.Count == 0 && _selectedLinkHosts.RefList.Count == 0)
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

        /// <summary></summary>
        public void GetActiveUIDocument() => _getActiveUIDocument.Raise();
    }
}
