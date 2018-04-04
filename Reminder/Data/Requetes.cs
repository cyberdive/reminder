using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Apps72.Dev.Data;
using System;

namespace Reminder
{
    public class Requetes
    {
        public static IEnumerable<Tache> GetListTaches(int? IDUser = null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("   0 GetListTaches");
            var datafile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"reminderlog.txt");
            System.IO.File.AppendAllText(datafile, sb.ToString() + Environment.NewLine);
            sb.Clear();

            var dataService = new DataService();
            dataService.Connection();


            if (dataService.GetConnectionState != ConnectionState.Closed)
            {
                var cmd = dataService.GetDatabaseCommand();
                try
                {
                    if (IDUser == null || IDUser <= 0)
                    {
                        cmd.CommandText.AppendLine("SELECT T.ID,datStart, datEnd,Caption,T.ForeColor,T.Color,Information,RS,IDUser, ")
                                .AppendLine("    IDWhoRemind,MinuteRemind")
                                .AppendLine("    FROM task AS T ")
                                .AppendLine("        LEFT JOIN Customer ON T.IDCustomer = Customer.ID ")
                                .AppendLine("    WHERE  ")
                                .AppendLine("           MinuteRemind>0")
                                  .AppendLine("          AND  DATETIME(datStart,'-' || MinuteRemind || ' minutes') <= DATETIME('now','LOCALTIME')  ")
                                .AppendLine("          AND  NOT COALESCE(Deleted,0) ")
                                .AppendLine("    ORDER BY datStart");
                    }
                    else
                    {

                        cmd.CommandText.AppendLine("SELECT T.ID,datStart, datEnd,Caption,T.ForeColor,T.Color,Information,RS,IDUser, ")
                                .AppendLine("    IDWhoRemind,MinuteRemind")
                                .AppendLine("    FROM task AS T ")
                                .AppendLine("        LEFT JOIN Customer ON T.IDCustomer = Customer.ID ")
                                .AppendLine("    WHERE T.IDWhoRemind = @idUSer  AND ")
                                .AppendLine("           MinuteRemind>0")
                                  .AppendLine("          AND  DATETIME(datStart,'-' || MinuteRemind || ' minutes') <= DATETIME('now','LOCALTIME')  ")
                                .AppendLine("          AND  NOT COALESCE(Deleted,0) ")
                                .AppendLine("    ORDER BY datStart");
                    }

                    cmd.Parameters.AddValues(new
                    {
                        idUSer = IDUser
                    });

                    return cmd.ExecuteTable<Tache>();
                }
                catch
                {
                    return new ObservableCollection<Tache>();
                }
            }
            else
            {
                return new ObservableCollection<Tache>();
            }
           
        }

        public static  Boolean IsDBUpdated()
        {
            var dataService = new DataService();
            return dataService.IsDBUpdated();
        }

        public static IEnumerable<User> GetListUser()
        {
            var dataService = new DataService();
            dataService.Connection();
            if (dataService.GetConnectionState != ConnectionState.Closed)
            {
                using (var cmd = dataService.GetDatabaseCommand())
                {
                    cmd.CommandText.AppendLine("SELECT ID, IFNULL(UserName,NickName) AS Nom ")
                            .AppendLine("    FROM UAC ")
                            .AppendLine("    ORDER BY Nom");

                    return cmd.ExecuteTable<User>();
                }
            }
            else
            {
                return new ObservableCollection<User>();
            }
        }
    }
}
