using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace Reminder
{
    public class Report
    {
        public Int64 TaskID;
        public bool AlarmDiseable;
        public DateTime ExpirationDate;
        public DateTime DateModified;

    }

    public class ReportsColl
    {
        private ObservableCollection<Report> stations;

        [JsonProperty(PropertyName = "Report")]
        public ObservableCollection<Report> Reports
        {
            get { return this.stations; }
            set
            {
                this.stations = value;
                RaisePropertyChanged(() => Reports);
            }
        }

        private void RaisePropertyChanged(Func<ObservableCollection<Report>> coll)
        {
        }
    }
}
