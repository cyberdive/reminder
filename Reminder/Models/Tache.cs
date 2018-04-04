using System;

namespace Reminder
{
    public class Tache
    {
        public Int64 ID { get; set; }
        public Int64 IDUser { get; set; }
        public DateTime datStart { get; set; }
        public DateTime datEnd { get; set; }
        public String Caption { get; set; }
        public Int64 Color { get; set; }
        public Int64 ForeColor { get; set; }
        public String Information { get; set; }
        /// <Raison Sociale />
        public String RS { get; set; }
        public Int64 IDWhoRemind { get; set; }
        public Int64 MinuteRemind { get; set; }
        public String Echeance { get
            {
                int interval = (int)(datStart-DateTime.Now ).TotalMinutes;
                

                if (interval < 0)
                {
                    interval = Math.Abs(interval);
                    if (interval < 60)
                    {
                        return String.Format(Properties.Resources.DeadLineMin, interval);
                    }
                    else if (interval > 59 & interval < 1440)
                    {
                        interval = (interval / 60);
                        return String.Format(Properties.Resources.DeadLineHour, interval);
                    }
                    else
                    {
                        interval = ((interval / 60) / 24);
                        return String.Format(Properties.Resources.DeadLineDays, interval);
                    }
                }
                else
                {
                    return datStart.ToString();
                }
            }
        }



        public override bool Equals(Object obj)
        {
            var item  = obj as Tache;

            if (item == null)
            {
                return false;
            }

            return (ID == item.ID)
                    && (DateTime.Compare(datStart, item.datStart) == 0)
                    && (DateTime.Compare(datEnd, item.datEnd) == 0)
                    && (IDWhoRemind == item.IDWhoRemind)
                    && (IDUser == item.IDUser)
                    && (MinuteRemind == item.MinuteRemind)
                    && (IDWhoRemind == item.IDWhoRemind)
                    && (Echeance == item.Echeance);
        }

    }
}
