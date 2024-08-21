using Autodesk.Revit.DB;
using System.Collections.ObjectModel;
using System.Linq;

namespace FireBoost.Features.Specifications
{
    /// <summary></summary>
    public class SpecificationsVM
    {
        private readonly SpecificationsApp _app;
        private readonly Element[] _schedules;

        /// <summary></summary>
        public Element[] Schedules => _schedules;
        /// <summary></summary>
        public ObservableCollection<Element> SelectedSchedules;


        /// <summary></summary>
        public SpecificationsVM(SpecificationsApp app) 
        {
            _app = app;
            _schedules = _app.Schedules;
            SelectedSchedules = new ObservableCollection<Element>();
        }


        /// <summary></summary>
        public void SettingsShowDialog() => _app.SettingsShowDialog();

        /// <summary></summary>
        public void AddSchedule(Element view)
        {
            if (view == null || !view.IsValidObject) return;
            if (!SelectedSchedules.Any(x => x.Id.IntegerValue == view.Id.IntegerValue))
                SelectedSchedules.Add(view);
        }

        /// <summary></summary>
        public void RemoveSchedule(Element view)
        {
            if (view == null || !view.IsValidObject) return;
            if (SelectedSchedules.Any(x => x.Id.IntegerValue == view.Id.IntegerValue))
                SelectedSchedules.Remove(view);
        }
    }
}
