using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace FireBoost.Features.ParametersCopy.ViewModels
{
    /// <summary></summary>
    public class ListBoxParameter : INotifyPropertyChanged
    {
        private readonly string _valueDefault;
        private string _selectedParameter;

        /// <summary></summary>
        public string Name { get; }
        /// <summary></summary>
        public string[] ProjectParameters { get; set; }
        /// <summary></summary>
        public string SelectedParameter 
        {
            get => _selectedParameter;
            set => ChangeProperty(ref _selectedParameter, value == _valueDefault ? string.Empty : value);
        }

        /// <summary></summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary></summary>
        public ListBoxParameter(string name, string[] projectParameters, string selectedParameter = null)
        {
            Name = name;
            ProjectParameters = projectParameters;
            SelectedParameter = selectedParameter;
            _valueDefault = projectParameters[0];
        }

        /// <summary></summary>
        protected virtual void ChangeProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
