using System.Collections.Generic;

namespace Reminder
{
    public class ListBoxEntry
    {
        //en C# les combo n'ont pas de itemData like vb6 :
        public int MinutesData;
        public string Caption;

        public ListBoxEntry(int key, string text)
        {
            MinutesData = key; Caption = text;
        }
        public override string ToString() { return Caption; }

        public static List<ListBoxEntry> GetDelay()
        {
            List<ListBoxEntry> delays = new List<ListBoxEntry>()
            {
#if DEBUG
                new ListBoxEntry(1, "1 minute"),
#endif
                new ListBoxEntry(5, Properties.Resources.R5),
                new ListBoxEntry(10, Properties.Resources.R10),
                new ListBoxEntry(15, Properties.Resources.R15),
                new ListBoxEntry(30, Properties.Resources.R30),
                new ListBoxEntry(60, Properties.Resources.R60),
                new ListBoxEntry(120, Properties.Resources.R120),
                new ListBoxEntry(240, Properties.Resources.R240),
                new ListBoxEntry(1440, Properties.Resources.R1440),
                new ListBoxEntry(2880, Properties.Resources.R2880),
                new ListBoxEntry(4320, Properties.Resources.R4320),
                new ListBoxEntry(5760, Properties.Resources.R5760),
                new ListBoxEntry(10080, Properties.Resources.R10080),
                new ListBoxEntry(20160, Properties.Resources.R20160)
            };
            return delays;
        }
    }
}
