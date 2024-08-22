using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FireBoost.Features.Settings
{
    /// <summary></summary>
    public class SettingsVM : INotifyPropertyChanged
    {
        private string _roundWallPath;
        private string _roundFloorPath;
        private string _rectangularWallPath;
        private string _rectangularFloorPath;
        private string _schedulesPath;
        private (string Name, string SelectedParameter)[] _parameters;

        /// <summary></summary>
        [JsonProperty("Parameters")]
        public (string Name, string SelectedParameter)[] Parameters
        {
            get => _parameters;
            set => OnPropertyChanged(ref _parameters, value);
        }

        /// <summary></summary>
        [JsonProperty("SchedulesPath")]
        public string SchedulesPath
        {
            get => _schedulesPath;
            set => OnPropertyChanged(ref _schedulesPath, value);
        }
        /// <summary></summary>
        [JsonProperty("RoundWallPath")]
        public string RoundWallPath
        {
            get => _roundWallPath;
            set => OnPropertyChanged(ref _roundWallPath, value);
        }
        /// <summary></summary>
        [JsonProperty("RoundFloorPath")]
        public string RoundFloorPath
        {
            get => _roundFloorPath;
            set => OnPropertyChanged(ref _roundFloorPath, value);
        }
        /// <summary></summary>
        [JsonProperty("RectangularWallPath")]
        public string RectangularWallPath
        {
            get => _rectangularWallPath;
            set => OnPropertyChanged(ref _rectangularWallPath, value);
        }
        /// <summary></summary>
        [JsonProperty("RectangularFloorPath")]
        public string RectangularFloorPath
        {
            get => _rectangularFloorPath;
            set => OnPropertyChanged(ref _rectangularFloorPath, value);
        }

        /// <summary></summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary></summary>
        protected virtual void OnPropertyChanged<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            oldValue = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary></summary>
        public string GetPath(BuiltInCategory bic, bool isRound)
        {
            switch (bic)
            {
                case BuiltInCategory.OST_Walls:
                    return isRound ? _roundWallPath : _rectangularWallPath;

                case BuiltInCategory.OST_Floors:
                    return isRound ? _roundFloorPath : _rectangularFloorPath;
            }

            return null;
        }
    }
}
